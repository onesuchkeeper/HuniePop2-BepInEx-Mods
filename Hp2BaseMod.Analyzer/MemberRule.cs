using System;
using Microsoft.CodeAnalysis;

internal sealed class MemberRule
{
    public string DiagnosticId { get; }
    public string MessageFormat { get; }
    public DiagnosticSeverity Severity { get; }
    public string MemberName { get; }
    public Func<ISymbol, bool> Condition { get; }

    public MemberRule(
        string diagnosticId,
        string memberName,
        string messageFormat,
        DiagnosticSeverity severity,
        Func<ISymbol, bool> condition = null)
    {
        DiagnosticId = diagnosticId;
        MemberName = memberName;
        MessageFormat = messageFormat;
        Severity = severity;
        Condition = condition;
    }
}