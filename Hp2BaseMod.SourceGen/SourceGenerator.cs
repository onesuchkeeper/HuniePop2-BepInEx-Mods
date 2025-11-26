using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hp2BaseMod.SourceGen
{
    [Generator]
    internal class SourceGenerator : IIncrementalGenerator
    {
        private static SourceGenerator _instance;
        private Dictionary<string, INamedTypeSymbol> _symbols;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _instance = this;
            _symbols = new();

            // Step 1: collect all properties with attributes
            var propertyDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is PropertyDeclarationSyntax prop && prop.AttributeLists.Count > 0,
                    transform: static (ctx, _) => ctx.Node as PropertyDeclarationSyntax
                )
                .Where(static prop => prop != null);

            // Step 2: Combine with compilation to get semantic model
            var compilationAndProperties = context.CompilationProvider.Combine(propertyDeclarations.Collect());

            // Step 3: Register source outputs to emit diagnostics
            context.RegisterSourceOutput(compilationAndProperties, Hp2BaseModPlugin_Output.Output);
        }

        public static bool TryGetSymbol(string name, Compilation compilation, out INamedTypeSymbol symbol)
        {
            if (!_instance._symbols.TryGetValue(name, out symbol))
            {
                symbol = compilation.GetTypeByMetadataName(name);
                _instance._symbols.Add(name, symbol);
            }

            return symbol != null;
        }
    }
}
