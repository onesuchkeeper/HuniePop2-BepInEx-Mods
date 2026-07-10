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
            // Track every identifier-name reference, not just dotted member access.
            // This single node kind also covers:
            //   - member access:            foo.favQuestionDefinitions
            //   - conditional access:       foo?.favQuestionDefinitions
            //   - object/collection init:   new TalkManager { favQuestionDefinitions = ... }
            //   - unqualified inherited access from a derived type: favQuestionDefinitions
            //   - nameof(...) targets, qualified or not
            // Group by syntax tree for semantic-model caching.
            var identifiersBySyntaxTree = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is IdentifierNameSyntax,
                static (ctx, _) => (Identifier: (IdentifierNameSyntax)ctx.Node, Tree: ctx.Node.SyntaxTree))
                .Collect()
                .Select(static (identifiers, _) =>
                {
                    var grouped = new Dictionary<SyntaxTree, List<IdentifierNameSyntax>>();
                    foreach (var (identifier, tree) in identifiers)
                    {
                        if (!grouped.ContainsKey(tree))
                        {
                            grouped[tree] = new List<IdentifierNameSyntax>();
                        }

                        grouped[tree].Add(identifier);
                    }

                    return grouped.ToImmutableDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.ToImmutableArray());
                });

            // Track members and class declarations that carry any attribute. Feeds both the
            // existing InteropMethod validation and the new Expansion-pattern analysis below.
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
                        {
                            grouped[tree] = new List<MemberDeclarationSyntax>();
                        }

                        grouped[tree].Add(member);
                    }

                    return grouped.ToImmutableDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.ToImmutableArray());
                });

            var compilationAndMembers = context.CompilationProvider.Combine(membersWithAttributes);

            context.RegisterSourceOutput(
                compilationAndMembers,
                static (spc, pair) =>
                {
                    AttributeAnalyzer.AnalyzeAttributesWithCaching(spc, pair.Left, pair.Right);
                });

            // Expansion-pattern structural checks (naming) plus discovery of
            // [Deprecates]/[Overwrites]/[Repurposes] member attributes.
            var expansionAnalysis = compilationAndMembers
                .Select(static (pair, ct) => ExpansionAnalyzer.Analyze(pair.Left, pair.Right, ct));

            context.RegisterSourceOutput(
                expansionAnalysis,
                static (spc, result) =>
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        spc.ReportDiagnostic(diagnostic);
                    }
                });

            // Generates the shared half of every [Expansion] partial class.
            context.RegisterSourceOutput(
                compilationAndMembers,
                static (spc, pair) =>
                {
                    ExpansionGenerator.GenerateWithCaching(spc, pair.Left, pair.Right);
                });

            var mergedRules = expansionAnalysis
                .Select(static (result, _) => MergeRules(result.Rules));

            var compilationAndIdentifiers = context.CompilationProvider
                .Combine(identifiersBySyntaxTree)
                .Combine(mergedRules);

            context.RegisterSourceOutput(
                compilationAndIdentifiers,
                static (spc, pair) =>
                {
                    AnalyzeCompilationWithCaching(spc, pair.Left.Left, pair.Left.Right, pair.Right);
                });
        }

        private static void AnalyzeCompilationWithCaching(
            SourceProductionContext context,
            Compilation compilation,
            ImmutableDictionary<SyntaxTree, ImmutableArray<IdentifierNameSyntax>> identifiersBySyntaxTree,
            Dictionary<string, Dictionary<string, List<MemberRule>>> rulesByType)
        {
            // Cache semantic models by syntax tree
            var semanticModelCache = new Dictionary<SyntaxTree, SemanticModel>(identifiersBySyntaxTree.Count);

            foreach (var entry in identifiersBySyntaxTree)
            {
                if (!semanticModelCache.TryGetValue(entry.Key, out var semanticModel))
                {
                    semanticModel = compilation.GetSemanticModel(entry.Key);
                    semanticModelCache[entry.Key] = semanticModel;
                }

                foreach (var identifier in entry.Value)
                {
                    AnalyzeIdentifierReference(context, identifier, semanticModel, rulesByType);
                }
            }
        }

        private static void AnalyzeIdentifierReference(
            SourceProductionContext context,
            IdentifierNameSyntax identifier,
            SemanticModel semanticModel,
            Dictionary<string, Dictionary<string, List<MemberRule>>> rulesByType)
        {
            var symbol = semanticModel.GetSymbolInfo(identifier, context.CancellationToken).Symbol;

            if (symbol == null)
            {
                return;
            }

            // Skip namespaces and types-as-symbols (e.g. "TalkManager" used as a type reference)
            if (symbol is INamespaceSymbol || symbol is INamedTypeSymbol)
            {
                return;
            }

            var containingType = symbol.ContainingType;
            if (containingType == null)
            {
                return;
            }

            // The base game is written entirely in the global namespace. A same-named type
            // declared inside any mod's own namespace must never be mistaken for a base-game
            // type, so require the containing type to live in the global namespace.
            if (!IsDeclaredInGlobalNamespace(containingType))
            {
                return;
            }

            var typeName = containingType.Name;

            if (!rulesByType.TryGetValue(typeName, out var memberRules))
            {
                return;
            }

            if (!memberRules.TryGetValue(symbol.Name, out var rules))
            {
                return;
            }

            foreach (var rule in rules)
            {
                if (rule.Condition != null && !rule.Condition(symbol))
                {
                    continue;
                }

                ReportDiagnostic(context, identifier, containingType, symbol, rule);
            }
        }

        private static bool IsDeclaredInGlobalNamespace(INamedTypeSymbol type)
        {
            return type.ContainingNamespace != null && type.ContainingNamespace.IsGlobalNamespace;
        }

        // Hand-authored rules for base-game types that do not yet have an Expansion class
        // carrying [Deprecates]/[Overwrites]/[Repurposes] attributes. As each type gains a
        // proper Expansion, its entry here can be retired in favor of the attribute-driven rules
        // discovered by ExpansionAnalyzer.
        private static readonly Dictionary<string, Dictionary<string, List<MemberRule>>> StaticRulesByType = new()
        {
            {"TalkManager", Rules.TalkManager},
            {"GirlPairDefinition", Rules.GirlPairDefinition},
            {"LocationDefinition", Rules.LocationDefinition},
            {"LocationManager", Rules.LocationManager},
        };

        private static Dictionary<string, Dictionary<string, List<MemberRule>>> MergeRules(
            ImmutableDictionary<string, Dictionary<string, List<MemberRule>>> discoveredRules)
        {
            // Defensive copies throughout: StaticRulesByType and its inner lists are static and
            // must never be mutated by a generator pass, since the same instances persist across
            // incremental runs within the same compiler process.
            var merged = new Dictionary<string, Dictionary<string, List<MemberRule>>>(StaticRulesByType.Count);

            foreach (var typeEntry in StaticRulesByType)
            {
                merged[typeEntry.Key] = new Dictionary<string, List<MemberRule>>(typeEntry.Value);
            }

            foreach (var typeEntry in discoveredRules)
            {
                if (!merged.TryGetValue(typeEntry.Key, out var existingMemberRules))
                {
                    existingMemberRules = new Dictionary<string, List<MemberRule>>();
                    merged[typeEntry.Key] = existingMemberRules;
                }

                foreach (var memberEntry in typeEntry.Value)
                {
                    if (existingMemberRules.TryGetValue(memberEntry.Key, out var existingRules))
                    {
                        var combined = new List<MemberRule>(existingRules);
                        combined.AddRange(memberEntry.Value);
                        existingMemberRules[memberEntry.Key] = combined;
                    }
                    else
                    {
                        existingMemberRules[memberEntry.Key] = memberEntry.Value;
                    }
                }
            }

            return merged;
        }

        private static void ReportDiagnostic(
            SourceProductionContext context,
            IdentifierNameSyntax identifier,
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

            var location = Location.Create(
                identifier.SyntaxTree,
                identifier.Span);

            context.ReportDiagnostic(
                Diagnostic.Create(
                    descriptor,
                    location,
                    member.Name,
                    containingType.Name));
        }
    }
}