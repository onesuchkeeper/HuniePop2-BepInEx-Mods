using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Hp2BaseMod.Analyzer;

internal static partial class Rules
{
    public static readonly Dictionary<string, List<MemberRule>> LocationDefinition = new()
    {
        {"dateGirlStyleType", new () {new MemberRule(
            diagnosticId: DiagnosticStrings.ID_DEPRECIATED_MEMBER,
            severity: DiagnosticSeverity.Warning,
            messageFormat: DiagnosticStrings.MESSAGE_PREFIX_DEPRECIATED + " Use ExpandedLocationDefinition.DefaultStyle instead."
        )}}
    };
}
