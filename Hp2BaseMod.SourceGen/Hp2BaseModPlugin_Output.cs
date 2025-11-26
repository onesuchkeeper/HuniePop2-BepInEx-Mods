using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hp2BaseMod.SourceGen;

internal static class Hp2BaseModPlugin_Output
{
    const string ID_PREFIX = "PLGN";
    const string ID_CONFIG_PROPERTY = ID_PREFIX + "001";
    const string ID_METHOD_INTEROP = ID_PREFIX + "002";

    internal static void Output(SourceProductionContext spc, (Compilation left, ImmutableArray<PropertyDeclarationSyntax?> Right) source)
    {
        var (compilation, properties) = source;

        // Resolve symbols from base mod library
        if (!(SourceGenerator.TryGetSymbol($"{MetaNames.Namespace}.{MetaNames.Plugin}", compilation, out var pluginSymbol)
            && SourceGenerator.TryGetSymbol($"{MetaNames.Namespace}.{MetaNames.A_ConfigProperty}", compilation, out var configPropertyAttSymbol)
            && SourceGenerator.TryGetSymbol($"{MetaNames.Namespace}.{MetaNames.A_InteropMethod}", compilation, out var interopMethodAttSymbol)))
        {
            return;
        }

        // 1. Check properties
        foreach (var property in properties)
        {
            if (property == null) continue;

            var model = compilation.GetSemanticModel(property.SyntaxTree);
            var propertySymbol = model.GetDeclaredSymbol(property) as IPropertySymbol;
            if (propertySymbol == null) continue;

            // Only check ConfigPropertyAttribute on properties
            propertySymbol.AssertAttributesInDerivedType(pluginSymbol, MetaNames.Plugin, spc, new[]
            {
                (configPropertyAttSymbol, MetaNames.A_ConfigProperty, ID_CONFIG_PROPERTY)
            });
        }

        // 2. Check methods for InteropMethodAttribute
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var model = compilation.GetSemanticModel(syntaxTree);

            var methodDeclarations = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var methodDecl in methodDeclarations)
            {
                var methodSymbol = model.GetDeclaredSymbol(methodDecl) as IMethodSymbol;
                if (methodSymbol == null) continue;

                // Only check InteropMethodAttribute on methods
                methodSymbol.AssertAttributesInDerivedType(pluginSymbol, MetaNames.Plugin, spc, new[]
                {
                    (interopMethodAttSymbol, MetaNames.A_InteropMethod, ID_METHOD_INTEROP)
                });
            }
        }
    }
}
