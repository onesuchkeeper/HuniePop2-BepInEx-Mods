using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Hp2BaseMod.Analyzer;

internal static partial class Rules
{
    public static readonly List<MemberRule> TalkManager = [
        new MemberRule(
            memberName: "favQuestionDefinitions",
            diagnosticId: DiagnosticStrings.ID_DEPRECIATED_MEMBER,
            severity: DiagnosticSeverity.Warning,
            messageFormat: DiagnosticStrings.MESSAGE_PREFIX_DEPRECIATED + " Use Game.Data.Questions instead."
        ),
        new MemberRule(
            memberName: "HasFavAnswer",
            diagnosticId: DiagnosticStrings.ID_OVERWRITTEN_METHOD,
            severity: DiagnosticSeverity.Info,
            messageFormat: DiagnosticStrings.MESSAGE_PREFIX_OVERWRITTEN + " It now uses question ids."
        ),
        new MemberRule(
            memberName: "LearnFavAnswer",
            diagnosticId: DiagnosticStrings.ID_OVERWRITTEN_METHOD,
            severity: DiagnosticSeverity.Info,
            messageFormat: DiagnosticStrings.MESSAGE_PREFIX_OVERWRITTEN + " It now uses question ids."
        ),
        new MemberRule(
            memberName: "learnedFavs",
            diagnosticId: DiagnosticStrings.ID_REPURPOSED_FIELD,
            severity: DiagnosticSeverity.Info,
            messageFormat: DiagnosticStrings.MESSAGE_REPURPOSED_FIELD + " It now uses question ids."
        ),
    ];
}
