using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hp2BaseMod.Analyzer
{
    internal static class AttributeAnalyzer
    {
        private const string InteropMethodAttributeName = "InteropMethodAttribute";
        private const string InteropMethodShortName = "InteropMethod";
        private const string Hp2BaseModPluginTypeName = "Hp2BaseModPlugin";

        public static void AnalyzeAttributesWithCaching(
            SourceProductionContext context,
            Compilation compilation,
            ImmutableDictionary<SyntaxTree, ImmutableArray<MemberDeclarationSyntax>> membersBySyntaxTree)
        {
            // Cache semantic models by syntax tree - major performance optimization
            var semanticModelCache = new Dictionary<SyntaxTree, SemanticModel>(membersBySyntaxTree.Count);

            foreach (var entry in membersBySyntaxTree)
            {
                // Get or create semantic model once per syntax tree
                if (!semanticModelCache.TryGetValue(entry.Key, out var semanticModel))
                {
                    semanticModel = compilation.GetSemanticModel(entry.Key);
                    semanticModelCache[entry.Key] = semanticModel;
                }

                // Process all members in this file with the cached semantic model
                foreach (var memberSyntax in entry.Value)
                {
                    AnalyzeMember(context, memberSyntax, semanticModel);
                }
            }
        }

        private static void AnalyzeMember(
            SourceProductionContext context,
            MemberDeclarationSyntax memberSyntax,
            SemanticModel semanticModel)
        {
            var memberSymbol = semanticModel.GetDeclaredSymbol(memberSyntax, context.CancellationToken);

            if (memberSymbol == null)
                return;

            // Check for InteropMethodAttribute on methods
            if (memberSymbol is IMethodSymbol methodSymbol)
            {
                AnalyzeInteropMethodAttribute(context, methodSymbol, memberSyntax);
            }
        }

        private static void AnalyzeInteropMethodAttribute(
            SourceProductionContext context,
            IMethodSymbol methodSymbol,
            MemberDeclarationSyntax syntax)
        {
            // Combined check: find attribute data and syntax in single pass
            var (hasAttribute, attributeSyntax) = FindInteropMethodAttribute(methodSymbol, syntax);

            if (!hasAttribute || attributeSyntax == null)
                return;

            // Check if containing class inherits from Hp2BaseModPlugin
            if (!InheritsFromHp2BaseModPlugin(methodSymbol.ContainingType))
            {
                var descriptor = new DiagnosticDescriptor(
                    id: DiagnosticStrings.ID_INVALID_INTEROP_METHOD,
                    title: "Invalid InteropMethod usage",
                    messageFormat: DiagnosticStrings.MESSAGE_INVALID_INTEROP_METHOD,
                    category: "Usage",
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true);

                var location = Location.Create(attributeSyntax.SyntaxTree, attributeSyntax.Name.Span);
                context.ReportDiagnostic(
                    Diagnostic.Create(descriptor, location, methodSymbol.Name));
            }
        }

        // Optimized: Combined attribute check and syntax lookup in single pass
        private static (bool hasAttribute, AttributeSyntax attributeSyntax) FindInteropMethodAttribute(
            ISymbol symbol,
            MemberDeclarationSyntax syntax)
        {
            // First check symbol attributes
            var hasAttr = false;
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass == null)
                    continue;

                if (IsInteropMethodAttribute(attr.AttributeClass))
                {
                    hasAttr = true;
                    break;
                }
            }

            if (!hasAttr)
                return (false, null);

            // Find matching syntax node
            foreach (var attributeList in syntax.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (IsInteropMethodAttributeSyntax(attribute.Name))
                        return (true, attribute);
                }
            }

            return (false, null);
        }

        // Optimized: Fast-path string comparisons with early exits
        private static bool IsInteropMethodAttribute(INamedTypeSymbol attributeClass)
        {
            var name = attributeClass.Name;

            // Fast path: exact matches
            if (name == InteropMethodAttributeName || name == InteropMethodShortName)
                return true;

            // Check with "Attribute" suffix stripped
            if (name.Length > 9 && name.EndsWith("Attribute", StringComparison.Ordinal))
            {
                var nameWithoutSuffix = name.Substring(0, name.Length - 9);
                if (nameWithoutSuffix == InteropMethodShortName)
                    return true;
            }

            // Check full name
            var fullName = attributeClass.ToDisplayString();
            return fullName.EndsWith(InteropMethodAttributeName, StringComparison.Ordinal) ||
                   fullName.EndsWith(InteropMethodShortName, StringComparison.Ordinal);
        }

        private static bool IsInteropMethodAttributeSyntax(NameSyntax nameSyntax)
        {
            var name = nameSyntax.ToString();

            // Fast path: exact matches
            if (name == InteropMethodAttributeName || name == InteropMethodShortName)
                return true;

            // Check with "Attribute" suffix stripped
            if (name.Length > 9 && name.EndsWith("Attribute", StringComparison.Ordinal))
            {
                var nameWithoutSuffix = name.Substring(0, name.Length - 9);
                return nameWithoutSuffix == InteropMethodShortName;
            }

            return false;
        }

        private static bool InheritsFromHp2BaseModPlugin(INamedTypeSymbol typeSymbol)
        {
            var currentType = typeSymbol.BaseType;
            while (currentType != null)
            {
                // Only accept Hp2BaseModPlugin, nothing else
                if (currentType.Name == Hp2BaseModPluginTypeName)
                    return true;
                currentType = currentType.BaseType;
            }
            return false;
        }
    }
}