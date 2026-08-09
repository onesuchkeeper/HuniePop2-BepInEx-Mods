using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hp2BaseMod.Analyzer;

internal static class ExpansionGenerator
{
    private static readonly SymbolDisplayFormat TypeNameFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    private static Compilation EnsureFullMetadataImport(Compilation compilation)
    {
        if (compilation is CSharpCompilation csharpCompilation &&
            csharpCompilation.Options is CSharpCompilationOptions options &&
            options.MetadataImportOptions != MetadataImportOptions.All)
        {
            return csharpCompilation.WithOptions(options.WithMetadataImportOptions(MetadataImportOptions.All));
        }

        return compilation;
    }

    public static void GenerateWithCaching(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableDictionary<SyntaxTree, ImmutableArray<MemberDeclarationSyntax>> membersBySyntaxTree)
    {
        // Default MetadataImportOptions (Public) means private/internal members of types from a
        // referenced metadata assembly (a compiled DLL like Assembly-CSharp.dll, as opposed to a
        // type compiled from this project's own source) are not imported into the symbol model
        // at all - not "found but wrong kind", genuinely invisible to GetMembers(). Fields/Methods
        // exists specifically to reach private base-game members via AccessTools, so this
        // generator needs the fuller import. This only affects our own analysis here; it does not
        // change the actual compiled output.
        compilation = EnsureFullMetadataImport(compilation);

        // Resolved once per compilation, not per class: this interface's identity doesn't depend
        // on anything class-specific, only on what's referenced by the compilation as a whole.
        var expansionCoreInterfaceType = compilation.GetTypeByMetadataName("Hp2BaseMod.IExpansionCore`1");

        var semanticModelCache = new Dictionary<SyntaxTree, SemanticModel>(membersBySyntaxTree.Count);
        var processed = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var entry in membersBySyntaxTree)
        {
            if (!semanticModelCache.TryGetValue(entry.Key, out var semanticModel))
            {
                semanticModel = compilation.GetSemanticModel(entry.Key);
                semanticModelCache[entry.Key] = semanticModel;
            }

            foreach (var memberSyntax in entry.Value)
            {
                if (memberSyntax is not ClassDeclarationSyntax classSyntax)
                {
                    continue;
                }

                var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax, context.CancellationToken) as INamedTypeSymbol;
                if (classSymbol == null || !processed.Add(classSymbol))
                {
                    continue;
                }

                var expansionAttribute = ExpansionAttributeReader.FindExpansionAttribute(classSymbol);
                if (expansionAttribute == null)
                {
                    continue;
                }

                var baseType = ExpansionAttributeReader.GetExpansionBaseType(classSymbol);
                if (baseType == null)
                {
                    continue;
                }

                if (!IsPartialClass(classSyntax))
                {
                    ReportDiagnostic(
                        context,
                        DiagnosticStrings.ID_EXPANSION_CLASS_NOT_PARTIAL,
                        "Expansion class must be partial",
                        DiagnosticStrings.MESSAGE_EXPANSION_CLASS_NOT_PARTIAL,
                        DiagnosticSeverity.Error,
                        classSyntax.Identifier.GetLocation(),
                        classSymbol.Name);
                    continue;
                }

                var attributeLocation = expansionAttribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
                    ?? classSyntax.Identifier.GetLocation();

                var hasModId = ExpansionAttributeReader.GetBoolNamedArgument(expansionAttribute, "HasModId");
                var hasOnDestroy = HasParameterlessInstanceMethod(classSymbol, "OnDestroy");
                var hasOnInit = HasParameterlessInstanceMethod(classSymbol, "OnInit");

                // Structural base-class integration: neither of these requires anything on the
                // [Expansion] attribute itself, both are discovered purely from what the
                // developer's partial class already declares as its base type.
                var hasCoreInterface = ImplementsExpansionCoreInterface(classSymbol, baseType, expansionCoreInterfaceType);
                var baseCoreConstructor = FindSingleCoreParamBaseConstructor(classSymbol.BaseType, baseType);
                var chainToBaseConstructor = baseCoreConstructor != null;

                var generatingAssembly = compilation.Assembly;
                var extraDiagnostics = new List<Diagnostic>();
                var fieldPlans = BuildFieldPlans(baseType, generatingAssembly, attributeLocation, extraDiagnostics);
                var propertyPlans = BuildPropertyPlans(baseType, generatingAssembly, attributeLocation, extraDiagnostics);
                var methodPlans = BuildMethodPlans(baseType, generatingAssembly, attributeLocation, extraDiagnostics);

                foreach (var diagnostic in extraDiagnostics)
                {
                    context.ReportDiagnostic(diagnostic);
                }

                // classSymbol here only reflects the developer's own syntax trees - this
                // generator's output has not been added to the compilation yet - so any member
                // found now is genuinely developer-authored, and its location is safe to report.
                var relativeIdType = hasModId ? compilation.GetTypeByMetadataName("Hp2BaseMod.RelativeId") : null;
                var conflicts = FindConflicts(classSymbol, baseType, relativeIdType, fieldPlans, propertyPlans, methodPlans, hasModId, hasCoreInterface);

                foreach (var conflict in conflicts)
                {
                    ReportDiagnostic(
                        context,
                        DiagnosticStrings.ID_EXPANSION_MEMBER_CONFLICTS_WITH_GENERATED,
                        "Member conflicts with generated Expansion code",
                        DiagnosticStrings.MESSAGE_EXPANSION_MEMBER_CONFLICTS_WITH_GENERATED,
                        DiagnosticSeverity.Error,
                        conflict.Location,
                        conflict.MemberName,
                        classSymbol.Name);
                }

                var skip = new HashSet<string>(conflicts.Select(c => c.Key));

                var source = GenerateSource(classSymbol, baseType, fieldPlans, propertyPlans, methodPlans, hasModId, hasOnDestroy, hasOnInit, hasCoreInterface, chainToBaseConstructor, skip);
                var fileName = BuildFileName(classSymbol);

                context.AddSource(fileName, source);
            }
        }
    }

    private static bool IsPartialClass(ClassDeclarationSyntax classSyntax)
    {
        foreach (var modifier in classSyntax.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PartialKeyword))
            {
                return true;
            }
        }

        return false;
    }

    // Hierarchy-aware deliberately: a hook like OnDestroy() may be inherited from an arbitrary
    // base class the [Expansion] partial derives from, not just declared directly on it. Using
    // classSymbol.GetMembers(name) alone (declared-only) would report "not present" for an
    // inherited hook, and the generator would then be unaware that a same-named, same-shaped
    // member already exists - the exact conflict this is trying to avoid.
    private static bool HasParameterlessInstanceMethod(INamedTypeSymbol classSymbol, string name)
    {
        foreach (var member in GetMembersInHierarchy(classSymbol, name))
        {
            if (member is IMethodSymbol method && !method.IsStatic && method.Parameters.Length == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetAccessibilityKeyword(Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.Public:
                return "public";
            case Accessibility.Internal:
                return "internal";
            case Accessibility.Protected:
                return "protected";
            case Accessibility.ProtectedOrInternal:
                return "protected internal";
            case Accessibility.ProtectedAndInternal:
                return "private protected";
            case Accessibility.Private:
                return "private";
            default:
                return "internal";
        }
    }

    private static string TrimLeadingUnderscores(string name)
    {
        var index = 0;
        while (index < name.Length && name[index] == '_')
        {
            index++;
        }

        return index == 0 ? name : name.Substring(index);
    }

    // INamedTypeSymbol.GetMembers(name) only returns members declared directly on that type, not
    // inherited ones. AccessTools.Field/Method walk the base-type chain at runtime (that's part
    // of why they're used over raw reflection for inherited private members), so our own
    // existence check needs to do the same or it will report false "not found" errors for
    // perfectly valid, correctly-spelled members that simply live on a base class.
    //
    // Deliberately using the parameterless GetMembers() plus a manual name filter here, rather
    // than the name-filtered GetMembers(name) overload: for types imported from a metadata
    // reference (a compiled DLL like Assembly-CSharp.dll, as opposed to a type compiled from this
    // project's own source), GetMembers(name) is unreliable for private members - it can return
    // an empty result for a private member that genuinely exists and is found by GetMembers().
    // baseType here is always such a metadata-imported symbol, so this matters every time.
    private static IEnumerable<ISymbol> GetMembersInHierarchy(INamedTypeSymbol type, string name)
    {
        var current = type;
        while (current != null)
        {
            foreach (var member in current.GetMembers())
            {
                if (member.Name == name)
                {
                    yield return member;
                }
            }

            current = current.BaseType;
        }
    }

    // Looks for an accessible constructor on 'baseClassType' shaped exactly like "(coreType core)"
    // - a single parameter whose type is exactly the [Expansion] attribute's base type. This is
    // deliberately structural (no attribute, no naming convention): any base class the developer
    // chooses to inherit from either has a constructor shaped this way or it doesn't, and if it
    // does, that's the constructor our generated one should be chaining to via ": base(core)".
    //
    // Returns null (and chains nothing) if: there's no base class beyond object, no constructor
    // matches, the match is private (inaccessible from a derived class), or more than one
    // constructor matches (ambiguous - safer to chain nothing than guess wrong).
    private static IMethodSymbol FindSingleCoreParamBaseConstructor(INamedTypeSymbol baseClassType, INamedTypeSymbol coreType)
    {
        if (baseClassType == null || baseClassType.SpecialType == SpecialType.System_Object)
        {
            return null;
        }

        IMethodSymbol match = null;
        foreach (var constructor in baseClassType.Constructors)
        {
            if (constructor.Parameters.Length != 1)
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, coreType))
            {
                continue;
            }

            if (constructor.DeclaredAccessibility == Accessibility.Private)
            {
                continue;
            }

            if (match != null)
            {
                // Ambiguous (e.g. two constructors somehow both matched, or unlikely partial
                // overload weirdness) - don't guess, fall back to not chaining.
                return null;
            }

            match = constructor;
        }

        return match;
    }

    // Looks anywhere in classSymbol's inheritance chain for Hp2BaseMod.IExpansionCore<TBase>,
    // where TBase is exactly the [Expansion] attribute's base type. AllInterfaces already
    // flattens the whole chain with generic substitution applied, so it doesn't matter whether
    // the interface is implemented by classSymbol itself, a direct base, or further up.
    //
    // expansionCoreInterfaceDefinition is the unbound IExpansionCore<T> symbol resolved once by
    // the caller via compilation.GetTypeByMetadataName - passed in rather than re-resolved here
    // since a missing metadata reference (interface not in scope) just means "never matches",
    // not an error.
    private static bool ImplementsExpansionCoreInterface(
        INamedTypeSymbol classSymbol,
        INamedTypeSymbol coreType,
        INamedTypeSymbol expansionCoreInterfaceDefinition)
    {
        if (expansionCoreInterfaceDefinition == null)
        {
            return false;
        }

        foreach (var iface in classSymbol.AllInterfaces)
        {
            if (!SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, expansionCoreInterfaceDefinition))
            {
                continue;
            }

            if (iface.TypeArguments.Length == 1 && SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], coreType))
            {
                return true;
            }
        }

        return false;
    }

    // True if 'type' (and, for named types, its containing-type chain and any generic type
    // arguments) is accessible from code sitting in 'generatingAssembly'. Private members are
    // never accessible from outside their declaring class; internal members are only accessible
    // from within the same assembly (no InternalsVisibleTo assumed).
    private static bool IsTypeUsableInGeneratedCode(ITypeSymbol type, IAssemblySymbol generatingAssembly)
    {
        if (type is IArrayTypeSymbol arrayType)
        {
            return IsTypeUsableInGeneratedCode(arrayType.ElementType, generatingAssembly);
        }

        if (type is not INamedTypeSymbol namedType)
        {
            return true;
        }

        foreach (var typeArgument in namedType.TypeArguments)
        {
            if (!IsTypeUsableInGeneratedCode(typeArgument, generatingAssembly))
            {
                return false;
            }
        }

        var current = namedType;
        while (current != null)
        {
            switch (current.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    break;
                case Accessibility.Internal:
                case Accessibility.ProtectedOrInternal:
                    if (!SymbolEqualityComparer.Default.Equals(current.ContainingAssembly, generatingAssembly))
                    {
                        return false;
                    }

                    break;
                default:
                    return false;
            }

            current = current.ContainingType;
        }

        return true;
    }

    private readonly struct FieldPlan
    {
        public FieldPlan(string originalName, string fieldVariableName, IFieldSymbol symbol, bool typeAccessible)
        {
            OriginalName = originalName;
            FieldVariableName = fieldVariableName;
            Symbol = symbol;
            TypeAccessible = typeAccessible;
        }

        public string OriginalName { get; }

        public string FieldVariableName { get; }

        public IFieldSymbol Symbol { get; }

        public bool TypeAccessible { get; }
    }

    private readonly struct PropertyPlan
    {
        public PropertyPlan(string originalName, string propertyVariableName, IPropertySymbol symbol, bool typeAccessible)
        {
            OriginalName = originalName;
            PropertyVariableName = propertyVariableName;
            Symbol = symbol;
            TypeAccessible = typeAccessible;
        }

        public string OriginalName { get; }

        public string PropertyVariableName { get; }

        public IPropertySymbol Symbol { get; }

        public bool TypeAccessible { get; }
    }

    private readonly struct MethodPlan
    {
        public MethodPlan(string originalName, string methodVariableName, IMethodSymbol symbol, bool signatureAccessible)
        {
            OriginalName = originalName;
            MethodVariableName = methodVariableName;
            Symbol = symbol;
            SignatureAccessible = signatureAccessible;
        }

        public string OriginalName { get; }

        public string MethodVariableName { get; }

        public IMethodSymbol Symbol { get; }

        public bool SignatureAccessible { get; }
    }

    // True for a private/protected member declared directly on the base type that is a candidate
    // for exposure: excludes anything compiler-generated (property/event backing fields, and the
    // like), since those aren't members the developer actually wrote and wrapping them would
    // just duplicate the property/event accessor that already wraps them properly.
    //
    // "protected" here covers every accessibility flavor a subclass can see that an outside
    // caller cannot: Protected, ProtectedAndInternal ("private protected"), and
    // ProtectedOrInternal ("protected internal") - all three are inaccessible to arbitrary
    // outside code the same way Private is, which is exactly the set Fields/Methods used to have
    // to be spelled out for by hand.
    private static bool IsExposableAccessibility(Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.Private:
            case Accessibility.Protected:
            case Accessibility.ProtectedAndInternal:
            case Accessibility.ProtectedOrInternal:
                return true;
            default:
                return false;
        }
    }

    // Every top-level identifier the generator itself may emit directly on the Expansion class:
    // the core/id backing pair and their properties, the internal lookup dictionary, the
    // static Get/Destroy accessors, and the two optional lifecycle hooks it looks for by name
    // (OnInit/OnDestroy). These names are reserved for the generator's own contract regardless
    // of whether a given [Expansion] actually uses every piece (e.g. "Id"/"_id" are reserved
    // even when HasModId is false) - a base type's own private/protected member sharing one of
    // these names is exactly the "OnDestroy() already exists on the Unity object" collision:
    // auto-exposing it would either shadow the real hook silently or hard-conflict with a
    // developer-written one, so it's excluded from exposure entirely rather than wrapped under
    // a mangled name. The member is still reachable by hand (AccessTools.Field/Method/Property)
    // if a developer genuinely needs it; it just doesn't get a generated wrapper.
    private static readonly HashSet<string> ReservedGeneratedMemberNames = new HashSet<string>
    {
        "_expansions",
        "_core",
        "Core",
        "_id",
        "Id",
        "OnInit",
        "OnDestroy",
        "Get",
        "Destroy",
    };

    private static bool IsReservedGeneratedMemberName(string name) => ReservedGeneratedMemberNames.Contains(name);

    // Guards against wrapping compiler-synthesized members whose names aren't legal C#
    // identifiers - most commonly lambda/local-function closure methods like
    // "<ShowDollOutline>b__158_1", but also anything else the compiler generates a non-source
    // name for. IsImplicitlyDeclared doesn't catch these: a lambda's synthesized method has real
    // syntax (the lambda body) behind it, so it isn't "implicit" in Roslyn's sense even though
    // the name itself could never appear in a wrapper's method/property declaration.
    private static bool IsValidExposableIdentifier(string name) => SyntaxFacts.IsValidIdentifier(name);

    // Declared-only (not hierarchy-walking) deliberately: exposing every private/protected member
    // of every ancestor all the way up to System.Object would bury the handful of members that
    // actually matter for a given base type under noise from unrelated framework base classes.
    // A base type with its own base class that also wants exposure gets its own [Expansion] over
    // that base class instead.
    private static List<FieldPlan> BuildFieldPlans(
        INamedTypeSymbol baseType,
        IAssemblySymbol generatingAssembly,
        Location diagnosticLocation,
        List<Diagnostic> diagnostics)
    {
        var plans = new List<FieldPlan>();

        foreach (var member in baseType.GetMembers())
        {
            if (member is not IFieldSymbol field || field.IsImplicitlyDeclared)
            {
                continue;
            }

            if (!IsExposableAccessibility(field.DeclaredAccessibility))
            {
                continue;
            }

            if (IsReservedGeneratedMemberName(field.Name))
            {
                continue;
            }

            if (!IsValidExposableIdentifier(field.Name))
            {
                continue;
            }

            var accessible = IsTypeUsableInGeneratedCode(field.Type, generatingAssembly);
            if (!accessible)
            {
                diagnostics.Add(BuildDiagnostic(
                    DiagnosticStrings.ID_EXPANSION_MEMBER_TYPE_INACCESSIBLE,
                    "Member type not accessible",
                    DiagnosticStrings.MESSAGE_EXPANSION_MEMBER_TYPE_INACCESSIBLE,
                    DiagnosticSeverity.Warning,
                    diagnosticLocation,
                    field.Name,
                    baseType.Name,
                    "FieldInfo"));
            }

            plans.Add(new FieldPlan(field.Name, "f_" + TrimLeadingUnderscores(field.Name), field, accessible));
        }

        return plans;
    }

    // Indexers are skipped - "this[...]" has no name to hang a like-named wrapper property off
    // of, and AccessTools.Property has no notion of an indexer's parameter list to disambiguate
    // by, the same ambiguity problem overloaded methods have but with no analogous fix available.
    private static List<PropertyPlan> BuildPropertyPlans(
        INamedTypeSymbol baseType,
        IAssemblySymbol generatingAssembly,
        Location diagnosticLocation,
        List<Diagnostic> diagnostics)
    {
        var plans = new List<PropertyPlan>();

        foreach (var member in baseType.GetMembers())
        {
            if (member is not IPropertySymbol property || property.IsImplicitlyDeclared || property.IsIndexer)
            {
                continue;
            }

            if (!IsExposableAccessibility(property.DeclaredAccessibility))
            {
                continue;
            }

            if (IsReservedGeneratedMemberName(property.Name))
            {
                continue;
            }

            if (!IsValidExposableIdentifier(property.Name))
            {
                continue;
            }

            var accessible = IsTypeUsableInGeneratedCode(property.Type, generatingAssembly);
            if (!accessible)
            {
                diagnostics.Add(BuildDiagnostic(
                    DiagnosticStrings.ID_EXPANSION_MEMBER_TYPE_INACCESSIBLE,
                    "Member type not accessible",
                    DiagnosticStrings.MESSAGE_EXPANSION_MEMBER_TYPE_INACCESSIBLE,
                    DiagnosticSeverity.Warning,
                    diagnosticLocation,
                    property.Name,
                    baseType.Name,
                    "PropertyInfo"));
            }

            plans.Add(new PropertyPlan(property.Name, "p_" + TrimLeadingUnderscores(property.Name), property, accessible));
        }

        return plans;
    }

    private static List<MethodPlan> BuildMethodPlans(
        INamedTypeSymbol baseType,
        IAssemblySymbol generatingAssembly,
        Location diagnosticLocation,
        List<Diagnostic> diagnostics)
    {
        var plans = new List<MethodPlan>();

        // Grouped by name (still preserving declaration order between groups) purely so that
        // overloads of the same private/protected method name keep getting the same "_1", "_2"...
        // disambiguation a single explicit Methods entry used to produce.
        var byName = new Dictionary<string, List<IMethodSymbol>>();
        var nameOrder = new List<string>();

        foreach (var member in baseType.GetMembers())
        {
            if (member is not IMethodSymbol method ||
                method.MethodKind != MethodKind.Ordinary ||
                method.IsImplicitlyDeclared)
            {
                continue;
            }

            if (!IsExposableAccessibility(method.DeclaredAccessibility))
            {
                continue;
            }

            if (IsReservedGeneratedMemberName(method.Name))
            {
                continue;
            }

            if (!IsValidExposableIdentifier(method.Name))
            {
                continue;
            }

            if (!byName.TryGetValue(method.Name, out var overloads))
            {
                overloads = new List<IMethodSymbol>();
                byName[method.Name] = overloads;
                nameOrder.Add(method.Name);
            }

            overloads.Add(method);
        }

        foreach (var methodName in nameOrder)
        {
            var overloads = byName[methodName];

            // Deterministic ordering across builds: by parameter count, then parameter type names.
            overloads.Sort((a, b) =>
            {
                var countCompare = a.Parameters.Length.CompareTo(b.Parameters.Length);
                if (countCompare != 0)
                {
                    return countCompare;
                }

                var aKey = string.Join(",", a.Parameters.Select(p => p.Type.ToDisplayString(TypeNameFormat)));
                var bKey = string.Join(",", b.Parameters.Select(p => p.Type.ToDisplayString(TypeNameFormat)));
                return string.CompareOrdinal(aKey, bKey);
            });

            var trimmedName = TrimLeadingUnderscores(methodName);
            var needsSuffix = overloads.Count > 1;

            for (var i = 0; i < overloads.Count; i++)
            {
                var method = overloads[i];
                var variableName = needsSuffix ? "m_" + trimmedName + "_" + (i + 1) : "m_" + trimmedName;

                var signatureAccessible = IsTypeUsableInGeneratedCode(method.ReturnType, generatingAssembly);
                if (signatureAccessible)
                {
                    foreach (var parameter in method.Parameters)
                    {
                        if (!IsTypeUsableInGeneratedCode(parameter.Type, generatingAssembly))
                        {
                            signatureAccessible = false;
                            break;
                        }
                    }
                }

                if (!signatureAccessible)
                {
                    diagnostics.Add(BuildDiagnostic(
                        DiagnosticStrings.ID_EXPANSION_MEMBER_TYPE_INACCESSIBLE,
                        "Member type not accessible",
                        DiagnosticStrings.MESSAGE_EXPANSION_MEMBER_TYPE_INACCESSIBLE,
                        DiagnosticSeverity.Warning,
                        diagnosticLocation,
                        methodName,
                        baseType.Name,
                        "MethodInfo"));
                }

                plans.Add(new MethodPlan(methodName, variableName, method, signatureAccessible));
            }
        }

        return plans;
    }

    private readonly struct MemberConflict
    {
        public MemberConflict(string key, string memberName, Location location)
        {
            Key = key;
            MemberName = memberName;
            Location = location;
        }

        public string Key { get; }

        public string MemberName { get; }

        public Location Location { get; }
    }

    private static List<MemberConflict> FindConflicts(
        INamedTypeSymbol classSymbol,
        INamedTypeSymbol baseType,
        INamedTypeSymbol relativeIdType,
        List<FieldPlan> fieldPlans,
        List<PropertyPlan> propertyPlans,
        List<MethodPlan> methodPlans,
        bool hasModId,
        bool hasCoreInterface)
    {
        var conflicts = new List<MemberConflict>();

        AddUniqueMemberConflict(classSymbol, "_expansions", "_expansions", conflicts);

        // When hasCoreInterface is true, the generator deliberately never emits "_core"/"Core" -
        // it reuses the inherited IExpansionCore<T>.Core instead - so an inherited member with
        // either name is the intended reuse, not a conflict. When false, these are generated
        // as usual, so any existing member (inherited or not) genuinely would collide.
        if (!hasCoreInterface)
        {
            AddUniqueMemberConflict(classSymbol, "_core", "_core", conflicts);
            AddUniqueMemberConflict(classSymbol, "Core", "prop_core", conflicts);
        }

        AddConstructorConflict(classSymbol, baseType, relativeIdType, hasModId, conflicts);
        AddMethodConflict(classSymbol, "Get", new[] { baseType }, "get_base", conflicts);
        AddMethodConflict(classSymbol, "Destroy", new[] { baseType }, "destroy_base", conflicts);
        AddInstanceDestroyConflict(classSymbol, conflicts);

        if (hasModId && relativeIdType != null)
        {
            AddUniqueMemberConflict(classSymbol, "_id", "_id", conflicts);
            AddUniqueMemberConflict(classSymbol, "Id", "prop_id", conflicts);
            AddMethodConflict(classSymbol, "Get", new[] { relativeIdType }, "get_relid", conflicts);
            AddMethodConflict(classSymbol, "Destroy", new[] { relativeIdType }, "destroy_relid", conflicts);
        }

        foreach (var fieldPlan in fieldPlans)
        {
            AddUniqueMemberConflict(classSymbol, fieldPlan.FieldVariableName, "field:" + fieldPlan.OriginalName, conflicts);
            AddUniqueMemberConflict(classSymbol, fieldPlan.OriginalName, "fieldprop:" + fieldPlan.OriginalName, conflicts);
        }

        foreach (var propertyPlan in propertyPlans)
        {
            AddUniqueMemberConflict(classSymbol, propertyPlan.PropertyVariableName, "property:" + propertyPlan.OriginalName, conflicts);
            AddUniqueMemberConflict(classSymbol, propertyPlan.OriginalName, "propwrap:" + propertyPlan.OriginalName, conflicts);
        }

        foreach (var methodPlan in methodPlans)
        {
            AddUniqueMemberConflict(classSymbol, methodPlan.MethodVariableName, "methodfield:" + methodPlan.MethodVariableName, conflicts);

            if (methodPlan.SignatureAccessible)
            {
                var parameterTypes = methodPlan.Symbol.Parameters.Select(p => p.Type).OfType<INamedTypeSymbol>().ToArray();
                if (parameterTypes.Length == methodPlan.Symbol.Parameters.Length)
                {
                    AddMethodConflict(classSymbol, methodPlan.Symbol.Name, parameterTypes, "wrapper:" + methodPlan.MethodVariableName, conflicts);
                }
            }
        }

        return conflicts;
    }

    // For identifiers that must be unique regardless of member kind (fields, the properties we
    // generate, etc.) - any existing member with that name, of any kind, is a genuine conflict.
    // Hierarchy-aware: an inherited member (from an arbitrary base class the partial declares)
    // would be silently hidden (CS0108) by a generated member of the same name just as surely as
    // a directly-declared one would collide, so it needs to be reported the same way.
    private static void AddUniqueMemberConflict(INamedTypeSymbol classSymbol, string memberName, string key, List<MemberConflict> conflicts)
    {
        foreach (var member in GetMembersInHierarchy(classSymbol, memberName))
        {
            var location = member.Locations.FirstOrDefault() ?? Location.None;
            conflicts.Add(new MemberConflict(key, memberName, location));
            return;
        }
    }

    private static void AddConstructorConflict(INamedTypeSymbol classSymbol, INamedTypeSymbol baseType, INamedTypeSymbol relativeIdType, bool hasModId, List<MemberConflict> conflicts)
    {
        var expectedParameterCount = hasModId ? 2 : 1;

        foreach (var constructor in classSymbol.Constructors)
        {
            if (constructor.Parameters.Length != expectedParameterCount)
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, baseType))
            {
                continue;
            }

            if (hasModId && (relativeIdType == null || !SymbolEqualityComparer.Default.Equals(constructor.Parameters[1].Type, relativeIdType)))
            {
                continue;
            }

            var location = constructor.Locations.FirstOrDefault() ?? Location.None;
            var signature = hasModId
                ? classSymbol.Name + "(" + baseType.Name + ", RelativeId)"
                : classSymbol.Name + "(" + baseType.Name + ")";
            conflicts.Add(new MemberConflict("ctor", signature, location));
            return;
        }
    }

    // Hierarchy-aware for the same reason as AddUniqueMemberConflict above: a matching method
    // inherited from an arbitrary base class would be hidden by a generated one just as surely
    // as a directly-declared one.
    private static void AddMethodConflict(INamedTypeSymbol classSymbol, string methodName, INamedTypeSymbol[] parameterTypes, string key, List<MemberConflict> conflicts)
    {
        foreach (var member in GetMembersInHierarchy(classSymbol, methodName))
        {
            if (member is not IMethodSymbol method || method.MethodKind != MethodKind.Ordinary)
            {
                continue;
            }

            if (method.Parameters.Length != parameterTypes.Length)
            {
                continue;
            }

            var isMatch = true;
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                if (!SymbolEqualityComparer.Default.Equals(method.Parameters[i].Type, parameterTypes[i]))
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
            {
                var location = method.Locations.FirstOrDefault() ?? Location.None;
                conflicts.Add(new MemberConflict(key, methodName, location));
                return;
            }
        }
    }

    private static void AddInstanceDestroyConflict(INamedTypeSymbol classSymbol, List<MemberConflict> conflicts)
    {
        foreach (var member in GetMembersInHierarchy(classSymbol, "Destroy"))
        {
            if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary } method && !method.IsStatic && method.Parameters.Length == 0)
            {
                var location = method.Locations.FirstOrDefault() ?? Location.None;
                conflicts.Add(new MemberConflict("destroy_instance", "Destroy()", location));
                return;
            }
        }
    }

    private static string GenerateSource(
        INamedTypeSymbol classSymbol,
        INamedTypeSymbol baseType,
        List<FieldPlan> fieldPlans,
        List<PropertyPlan> propertyPlans,
        List<MethodPlan> methodPlans,
        bool hasModId,
        bool hasOnDestroy,
        bool hasOnInit,
        bool hasCoreInterface,
        bool chainToBaseConstructor,
        HashSet<string> skip)
    {
        var baseTypeName = baseType.ToDisplayString(TypeNameFormat);
        var expansionTypeName = classSymbol.Name;
        var extensionClassName = baseType.Name + "Exp_Ext";
        var accessibility = GetAccessibilityKeyword(classSymbol.DeclaredAccessibility);
        var namespaceName = classSymbol.ContainingNamespace?.ToDisplayString();
        var hasNamespace = !string.IsNullOrEmpty(namespaceName) && namespaceName != "<global namespace>";
        var indent = hasNamespace ? "    " : "";

        var sb = new StringBuilder(4096);
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Reflection;");
        sb.AppendLine("using HarmonyLib;");
        sb.AppendLine();

        if (hasNamespace)
        {
            sb.Append("namespace ").AppendLine(namespaceName);
            sb.AppendLine("{");
        }

        AppendExtensionClass(sb, indent, accessibility, extensionClassName, baseTypeName, expansionTypeName, hasModId);
        sb.AppendLine();
        AppendExpansionClass(sb, indent, accessibility, expansionTypeName, baseTypeName, fieldPlans, propertyPlans, methodPlans, hasModId, hasOnDestroy, hasOnInit, hasCoreInterface, chainToBaseConstructor, skip);

        if (hasNamespace)
        {
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    private static void AppendExtensionClass(
        StringBuilder sb,
        string indent,
        string accessibility,
        string extensionClassName,
        string baseTypeName,
        string expansionTypeName,
        bool hasModId)
    {
        sb.Append(indent).Append(accessibility).Append(" static class ").AppendLine(extensionClassName);
        sb.Append(indent).AppendLine("{");
        sb.Append(indent).Append("    ").Append(accessibility).Append(" static ").Append(expansionTypeName).Append(" GetExpansion(this ").Append(baseTypeName).AppendLine(" core)");
        sb.Append(indent).Append("        => ").Append(expansionTypeName).AppendLine(".Get(core);");
        sb.AppendLine();
        sb.Append(indent).Append("    ").Append(accessibility).Append(" static void DestroyExpansion(this ").Append(baseTypeName).AppendLine(" core)");
        sb.Append(indent).Append("        => ").Append(expansionTypeName).AppendLine(".Destroy(core);");

        if (hasModId)
        {
            sb.AppendLine();
            sb.Append(indent).Append("    ").Append(accessibility).Append(" static global::Hp2BaseMod.RelativeId ModId(this ").Append(baseTypeName).AppendLine(" def)");
            sb.Append(indent).AppendLine("        => global::Hp2BaseMod.ModInterface.Data.GetDataId(def);");
        }

        sb.Append(indent).AppendLine("}");
    }

    private static void AppendExpansionClass(
        StringBuilder sb,
        string indent,
        string accessibility,
        string expansionTypeName,
        string baseTypeName,
        List<FieldPlan> fieldPlans,
        List<PropertyPlan> propertyPlans,
        List<MethodPlan> methodPlans,
        bool hasModId,
        bool hasOnDestroy,
        bool hasOnInit,
        bool hasCoreInterface,
        bool chainToBaseConstructor,
        HashSet<string> skip)
    {
        var dictionaryKeyType = hasModId ? "RelativeId" : baseTypeName;

        // The single expression every generated accessor uses to reach the core reference.
        // Normally that's our own private "_core" field. When the base class already implements
        // IExpansionCore<T>, there is no "_core" field here at all - "Core" (the inherited
        // property) is the only copy, so every generated site targets that instead.
        var coreExpr = hasCoreInterface ? "Core" : "_core";

        sb.Append(indent).Append(accessibility).Append(" sealed partial class ").AppendLine(expansionTypeName);
        sb.Append(indent).AppendLine("{");

        // Public properties with their backing fields directly beneath, per member ordering.
        // Skipped entirely when hasCoreInterface: the base class already provides both, and
        // generating our own here would be a second, shadowing copy of the same reference.
        if (!hasCoreInterface)
        {
            if (!skip.Contains("prop_core"))
            {
                sb.Append(indent).Append("    public ").Append(baseTypeName).AppendLine(" Core => _core;");
            }

            if (!skip.Contains("_core"))
            {
                sb.Append(indent).Append("    private readonly ").Append(baseTypeName).AppendLine(" _core;");
            }
        }

        if (hasModId)
        {
            if (!skip.Contains("prop_id"))
            {
                sb.AppendLine();
                sb.Append(indent).AppendLine("    public RelativeId Id => _id;");
            }

            if (!skip.Contains("_id"))
            {
                sb.Append(indent).AppendLine("    private readonly RelativeId _id;");
            }
        }

        // Other private fields.
        if (!skip.Contains("_expansions"))
        {
            sb.AppendLine();
            sb.Append(indent).Append("    private static readonly Dictionary<").Append(dictionaryKeyType).Append(", ").Append(expansionTypeName).Append("> _expansions = new Dictionary<").Append(dictionaryKeyType).Append(", ").Append(expansionTypeName).AppendLine(">();");
        }

        foreach (var fieldPlan in fieldPlans)
        {
            AppendFieldAccessor(sb, indent, baseTypeName, fieldPlan, coreExpr, skip);
        }

        foreach (var propertyPlan in propertyPlans)
        {
            AppendPropertyAccessor(sb, indent, baseTypeName, propertyPlan, coreExpr, skip);
        }

        foreach (var methodPlan in methodPlans)
        {
            AppendMethodAccessor(sb, indent, baseTypeName, methodPlan, coreExpr, skip);
        }

        // Constructor. The public signature ("(core)" or "(core, id)") is unaffected by base-
        // class integration - it's always what static Get/the extension methods need to call.
        // What changes is the body: chainToBaseConstructor adds ": base(core)" when the base
        // class has an accessible constructor shaped exactly like that, and hasCoreInterface
        // drops the "_core = core ..." assignment since there's no "_core" field here to assign
        // to (the base class already stores it, reachable via the inherited "Core").
        if (!skip.Contains("ctor"))
        {
            sb.AppendLine();

            var baseChain = chainToBaseConstructor ? " : base(core)" : "";

            if (hasModId)
            {
                sb.Append(indent).Append("    private ").Append(expansionTypeName).Append("(").Append(baseTypeName).Append(" core, RelativeId id)").AppendLine(baseChain);
                sb.Append(indent).AppendLine("    {");

                if (!hasCoreInterface)
                {
                    sb.Append(indent).AppendLine("        _core = core ?? throw new ArgumentNullException(nameof(core));");
                }

                sb.Append(indent).AppendLine("        _id = id;");
            }
            else
            {
                sb.Append(indent).Append("    private ").Append(expansionTypeName).Append("(").Append(baseTypeName).Append(" core)").AppendLine(baseChain);
                sb.Append(indent).AppendLine("    {");

                if (!hasCoreInterface)
                {
                    sb.Append(indent).AppendLine("        _core = core ?? throw new ArgumentNullException(nameof(core));");
                }
            }

            if (hasOnInit)
            {
                sb.Append(indent).AppendLine("        OnInit();");
            }

            sb.Append(indent).AppendLine("    }");
        }

        sb.AppendLine();

        if (hasModId)
        {
            AppendModIdKeyedAccessors(sb, indent, expansionTypeName, baseTypeName, hasOnDestroy, skip);
        }
        else
        {
            AppendCoreKeyedAccessors(sb, indent, expansionTypeName, baseTypeName, hasOnDestroy, skip);
        }

        if (!skip.Contains("destroy_instance"))
        {
            sb.AppendLine();
            sb.Append(indent).Append("    public void Destroy() => Destroy(").Append(coreExpr).AppendLine(");");
        }

        sb.Append(indent).AppendLine("}");
    }

    private static void AppendFieldAccessor(
        StringBuilder sb,
        string indent,
        string baseTypeName,
        FieldPlan fieldPlan,
        string coreExpr,
        HashSet<string> skip)
    {
        if (!skip.Contains("field:" + fieldPlan.OriginalName))
        {
            sb.AppendLine();
            sb.Append(indent).Append("    private static readonly FieldInfo ").Append(fieldPlan.FieldVariableName).Append(" = AccessTools.Field(typeof(").Append(baseTypeName).Append("), \"").Append(fieldPlan.OriginalName).AppendLine("\");");
        }

        if (!fieldPlan.TypeAccessible || skip.Contains("fieldprop:" + fieldPlan.OriginalName))
        {
            return;
        }

        var fieldTypeName = fieldPlan.Symbol.Type.ToDisplayString(TypeNameFormat);
        var isStatic = fieldPlan.Symbol.IsStatic;
        var target = isStatic ? "null" : coreExpr;

        sb.AppendLine();
        sb.Append(indent).Append("    public ").Append(isStatic ? "static " : "").Append(fieldTypeName).Append(" ").AppendLine(fieldPlan.OriginalName);
        sb.Append(indent).AppendLine("    {");
        sb.Append(indent).Append("        get => (").Append(fieldTypeName).Append(")").Append(fieldPlan.FieldVariableName).Append(".GetValue(").Append(target).AppendLine(");");
        sb.Append(indent).Append("        set => ").Append(fieldPlan.FieldVariableName).Append(".SetValue(").Append(target).AppendLine(", value);");
        sb.Append(indent).AppendLine("    }");
    }

    private static void AppendPropertyAccessor(
        StringBuilder sb,
        string indent,
        string baseTypeName,
        PropertyPlan propertyPlan,
        string coreExpr,
        HashSet<string> skip)
    {
        if (!skip.Contains("property:" + propertyPlan.OriginalName))
        {
            sb.AppendLine();
            sb.Append(indent).Append("    private static readonly PropertyInfo ").Append(propertyPlan.PropertyVariableName).Append(" = AccessTools.Property(typeof(").Append(baseTypeName).Append("), \"").Append(propertyPlan.OriginalName).AppendLine("\");");
        }

        if (!propertyPlan.TypeAccessible || skip.Contains("propwrap:" + propertyPlan.OriginalName))
        {
            return;
        }

        var property = propertyPlan.Symbol;
        var propertyTypeName = property.Type.ToDisplayString(TypeNameFormat);
        var isStatic = property.IsStatic;
        var target = isStatic ? "null" : coreExpr;

        // Only emit the accessors the original property actually has - a get-only source
        // property should not gain a settable wrapper (and vice versa for set-only/init-only).
        var hasGetter = property.GetMethod != null;
        var hasSetter = property.SetMethod != null;

        sb.AppendLine();
        sb.Append(indent).Append("    public ").Append(isStatic ? "static " : "").Append(propertyTypeName).Append(" ").AppendLine(propertyPlan.OriginalName);
        sb.Append(indent).AppendLine("    {");

        if (hasGetter)
        {
            sb.Append(indent).Append("        get => (").Append(propertyTypeName).Append(")").Append(propertyPlan.PropertyVariableName).Append(".GetValue(").Append(target).AppendLine(");");
        }

        if (hasSetter)
        {
            sb.Append(indent).Append("        set => ").Append(propertyPlan.PropertyVariableName).Append(".SetValue(").Append(target).AppendLine(", value);");
        }

        sb.Append(indent).AppendLine("    }");
    }

    private static void AppendMethodAccessor(
        StringBuilder sb,
        string indent,
        string baseTypeName,
        MethodPlan methodPlan,
        string coreExpr,
        HashSet<string> skip)
    {
        var method = methodPlan.Symbol;

        if (!skip.Contains("methodfield:" + methodPlan.MethodVariableName))
        {
            sb.AppendLine();
            sb.Append(indent).Append("    private static readonly MethodInfo ").Append(methodPlan.MethodVariableName).Append(" = AccessTools.Method(typeof(").Append(baseTypeName).Append("), \"").Append(methodPlan.OriginalName).Append("\", ").Append(BuildParameterTypeArrayExpression(method)).AppendLine(");");
        }

        if (!methodPlan.SignatureAccessible || skip.Contains("wrapper:" + methodPlan.MethodVariableName))
        {
            return;
        }

        AppendMethodWrapper(sb, indent, method, methodPlan.MethodVariableName, coreExpr);
    }

    private static string BuildParameterTypeArrayExpression(IMethodSymbol method)
    {
        if (method.Parameters.Length == 0)
        {
            return "new Type[] { }";
        }

        var entries = new List<string>(method.Parameters.Length);
        foreach (var parameter in method.Parameters)
        {
            var typeName = parameter.Type.ToDisplayString(TypeNameFormat);
            entries.Add(parameter.RefKind == RefKind.None
                ? "typeof(" + typeName + ")"
                : "typeof(" + typeName + ").MakeByRefType()");
        }

        return "new Type[] { " + string.Join(", ", entries) + " }";
    }

    private static string GetParameterModifierKeyword(RefKind refKind)
    {
        switch (refKind)
        {
            case RefKind.Ref:
                return "ref ";
            case RefKind.Out:
                return "out ";
            case RefKind.In:
                return "in ";
            default:
                return "";
        }
    }

    // Instance target methods invoke against the core reference implicitly (coreExpr - either
    // our own "_core" field or the base class's inherited "Core" property, see coreExpr in
    // AppendExpansionClass) rather than taking an explicit instance parameter, since the
    // Expansion already wraps exactly one core object. Static target methods need no instance at
    // all. ref/out parameters are round-tripped through the args array MethodInfo.Invoke uses:
    // 'out' parameters start as default(T) (an 'out' parameter can't be read before it's
    // assigned, even to seed a placeholder), 'ref' start as the caller's current value, and both
    // are read back from the array afterward.
    private static void AppendMethodWrapper(
        StringBuilder sb,
        string indent,
        IMethodSymbol method,
        string fieldVariableName,
        string coreExpr)
    {
        var returnTypeName = method.ReturnsVoid ? "void" : method.ReturnType.ToDisplayString(TypeNameFormat);
        var isStatic = method.IsStatic;

        var parameterDeclarations = new List<string>(method.Parameters.Length);
        foreach (var parameter in method.Parameters)
        {
            var typeName = parameter.Type.ToDisplayString(TypeNameFormat);
            parameterDeclarations.Add(GetParameterModifierKeyword(parameter.RefKind) + typeName + " " + parameter.Name);
        }

        sb.AppendLine();
        sb.Append(indent).Append("    public ").Append(isStatic ? "static " : "").Append(returnTypeName).Append(" ").Append(method.Name).Append("(").Append(string.Join(", ", parameterDeclarations)).AppendLine(")");
        sb.Append(indent).AppendLine("    {");

        sb.Append(indent).Append("        var args = new object[] { ");
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            var parameter = method.Parameters[i];
            if (parameter.RefKind == RefKind.Out)
            {
                sb.Append("default(").Append(parameter.Type.ToDisplayString(TypeNameFormat)).Append(")");
            }
            else
            {
                sb.Append(parameter.Name);
            }
        }

        sb.AppendLine(" };");

        var target = isStatic ? "null" : coreExpr;

        if (method.ReturnsVoid)
        {
            sb.Append(indent).Append("        ").Append(fieldVariableName).Append(".Invoke(").Append(target).AppendLine(", args);");
        }
        else
        {
            sb.Append(indent).Append("        var result = ").Append(fieldVariableName).Append(".Invoke(").Append(target).AppendLine(", args);");
        }

        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var parameter = method.Parameters[i];
            if (parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.Out)
            {
                sb.Append(indent).Append("        ").Append(parameter.Name).Append(" = (").Append(parameter.Type.ToDisplayString(TypeNameFormat)).Append(")args[").Append(i).AppendLine("];");
            }
        }

        if (!method.ReturnsVoid)
        {
            sb.Append(indent).Append("        return (").Append(returnTypeName).AppendLine(")result;");
        }

        sb.Append(indent).AppendLine("    }");
    }

    // Dictionary is keyed by the core reference itself: Get can create-or-fetch directly against
    // the dictionary, and Destroy only ever needs the core reference too.
    private static void AppendCoreKeyedAccessors(
        StringBuilder sb,
        string indent,
        string expansionTypeName,
        string baseTypeName,
        bool hasOnDestroy,
        HashSet<string> skip)
    {
        if (!skip.Contains("get_base"))
        {
            sb.Append(indent).Append("    public static ").Append(expansionTypeName).Append(" Get(").Append(baseTypeName).AppendLine(" core)");
            sb.Append(indent).AppendLine("    {");
            sb.Append(indent).AppendLine("        if (!_expansions.TryGetValue(core, out var expansion))");
            sb.Append(indent).AppendLine("        {");
            sb.Append(indent).Append("            expansion = new ").Append(expansionTypeName).AppendLine("(core);");
            sb.Append(indent).AppendLine("            _expansions[core] = expansion;");
            sb.Append(indent).AppendLine("        }");
            sb.AppendLine();
            sb.Append(indent).AppendLine("        return expansion;");
            sb.Append(indent).AppendLine("    }");
            sb.AppendLine();
        }

        if (skip.Contains("destroy_base"))
        {
            return;
        }

        sb.Append(indent).Append("    public static void Destroy(").Append(baseTypeName).AppendLine(" core)");
        sb.Append(indent).AppendLine("    {");

        if (hasOnDestroy)
        {
            sb.Append(indent).AppendLine("        if (_expansions.TryGetValue(core, out var expansion))");
            sb.Append(indent).AppendLine("        {");
            sb.Append(indent).AppendLine("            expansion.OnDestroy();");
            sb.Append(indent).AppendLine("            _expansions.Remove(core);");
            sb.Append(indent).AppendLine("        }");
        }
        else
        {
            sb.Append(indent).AppendLine("        _expansions.Remove(core);");
        }

        sb.Append(indent).AppendLine("    }");
    }

    // Dictionary is keyed by RelativeId. Get(BaseType) still needs the core reference to
    // construct a new instance, so it computes the id via ModId() and uses that as the key.
    // Get(RelativeId) can only ever fetch an existing entry - there is no core reference to
    // construct from an id alone - so it returns null when nothing has been created yet.
    private static void AppendModIdKeyedAccessors(
        StringBuilder sb,
        string indent,
        string expansionTypeName,
        string baseTypeName,
        bool hasOnDestroy,
        HashSet<string> skip)
    {
        if (!skip.Contains("get_base"))
        {
            sb.Append(indent).Append("    public static ").Append(expansionTypeName).Append(" Get(").Append(baseTypeName).AppendLine(" def)");
            sb.Append(indent).AppendLine("    {");
            sb.Append(indent).AppendLine("        var id = def.ModId();");
            sb.Append(indent).AppendLine("        if (!_expansions.TryGetValue(id, out var expansion))");
            sb.Append(indent).AppendLine("        {");
            sb.Append(indent).Append("            expansion = new ").Append(expansionTypeName).AppendLine("(def, id);");
            sb.Append(indent).AppendLine("            _expansions[id] = expansion;");
            sb.Append(indent).AppendLine("        }");
            sb.AppendLine();
            sb.Append(indent).AppendLine("        return expansion;");
            sb.Append(indent).AppendLine("    }");
            sb.AppendLine();
        }

        if (!skip.Contains("get_relid"))
        {
            sb.Append(indent).Append("    public static ").Append(expansionTypeName).AppendLine("? Get(RelativeId id)");
            sb.Append(indent).AppendLine("        => _expansions.TryGetValue(id, out var expansion) ? expansion : null;");
            sb.AppendLine();
        }

        if (!skip.Contains("destroy_base"))
        {
            sb.Append(indent).Append("    public static void Destroy(").Append(baseTypeName).AppendLine(" def)");
            sb.Append(indent).AppendLine("        => Destroy(def.ModId());");
            sb.AppendLine();
        }

        if (skip.Contains("destroy_relid"))
        {
            return;
        }

        sb.Append(indent).AppendLine("    public static void Destroy(RelativeId id)");
        sb.Append(indent).AppendLine("    {");

        if (hasOnDestroy)
        {
            sb.Append(indent).AppendLine("        if (_expansions.TryGetValue(id, out var expansion))");
            sb.Append(indent).AppendLine("        {");
            sb.Append(indent).AppendLine("            expansion.OnDestroy();");
            sb.Append(indent).AppendLine("            _expansions.Remove(id);");
            sb.Append(indent).AppendLine("        }");
        }
        else
        {
            sb.Append(indent).AppendLine("        _expansions.Remove(id);");
        }

        sb.Append(indent).AppendLine("    }");
    }

    private static string BuildFileName(INamedTypeSymbol classSymbol)
    {
        var fullyQualifiedName = classSymbol.ToDisplayString(TypeNameFormat);
        var fileName = fullyQualifiedName + ".Expansion.g.cs";
        return fileName.Replace('<', '_').Replace('>', '_');
    }

    private static Diagnostic BuildDiagnostic(
        string id,
        string title,
        string messageFormat,
        DiagnosticSeverity severity,
        Location location,
        params object[] messageArgs)
    {
        var descriptor = new DiagnosticDescriptor(
            id: id,
            title: title,
            messageFormat: messageFormat,
            category: "Usage",
            defaultSeverity: severity,
            isEnabledByDefault: true);

        return Diagnostic.Create(descriptor, location, messageArgs);
    }

    private static void ReportDiagnostic(
        SourceProductionContext context,
        string id,
        string title,
        string messageFormat,
        DiagnosticSeverity severity,
        Location location,
        params object[] messageArgs)
    {
        context.ReportDiagnostic(BuildDiagnostic(id, title, messageFormat, severity, location, messageArgs));
    }
}