using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Hp2BaseMod.Analyzer;

internal static partial class Rules
{
    public static readonly Dictionary<string, List<MemberRule>> TalkManager = new() {
        {"favQuestionDefinitions", new() {new MemberRule(
            diagnosticId: DiagnosticStrings.ID_DEPRECIATED_MEMBER,
            severity: DiagnosticSeverity.Warning,
            messageFormat: DiagnosticStrings.MESSAGE_PREFIX_DEPRECIATED + " Use Game.Data.Questions instead."
        )}},
        {"HasFavAnswer", new() {new MemberRule(
            diagnosticId: DiagnosticStrings.ID_OVERWRITTEN_METHOD,
            severity: DiagnosticSeverity.Info,
            messageFormat: DiagnosticStrings.MESSAGE_PREFIX_OVERWRITTEN + " It now uses question ids."
        )}},
        {"LearnFavAnswer", new() {new MemberRule(
            diagnosticId: DiagnosticStrings.ID_OVERWRITTEN_METHOD,
            severity: DiagnosticSeverity.Info,
            messageFormat: DiagnosticStrings.MESSAGE_PREFIX_OVERWRITTEN + " It now uses question ids."
        )}},
        {"LearnFavAnswer", new() {new MemberRule(
            diagnosticId: DiagnosticStrings.ID_REPURPOSED_FIELD,
            severity: DiagnosticSeverity.Info,
            messageFormat: DiagnosticStrings.MESSAGE_REPURPOSED_FIELD + " It now uses question ids."
        )}},
    };
}