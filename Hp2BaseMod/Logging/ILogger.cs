using System;
using System.Collections;

namespace Hp2BaseMod;

public interface ILogger
{
    public class Indent : IDisposable
    {
        private readonly ILogger _modLog;

        public Indent(ILogger modLog)
        {
            _modLog = modLog;
            _modLog.IncreaseIndent();
        }

        public void Dispose()
        {
            _modLog.DecreaseIndent();
        }
    }

    /// <summary>
    /// If debug messages should be printed to the log
    /// </summary>
    public bool ShowDebug { get; }

    public Indent MakeIndent(string title = null);

    /// <summary>
    /// Increases how indented the log messages will be
    /// </summary>
    public void IncreaseIndent();

    /// <summary>
    /// Decreases how indented the log messages will be
    /// </summary>
    public void DecreaseIndent();

    public void Debug(string message);

    /// <summary>
    /// outputs a formatted error message to the log
    /// </summary>
    /// <param name="message"></param>
    public void Error(string message);

    /// <summary>
    /// outputs a formatted error message to the log
    /// </summary>
    /// <param name="message"></param>
    public void Error(string context, Exception exception);

    /// <summary>
    /// for debugging, logs an error if the target is null.
    /// Returns true if the target is null, false otherwise.
    /// </summary>
    /// <param name="line"></param>
    public bool IsNull(object target, string name = null);

    /// <summary>
    /// outputs to the log
    /// </summary>
    /// <param name="line"></param>
    public void Message([System.Runtime.CompilerServices.CallerMemberName] string line = "");

    /// <summary>
    /// outputs to the log
    /// </summary>
    /// <param name="line"></param>
    public void Message(IEnumerable values);

    public void Warning(string line);

    public void LogMissingIdError(string descriptor, RelativeId id);

    public void LogMissingIdError(string descriptor, int localId, int SourceId);

    public void DisplayErrors();
}