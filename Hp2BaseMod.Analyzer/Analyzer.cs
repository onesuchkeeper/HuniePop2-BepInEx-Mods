using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hp2BaseMod.Analyzer
{
    [Generator(LanguageNames.CSharp)]
    public sealed class Hp2BaseModIncrementalAnalyzer : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Track member access expressions - group by syntax tree for caching
            var memberAccessesBySyntaxTree = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is MemberAccessExpressionSyntax,
                static (ctx, _) => (Access: (MemberAccessExpressionSyntax)ctx.Node, Tree: ctx.Node.SyntaxTree))
                .Collect()
                .Select(static (accesses, _) =>
                {
                    // Group by syntax tree to enable semantic model caching
                    var grouped = new Dictionary<SyntaxTree, List<MemberAccessExpressionSyntax>>();
                    foreach (var (access, tree) in accesses)
                    {
                        if (!grouped.ContainsKey(tree))
                            grouped[tree] = new List<MemberAccessExpressionSyntax>();
                        grouped[tree].Add(access);
                    }
                    return grouped.ToImmutableDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.ToImmutableArray());
                });

            var compilationAndAccesses = context.CompilationProvider
                .Combine(memberAccessesBySyntaxTree);

            context.RegisterSourceOutput(
                compilationAndAccesses,
                static (spc, pair) =>
                {
                    AnalyzeCompilationWithCaching(spc, pair.Left, pair.Right);
                });

            // Track members with attributes (methods and properties)
            var membersWithAttributes = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is MemberDeclarationSyntax member && member.AttributeLists.Count > 0,
                static (ctx, _) => (Member: (MemberDeclarationSyntax)ctx.Node, Tree: ctx.Node.SyntaxTree))
                .Collect()
                .Select(static (members, _) =>
                {
                    var grouped = new Dictionary<SyntaxTree, List<MemberDeclarationSyntax>>();
                    foreach (var (member, tree) in members)
                    {
                        if (!grouped.ContainsKey(tree))
                            grouped[tree] = new List<MemberDeclarationSyntax>();
                        grouped[tree].Add(member);
                    }
                    return grouped.ToImmutableDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.ToImmutableArray());
                });

            var compilationAndMembers = context.CompilationProvider
                .Combine(membersWithAttributes);

            context.RegisterSourceOutput(
                compilationAndMembers,
                static (spc, pair) =>
                {
                    AttributeAnalyzer.AnalyzeAttributesWithCaching(spc, pair.Left, pair.Right);
                });
        }

        private static void AnalyzeCompilationWithCaching(
            SourceProductionContext context,
            Compilation compilation,
            ImmutableDictionary<SyntaxTree, ImmutableArray<MemberAccessExpressionSyntax>> accessesBySyntaxTree)
        {
            // Cache semantic models by syntax tree
            var semanticModelCache = new Dictionary<SyntaxTree, SemanticModel>(accessesBySyntaxTree.Count);

            foreach (var entry in accessesBySyntaxTree)
            {
                // Get or create semantic model once per syntax tree
                if (!semanticModelCache.TryGetValue(entry.Key, out var semanticModel))
                {
                    semanticModel = compilation.GetSemanticModel(entry.Key);
                    semanticModelCache[entry.Key] = semanticModel;
                }

                // Process all accesses in this file with the cached semantic model
                foreach (var access in entry.Value)
                {
                    AnalyzeMemberAccess(context, access, semanticModel);
                }
            }
        }

        private static void AnalyzeMemberAccess(
            SourceProductionContext context,
            MemberAccessExpressionSyntax access,
            SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetSymbolInfo(access, context.CancellationToken).Symbol;

            if (symbol == null)
                return;

            // Skip namespaces and types-as-symbols
            if (symbol is INamespaceSymbol || symbol is INamedTypeSymbol)
                return;

            var containingType = symbol.ContainingType;
            if (containingType == null)
                return;

            // Use simple name for lookup (matches dictionary keys)
            var typeName = containingType.Name;

            // Early exit if type not in our tracked set
            if (!RulesByType.ContainsKey(typeName))
                return;

            // Get member-specific rules
            if (!RulesByType.TryGetValue(typeName, out var memberRules))
                return;

            if (!memberRules.TryGetValue(symbol.Name, out var rules))
                return;

            foreach (var rule in rules)
            {
                // Optional extra condition
                if (rule.Condition != null && !rule.Condition(symbol))
                    continue;

                ReportDiagnostic(context, access, containingType, symbol, rule);
            }
        }

        // Optimized nested dictionary: typeName -> memberName -> rules
        // This provides O(1) lookup instead of O(n) iteration
        private static readonly Dictionary<string, Dictionary<string, List<MemberRule>>> RulesByType = new()
        {
            {"global::TalkManager", Rules.TalkManager},
            {"global::GirlPairDefinition", Rules.GirlPairDefinition},
        };

        private static void ReportDiagnostic(
            SourceProductionContext context,
            MemberAccessExpressionSyntax accessSyntax,
            INamedTypeSymbol containingType,
            ISymbol member,
            MemberRule rule)
        {
            var descriptor = new DiagnosticDescriptor(
                id: rule.DiagnosticId,
                title: "Deprecated member usage",
                messageFormat: rule.MessageFormat,
                category: "Usage",
                defaultSeverity: rule.Severity,
                isEnabledByDefault: true);

            // Get the location from the syntax tree directly
            var location = Location.Create(
                accessSyntax.SyntaxTree,
                accessSyntax.Name.Span);

            context.ReportDiagnostic(
                Diagnostic.Create(
                    descriptor,
                    location,
                    member.Name,
                    containingType.Name));
        }
    }
}