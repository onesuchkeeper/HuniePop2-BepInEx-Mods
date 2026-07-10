using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hp2BaseMod.Analyzer;

internal readonly struct ExpansionAnalysisResult
{
    public ExpansionAnalysisResult(
        ImmutableDictionary<string, Dictionary<string, List<MemberRule>>> rules,
        ImmutableArray<Diagnostic> diagnostics)
    {
        Rules = rules;
        Diagnostics = diagnostics;
    }

    public ImmutableDictionary<string, Dictionary<string, List<MemberRule>>> Rules { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
}

internal enum MemberRuleKind
{
    None,
    Deprecates,
    Overwrites,
    Repurposes,
}

// Validates Expansion class naming and discovers [Deprecates]/[Overwrites]/[Repurposes] member
// attributes so their rules can be merged into the deprecated-member-usage scan without
// hand-maintained *_Rules.cs registration. Partial-class and OnDestroyed requirements live in
// ExpansionGenerator.cs instead, since those are enforced alongside the code that depends on them.
internal static class ExpansionAnalyzer
{
    public static ExpansionAnalysisResult Analyze(
        Compilation compilation,
        ImmutableDictionary<SyntaxTree, ImmutableArray<MemberDeclarationSyntax>> membersBySyntaxTree,
        CancellationToken cancellationToken)
    {
        var rules = new Dictionary<string, Dictionary<string, List<MemberRule>>>();
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var semanticModelCache = new Dictionary<SyntaxTree, SemanticModel>(membersBySyntaxTree.Count);

        foreach (var entry in membersBySyntaxTree)
        {
            if (!semanticModelCache.TryGetValue(entry.Key, out var semanticModel))
            {
                semanticModel = compilation.GetSemanticModel(entry.Key);
                semanticModelCache[entry.Key] = semanticModel;
            }

            foreach (var memberSyntax in entry.Value)
            {
                if (memberSyntax is ClassDeclarationSyntax classSyntax)
                {
                    ValidateExpansionNaming(diagnostics, semanticModel, classSyntax, cancellationToken);
                }
                else
                {
                    AnalyzeMemberRuleAttributes(rules, diagnostics, semanticModel, memberSyntax, cancellationToken);
                }
            }
        }

        return new ExpansionAnalysisResult(rules.ToImmutableDictionary(), diagnostics.ToImmutable());
    }

    private static void ValidateExpansionNaming(
        ImmutableArray<Diagnostic>.Builder diagnostics,
        SemanticModel semanticModel,
        ClassDeclarationSyntax classSyntax,
        CancellationToken cancellationToken)
    {
        var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax, cancellationToken) as INamedTypeSymbol;
        if (classSymbol == null)
        {
            return;
        }

        var baseType = ExpansionAttributeReader.GetExpansionBaseType(classSymbol);
        if (baseType == null)
        {
            return;
        }

        var expectedName = "Expanded" + baseType.Name;
        if (classSymbol.Name == expectedName)
        {
            return;
        }

        var descriptor = CreateDescriptor(
            DiagnosticStrings.ID_EXPANSION_NAMING_MISMATCH,
            "Expansion class naming mismatch",
            DiagnosticStrings.MESSAGE_EXPANSION_NAMING_MISMATCH,
            DiagnosticSeverity.Warning);

        var location = Location.Create(classSyntax.SyntaxTree, classSyntax.Identifier.Span);
        diagnostics.Add(Diagnostic.Create(descriptor, location, classSymbol.Name, baseType.Name));
    }

    private static void AnalyzeMemberRuleAttributes(
        Dictionary<string, Dictionary<string, List<MemberRule>>> rules,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        SemanticModel semanticModel,
        MemberDeclarationSyntax memberSyntax,
        CancellationToken cancellationToken)
    {
        var memberSymbol = semanticModel.GetDeclaredSymbol(memberSyntax, cancellationToken);
        if (memberSymbol == null)
        {
            return;
        }

        foreach (var attribute in memberSymbol.GetAttributes())
        {
            if (attribute.AttributeClass == null)
            {
                continue;
            }

            var kind = ExpansionAttributeReader.ClassifyMemberRuleAttribute(attribute.AttributeClass);
            if (kind == MemberRuleKind.None)
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 0)
            {
                continue;
            }

            var deprecatedMemberName = attribute.ConstructorArguments[0].Value as string;
            if (string.IsNullOrEmpty(deprecatedMemberName))
            {
                continue;
            }

            var note = attribute.ConstructorArguments.Length > 1
                ? attribute.ConstructorArguments[1].Value as string
                : null;

            var containingType = memberSymbol.ContainingType;
            var baseType = containingType == null ? null : ExpansionAttributeReader.GetExpansionBaseType(containingType);

            if (baseType == null)
            {
                var descriptor = CreateDescriptor(
                    DiagnosticStrings.ID_MEMBER_RULE_WITHOUT_EXPANSION,
                    "Member rule declared outside an Expansion",
                    DiagnosticStrings.MESSAGE_MEMBER_RULE_WITHOUT_EXPANSION,
                    DiagnosticSeverity.Error);

                var errorLocation = memberSymbol.Locations.FirstOrDefault() ?? Location.None;
                diagnostics.Add(Diagnostic.Create(descriptor, errorLocation, memberSymbol.Name, containingType?.Name ?? "<unknown>"));
                continue;
            }

            var rule = BuildMemberRule(kind, containingType.Name, memberSymbol.Name, note);

            if (!rules.TryGetValue(baseType.Name, out var memberRules))
            {
                memberRules = new Dictionary<string, List<MemberRule>>();
                rules[baseType.Name] = memberRules;
            }

            if (!memberRules.TryGetValue(deprecatedMemberName, out var ruleList))
            {
                ruleList = new List<MemberRule>();
                memberRules[deprecatedMemberName] = ruleList;
            }

            ruleList.Add(rule);
        }
    }

    private static MemberRule BuildMemberRule(MemberRuleKind kind, string expandedTypeName, string replacementMemberName, string note)
    {
        switch (kind)
        {
            case MemberRuleKind.Deprecates:
                return new MemberRule(
                    diagnosticId: DiagnosticStrings.ID_DEPRECIATED_MEMBER,
                    severity: DiagnosticSeverity.Warning,
                    messageFormat: DiagnosticStrings.MESSAGE_PREFIX_DEPRECIATED + " " + BuildReplacementSentence(expandedTypeName, replacementMemberName, note));

            case MemberRuleKind.Overwrites:
                return new MemberRule(
                    diagnosticId: DiagnosticStrings.ID_OVERWRITTEN_METHOD,
                    severity: DiagnosticSeverity.Info,
                    messageFormat: DiagnosticStrings.MESSAGE_PREFIX_OVERWRITTEN + BuildNoteSuffix(note));

            case MemberRuleKind.Repurposes:
                return new MemberRule(
                    diagnosticId: DiagnosticStrings.ID_REPURPOSED_FIELD,
                    severity: DiagnosticSeverity.Info,
                    messageFormat: DiagnosticStrings.MESSAGE_REPURPOSED_FIELD + BuildNoteSuffix(note));

            default:
                throw new System.ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private static string BuildReplacementSentence(string expandedTypeName, string replacementMemberName, string note)
    {
        if (!string.IsNullOrEmpty(note))
        {
            return note;
        }

        return $"Use {expandedTypeName}.{replacementMemberName} instead.";
    }

    private static string BuildNoteSuffix(string note)
    {
        return string.IsNullOrEmpty(note) ? string.Empty : " " + note;
    }

    private static DiagnosticDescriptor CreateDescriptor(string id, string title, string messageFormat, DiagnosticSeverity severity)
    {
        return new DiagnosticDescriptor(
            id: id,
            title: title,
            messageFormat: messageFormat,
            category: "Usage",
            defaultSeverity: severity,
            isEnabledByDefault: true);
    }
}