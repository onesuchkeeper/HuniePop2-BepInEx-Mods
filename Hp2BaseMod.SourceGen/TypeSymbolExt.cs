using Microsoft.CodeAnalysis;

namespace Hp2BaseMod.SourceGen
{
    internal static class TypeSymbolExt
    {
        /// <summary>
        /// Returns true if <paramref name="type"/> derives from <paramref name="baseType"/>.
        /// </summary>
        public static bool DerivesFrom(this ITypeSymbol type, ITypeSymbol baseType)
        {
            while (type != null)
            {
                if (SymbolEqualityComparer.Default.Equals(type, baseType))
                    return true;
                type = type.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Reports diagnostics if the given member has any of the specified attributes but is not in a type deriving from <paramref name="baseType"/>.
        /// The diagnostic will be placed on the attribute itself.
        /// </summary>
        public static void AssertAttributesInDerivedType(
            this ISymbol memberSymbol,
            INamedTypeSymbol baseType,
            string baseTypeName,
            SourceProductionContext spc,
            IEnumerable<(INamedTypeSymbol AttributeSymbol, string AttributeName, string DiagnosticId)> attributesToCheck)
        {
            foreach (var (attributeSymbol, attributeName, diagnosticId) in attributesToCheck)
            {
                foreach (var attr in memberSymbol.GetAttributes())
                {
                    // Skip attributes that don’t match
                    if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass?.OriginalDefinition, attributeSymbol))
                        continue;

                    // Check inheritance
                    if (!memberSymbol.ContainingType.DerivesFrom(baseType))
                    {
                        // Get the syntax location of the attribute itself
                        var location = attr.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                       ?? memberSymbol.Locations.FirstOrDefault()
                                       ?? Location.None;

                        var diag = Diagnostic.Create(
                            new DiagnosticDescriptor(
                                id: diagnosticId,
                                title: DescriptorTitles.InvalidAtt,
                                messageFormat: $"{attributeName} may only be used in classes deriving from {baseTypeName}",
                                category: DescriptorCategories.Usage,
                                DiagnosticSeverity.Error,
                                isEnabledByDefault: true),
                            location
                        );

                        spc.ReportDiagnostic(diag);
                    }
                }
            }
        }
    }
}
