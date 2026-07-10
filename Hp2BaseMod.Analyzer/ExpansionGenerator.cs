using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hp2BaseMod.Analyzer;

// Generates the shared half of every [Expansion] partial class: the "{Base}Exp_Ext" extension
// class (GetExpansion/DestroyExpansion, and ModId when HasModId is set), the "Core"/"Id"
// properties and their backing fields, the private constructor, the static Get/Destroy
// accessors, and typed accessors for every entry in Fields/Methods.
//
// Fields: each entry gets a cached FieldInfo plus a private property (named exactly like the
// original field) that reads/writes through it.
//
// Methods: each entry is looked up on the base type; every overload found gets its own cached
// MethodInfo (disambiguated with explicit parameter types, since AccessTools.Method(type, name)
// alone throws AmbiguousMatchException at runtime when more than one overload exists) plus a
// private wrapper method with the real parameter/return types when possible. ref/out parameters
// are threaded through MethodInfo.Invoke's args array and read back afterward. If a parameter or
// return type isn't accessible from the generated code (private/inaccessible-internal), that
// overload falls back to just the raw MethodInfo with a warning - no wrapper is emitted for it.
//
// A missing Fields/Methods entry (no such member on the base type at all) is a build error
// pointing at the [Expansion] attribute, not a silent null MethodInfo/FieldInfo discovered later
// at runtime.
//
// Both OnInit() and OnDestroy() are optional on the developer's half of the partial class:
//   - OnInit() is called at the end of the constructor if present.
//   - OnDestroy() is called before the expansion is removed from the lookup if present;
//     otherwise Destroy just removes the entry directly.
//
// The generated half always mirrors the developer half's namespace and declared accessibility
// (public/internal/etc.), rather than assuming "public".
//
// HasModId only changes what the lookup dictionary is keyed by, not whether "Core"/"_core"
// exists:
//   - HasModId = false: dictionary keyed by the core object reference itself.
//   - HasModId = true: dictionary keyed by RelativeId (per the RelativeId guidance in the coding
//     standards for persistent game data), computed from "_core" via the generated ModId()
//     extension method. "Core"/"_core" and the constructor are generated either way; "Id"/"_id"
//     are additionally generated for the RelativeId.
//
// Before generating, the developer's half of the partial class is checked for any member that
// would collide with a piece we're about to emit (same name/parameter shape). Each collision is
// reported at the developer's own declaration - not the generated file - and that specific piece
// is skipped, so the compiler's own duplicate-member error (which would otherwise point at the
// hidden generated file) never occurs.
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

                var fields = ExpansionAttributeReader.GetStringArrayNamedArgument(expansionAttribute, "Fields");
                var methods = ExpansionAttributeReader.GetStringArrayNamedArgument(expansionAttribute, "Methods");
                var hasModId = ExpansionAttributeReader.GetBoolNamedArgument(expansionAttribute, "HasModId");
                var hasOnDestroy = HasParameterlessInstanceMethod(classSymbol, "OnDestroy");
                var hasOnInit = HasParameterlessInstanceMethod(classSymbol, "OnInit");

                var generatingAssembly = compilation.Assembly;
                var extraDiagnostics = new List<Diagnostic>();
                var fieldPlans = BuildFieldPlans(baseType, fields, generatingAssembly, attributeLocation, extraDiagnostics);
                var methodPlans = BuildMethodPlans(baseType, methods, generatingAssembly, attributeLocation, extraDiagnostics);

                foreach (var diagnostic in extraDiagnostics)
                {
                    context.ReportDiagnostic(diagnostic);
                }

                // classSymbol here only reflects the developer's own syntax trees - this
                // generator's output has not been added to the compilation yet - so any member
                // found now is genuinely developer-authored, and its location is safe to report.
                var relativeIdType = hasModId ? compilation.GetTypeByMetadataName("Hp2BaseMod.RelativeId") : null;
                var conflicts = FindConflicts(classSymbol, baseType, relativeIdType, fieldPlans, methodPlans, hasModId);

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

                var source = GenerateSource(classSymbol, baseType, fieldPlans, methodPlans, hasModId, hasOnDestroy, hasOnInit, skip);
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

    private static bool HasParameterlessInstanceMethod(INamedTypeSymbol classSymbol, string name)
    {
        foreach (var member in classSymbol.GetMembers(name))
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

    private static string DescribeSymbolKind(ISymbol symbol)
    {
        switch (symbol)
        {
            case IFieldSymbol:
                return "field";
            case IPropertySymbol:
                return "property";
            case IEventSymbol:
                return "event";
            case IMethodSymbol method:
                switch (method.MethodKind)
                {
                    case MethodKind.ExplicitInterfaceImplementation:
                        return "explicit interface implementation";
                    case MethodKind.PropertyGet:
                    case MethodKind.PropertySet:
                        return "property accessor";
                    case MethodKind.EventAdd:
                    case MethodKind.EventRemove:
                        return "event accessor";
                    case MethodKind.Constructor:
                        return "constructor";
                    default:
                        return "method";
                }

            default:
                return symbol.Kind.ToString().ToLowerInvariant();
        }
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

    private static List<FieldPlan> BuildFieldPlans(
        INamedTypeSymbol baseType,
        ImmutableArray<string> fields,
        IAssemblySymbol generatingAssembly,
        Location diagnosticLocation,
        List<Diagnostic> diagnostics)
    {
        var plans = new List<FieldPlan>();

        foreach (var fieldName in fields)
        {
            IFieldSymbol found = null;
            ISymbol anyMatch = null;
            foreach (var member in GetMembersInHierarchy(baseType, fieldName))
            {
                anyMatch = anyMatch ?? member;
                if (member is IFieldSymbol field)
                {
                    found = field;
                    break;
                }
            }

            if (found == null)
            {
                if (anyMatch != null)
                {
                    diagnostics.Add(BuildDiagnostic(
                        DiagnosticStrings.ID_EXPANSION_MEMBER_KIND_MISMATCH,
                        "Member is not a field",
                        DiagnosticStrings.MESSAGE_EXPANSION_MEMBER_KIND_MISMATCH,
                        DiagnosticSeverity.Error,
                        diagnosticLocation,
                        fieldName,
                        baseType.Name,
                        DescribeSymbolKind(anyMatch)));
                }
                else
                {
                    diagnostics.Add(BuildDiagnostic(
                        DiagnosticStrings.ID_EXPANSION_MEMBER_NOT_FOUND,
                        "Member not found on base type",
                        DiagnosticStrings.MESSAGE_EXPANSION_MEMBER_NOT_FOUND,
                        DiagnosticSeverity.Error,
                        diagnosticLocation,
                        fieldName,
                        baseType.Name));
                }

                continue;
            }

            var accessible = IsTypeUsableInGeneratedCode(found.Type, generatingAssembly);
            if (!accessible)
            {
                diagnostics.Add(BuildDiagnostic(
                    DiagnosticStrings.ID_EXPANSION_MEMBER_TYPE_INACCESSIBLE,
                    "Member type not accessible",
                    DiagnosticStrings.MESSAGE_EXPANSION_MEMBER_TYPE_INACCESSIBLE,
                    DiagnosticSeverity.Warning,
                    diagnosticLocation,
                    fieldName,
                    baseType.Name,
                    "FieldInfo"));
            }

            plans.Add(new FieldPlan(fieldName, "f_" + TrimLeadingUnderscores(fieldName), found, accessible));
        }

        return plans;
    }

    private static List<MethodPlan> BuildMethodPlans(
        INamedTypeSymbol baseType,
        ImmutableArray<string> methods,
        IAssemblySymbol generatingAssembly,
        Location diagnosticLocation,
        List<Diagnostic> diagnostics)
    {
        var plans = new List<MethodPlan>();

        foreach (var methodName in methods)
        {
            var overloads = new List<IMethodSymbol>();
            ISymbol anyMatch = null;
            foreach (var member in GetMembersInHierarchy(baseType, methodName))
            {
                anyMatch = anyMatch ?? member;
                if (member is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary)
                {
                    overloads.Add(method);
                }
            }

            if (overloads.Count == 0)
            {
                if (anyMatch != null)
                {
                    diagnostics.Add(BuildDiagnostic(
                        DiagnosticStrings.ID_EXPANSION_MEMBER_KIND_MISMATCH,
                        "Member is not an invocable method",
                        DiagnosticStrings.MESSAGE_EXPANSION_MEMBER_KIND_MISMATCH,
                        DiagnosticSeverity.Error,
                        diagnosticLocation,
                        methodName,
                        baseType.Name,
                        DescribeSymbolKind(anyMatch)));
                }
                else
                {
                    diagnostics.Add(BuildDiagnostic(
                        DiagnosticStrings.ID_EXPANSION_MEMBER_NOT_FOUND,
                        "Member not found on base type",
                        DiagnosticStrings.MESSAGE_EXPANSION_MEMBER_NOT_FOUND,
                        DiagnosticSeverity.Error,
                        diagnosticLocation,
                        methodName,
                        baseType.Name));
                }

                continue;
            }

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
        List<MethodPlan> methodPlans,
        bool hasModId)
    {
        var conflicts = new List<MemberConflict>();

        AddUniqueMemberConflict(classSymbol, "_expansions", "_expansions", conflicts);
        AddUniqueMemberConflict(classSymbol, "_core", "_core", conflicts);
        AddUniqueMemberConflict(classSymbol, "Core", "prop_core", conflicts);
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
    private static void AddUniqueMemberConflict(INamedTypeSymbol classSymbol, string memberName, string key, List<MemberConflict> conflicts)
    {
        foreach (var member in classSymbol.GetMembers(memberName))
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

    private static void AddMethodConflict(INamedTypeSymbol classSymbol, string methodName, INamedTypeSymbol[] parameterTypes, string key, List<MemberConflict> conflicts)
    {
        foreach (var member in classSymbol.GetMembers(methodName))
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
        foreach (var member in classSymbol.GetMembers("Destroy"))
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
        List<MethodPlan> methodPlans,
        bool hasModId,
        bool hasOnDestroy,
        bool hasOnInit,
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
        AppendExpansionClass(sb, indent, accessibility, expansionTypeName, baseTypeName, fieldPlans, methodPlans, hasModId, hasOnDestroy, hasOnInit, skip);

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
        List<MethodPlan> methodPlans,
        bool hasModId,
        bool hasOnDestroy,
        bool hasOnInit,
        HashSet<string> skip)
    {
        var dictionaryKeyType = hasModId ? "RelativeId" : baseTypeName;

        sb.Append(indent).Append(accessibility).Append(" sealed partial class ").AppendLine(expansionTypeName);
        sb.Append(indent).AppendLine("{");

        // Public properties with their backing fields directly beneath, per member ordering.
        if (!skip.Contains("prop_core"))
        {
            sb.Append(indent).Append("    public ").Append(baseTypeName).AppendLine(" Core => _core;");
        }

        if (!skip.Contains("_core"))
        {
            sb.Append(indent).Append("    private readonly ").Append(baseTypeName).AppendLine(" _core;");
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
            AppendFieldAccessor(sb, indent, baseTypeName, fieldPlan, skip);
        }

        foreach (var methodPlan in methodPlans)
        {
            AppendMethodAccessor(sb, indent, baseTypeName, methodPlan, skip);
        }

        // Constructor.
        if (!skip.Contains("ctor"))
        {
            sb.AppendLine();

            if (hasModId)
            {
                sb.Append(indent).Append("    private ").Append(expansionTypeName).Append("(").Append(baseTypeName).AppendLine(" core, RelativeId id)");
                sb.Append(indent).AppendLine("    {");
                sb.Append(indent).AppendLine("        _core = core ?? throw new ArgumentNullException(nameof(core));");
                sb.Append(indent).AppendLine("        _id = id;");
            }
            else
            {
                sb.Append(indent).Append("    private ").Append(expansionTypeName).Append("(").Append(baseTypeName).AppendLine(" core)");
                sb.Append(indent).AppendLine("    {");
                sb.Append(indent).AppendLine("        _core = core ?? throw new ArgumentNullException(nameof(core));");
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
            sb.Append(indent).AppendLine("    public void Destroy() => Destroy(_core);");
        }

        sb.Append(indent).AppendLine("}");
    }

    private static void AppendFieldAccessor(
        StringBuilder sb,
        string indent,
        string baseTypeName,
        FieldPlan fieldPlan,
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
        var target = isStatic ? "null" : "_core";

        sb.AppendLine();
        sb.Append(indent).Append("    private ").Append(isStatic ? "static " : "").Append(fieldTypeName).Append(" ").AppendLine(fieldPlan.OriginalName);
        sb.Append(indent).AppendLine("    {");
        sb.Append(indent).Append("        get => (").Append(fieldTypeName).Append(")").Append(fieldPlan.FieldVariableName).Append(".GetValue(").Append(target).AppendLine(");");
        sb.Append(indent).Append("        set => ").Append(fieldPlan.FieldVariableName).Append(".SetValue(").Append(target).AppendLine(", value);");
        sb.Append(indent).AppendLine("    }");
    }

    private static void AppendMethodAccessor(
        StringBuilder sb,
        string indent,
        string baseTypeName,
        MethodPlan methodPlan,
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

        AppendMethodWrapper(sb, indent, method, methodPlan.MethodVariableName);
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

    // Instance target methods invoke against "_core" implicitly rather than taking an explicit
    // instance parameter, since the Expansion already wraps exactly one core object. Static
    // target methods need no instance at all. ref/out parameters are round-tripped through the
    // args array MethodInfo.Invoke uses: 'out' parameters start as default(T) (an 'out'
    // parameter can't be read before it's assigned, even to seed a placeholder), 'ref' start as
    // the caller's current value, and both are read back from the array afterward.
    private static void AppendMethodWrapper(
        StringBuilder sb,
        string indent,
        IMethodSymbol method,
        string fieldVariableName)
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
        sb.Append(indent).Append("    private ").Append(isStatic ? "static " : "").Append(returnTypeName).Append(" ").Append(method.Name).Append("(").Append(string.Join(", ", parameterDeclarations)).AppendLine(")");
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

        var target = isStatic ? "null" : "_core";

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