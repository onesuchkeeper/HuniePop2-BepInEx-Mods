using System;
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
            // Track member access expressions so diagnostics are anchored to *usage syntax*
            var memberAccesses = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is MemberAccessExpressionSyntax,
                static (ctx, _) => (MemberAccessExpressionSyntax)ctx.Node);

            var compilationAndAccesses = context.CompilationProvider
                .Combine(memberAccesses.Collect());

            context.RegisterSourceOutput(
                compilationAndAccesses,
                static (spc, pair) =>
                {
                    AnalyzeCompilation(spc, pair.Left, pair.Right);
                });

            // Track members with attributes (methods and properties)
            var membersWithAttributes = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is MemberDeclarationSyntax member && member.AttributeLists.Count > 0,
                static (ctx, _) => (MemberDeclarationSyntax)ctx.Node);

            var compilationAndMembers = context.CompilationProvider
                .Combine(membersWithAttributes.Collect());

            context.RegisterSourceOutput(
                compilationAndMembers,
                static (spc, pair) =>
                {
                    AttributeAnalyzer.AnalyzeAttributes(spc, pair.Left, pair.Right);
                });
        }

        private static void AnalyzeCompilation(
            SourceProductionContext context,
            Compilation compilation,
            ImmutableArray<MemberAccessExpressionSyntax> memberAccesses)
        {
            foreach (var access in memberAccesses)
            {
                var semanticModel = compilation.GetSemanticModel(access.SyntaxTree);
                var symbol = semanticModel.GetSymbolInfo(access, context.CancellationToken).Symbol;

                if (symbol == null)
                    continue;

                // Consider all member symbols (fields, properties, methods, events, etc.)
                // Skip namespaces and types-as-symbols
                if (symbol is INamespaceSymbol || symbol is INamedTypeSymbol)
                    continue;

                var containingType = symbol.ContainingType;
                if (containingType == null)
                    continue;

                var fullTypeName = containingType.ToDisplayString();

                if (!RulesByType.TryGetValue(fullTypeName, out var rules))
                    continue;

                foreach (var rule in rules)
                {
                    // Match by member name first
                    if (!string.Equals(symbol.Name, rule.MemberName, StringComparison.Ordinal))
                        continue;

                    // Optional extra condition
                    if (rule.Condition != null && !rule.Condition(symbol))
                        continue;

                    ReportDiagnostic(context, access, containingType, symbol, rule);
                }
            }
        }

        // Example rule dictionary (replace with your real rules)
        private static readonly Dictionary<string, List<MemberRule>> RulesByType = new()
        {
            {"TalkManager", Rules.TalkManager}
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