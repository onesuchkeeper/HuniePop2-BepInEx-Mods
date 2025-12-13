using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hp2BaseMod.Analyzer
{
    internal static class AttributeAnalyzer
    {
        private const string InteropMethodAttributeName = "InteropMethodAttribute";
        private const string Hp2BaseModPluginTypeName = "Hp2BaseModPlugin";

        public static void AnalyzeAttributes(
            SourceProductionContext context,
            Compilation compilation,
            ImmutableArray<MemberDeclarationSyntax> membersWithAttributes)
        {
            foreach (var memberSyntax in membersWithAttributes)
            {
                var semanticModel = compilation.GetSemanticModel(memberSyntax.SyntaxTree);
                var memberSymbol = semanticModel.GetDeclaredSymbol(memberSyntax, context.CancellationToken);

                if (memberSymbol == null)
                    continue;

                // Check for InteropMethodAttribute on methods
                if (memberSymbol is IMethodSymbol methodSymbol)
                {
                    AnalyzeInteropMethodAttribute(context, methodSymbol, memberSyntax);
                }
            }
        }

        private static void AnalyzeInteropMethodAttribute(
            SourceProductionContext context,
            IMethodSymbol methodSymbol,
            MemberDeclarationSyntax syntax)
        {
            // Check if method has InteropMethodAttribute
            var interopMethodAttr = GetAttributeSyntax(methodSymbol, syntax, InteropMethodAttributeName);
            if (interopMethodAttr == null)
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

                var location = GetAttributeLocation(interopMethodAttr);
                context.ReportDiagnostic(
                    Diagnostic.Create(descriptor, location, methodSymbol.Name));
            }
        }

        private static AttributeSyntax GetAttributeSyntax(ISymbol symbol, MemberDeclarationSyntax syntax, string attributeName)
        {
            // First check if symbol has the attribute
            if (!HasAttribute(symbol, attributeName))
                return null;

            // Find the matching attribute syntax node
            foreach (var attributeList in syntax.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var name = attribute.Name.ToString();
                    var nameWithoutSuffix = name.EndsWith("Attribute")
                        ? name.Substring(0, name.Length - "Attribute".Length)
                        : name;

                    var searchNameWithoutSuffix = attributeName.EndsWith("Attribute")
                        ? attributeName.Substring(0, attributeName.Length - "Attribute".Length)
                        : attributeName;

                    if (name == attributeName ||
                        nameWithoutSuffix == searchNameWithoutSuffix ||
                        name == searchNameWithoutSuffix ||
                        nameWithoutSuffix == attributeName)
                    {
                        return attribute;
                    }
                }
            }

            return null;
        }

        private static Location GetAttributeLocation(AttributeSyntax attribute)
        {
            return Location.Create(attribute.SyntaxTree, attribute.Name.Span);
        }

        private static bool HasAttribute(ISymbol symbol, string attributeName)
        {
            var attributes = symbol.GetAttributes();
            foreach (var attr in attributes)
            {
                if (attr.AttributeClass == null)
                    continue;

                var name = attr.AttributeClass.Name;
                var fullName = attr.AttributeClass.ToDisplayString();

                var nameWithoutSuffix = name.EndsWith("Attribute")
                    ? name.Substring(0, name.Length - "Attribute".Length)
                    : name;

                var searchNameWithoutSuffix = attributeName.EndsWith("Attribute")
                    ? attributeName.Substring(0, attributeName.Length - "Attribute".Length)
                    : attributeName;

                // Check both simple name and full name
                if (name == attributeName ||
                    nameWithoutSuffix == searchNameWithoutSuffix ||
                    name == searchNameWithoutSuffix ||
                    nameWithoutSuffix == attributeName ||
                    fullName.EndsWith(attributeName) ||
                    fullName.EndsWith(searchNameWithoutSuffix))
                {
                    return true;
                }
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
