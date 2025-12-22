using System;
using Microsoft.CodeAnalysis;

namespace Hp2BaseMod.Analyzer
{
    internal sealed class MemberRule
    {
        public string DiagnosticId { get; }
        public string MessageFormat { get; }
        public DiagnosticSeverity Severity { get; }
        public Func<ISymbol, bool> Condition { get; }

        public MemberRule(
            string diagnosticId,
            string messageFormat,
            DiagnosticSeverity severity,
            Func<ISymbol, bool> condition = null)
        {
            DiagnosticId = diagnosticId;
            MessageFormat = messageFormat;
            Severity = severity;
            Condition = condition;
        }
    }
}