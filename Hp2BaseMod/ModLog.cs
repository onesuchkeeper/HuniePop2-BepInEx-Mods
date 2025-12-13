using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;

namespace Hp2BaseMod
{
    /// <summary>
    /// A log for modded info
    /// </summary>
    public class ModLog
    {
        public class Indent : IDisposable
        {
            private readonly ModLog _modLog;

            public Indent(ModLog modLog)
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
        public bool ShowDebug { get; set; } = false;

        /// <summary>
        /// How indented the log messages will be
        /// </summary>
        private int _indent;

        /// <summary>
        /// loader log
        /// </summary>
        private readonly ManualLogSource _logSource;

        public ModLog(string sourceName)
        {
            _logSource = new ManualLogSource(sourceName);
            BepInEx.Logging.Logger.Sources.Add(_logSource);
        }

        public Indent MakeIndent(string title = null)
        {
            if (title != null) { this.Message(title); }
            return new Indent(this);
        }

        /// <summary>
        /// Increases how indented the log messages will be
        /// </summary>
        public void IncreaseIndent()// => _indent++;
        {
            _indent++;
        }

        /// <summary>
        /// Decreases how indented the log messages will be
        /// </summary>
        public void DecreaseIndent()// => _indent = Math.Max(0, _indent - 1);
        {
            _indent = Math.Max(0, _indent - 1);
        }

        public void Debug(string message)
        {
            if (ShowDebug) Message(message);
        }

        /// <summary>
        /// outputs a formatted error message to the log
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message)
        {
            var lines = message.Split([Environment.NewLine], StringSplitOptions.None);
            var tab = new string('-', Math.Max(1, (_indent * 2) - 5));

            foreach (var l in lines)
            {
                _logSource.LogError($"-ERROR{tab} {l}");
            }
        }

        /// <summary>
        /// outputs a formatted error message to the log
        /// </summary>
        /// <param name="message"></param>
        public void Error(string context, Exception exception)
        {
            IEnumerable<string> lines = exception.ToString().Split([Environment.NewLine], StringSplitOptions.None);

            var tab = new string('-', Math.Max(1, (_indent * 2) - 5));

            foreach (var l in lines.Prepend(context))
            {
                _logSource.LogError($"-ERROR{tab} {l}");
            }
        }

        /// <summary>
        /// for debugging, logs an error if the target is null.
        /// Returns true if the target is null, false otherwise.
        /// </summary>
        /// <param name="line"></param>
        public bool InNull(object target, string name = null)
        {
            var isNull = target == null;
            if (isNull)
            {
                Error($"{name} is null");
            }
            return isNull;
        }

        /// <summary>
        /// outputs to the log
        /// </summary>
        /// <param name="line"></param>
        public void Message([System.Runtime.CompilerServices.CallerMemberName] string line = "")
        {
            if (line == null) { line = "null"; }

            var lines = line.Split([Environment.NewLine], StringSplitOptions.None);
            var tab = new string(' ', _indent * 2);

            foreach (var l in lines)
            {
                _logSource.LogInfo(tab + l);
            }
        }

        /// <summary>
        /// outputs to the log
        /// </summary>
        /// <param name="line"></param>
        public void Message(IEnumerable values) => Message(values == null ? null : $"[{string.Join(", ", values)}]");

        public void Warning(string line)
        {
            if (line == null) { line = "null"; }

            var lines = line.Split([Environment.NewLine], StringSplitOptions.None);
            var tab = new string(' ', _indent * 2);

            foreach (var l in lines)
            {
                _logSource.LogWarning(tab + l);
            }
        }

        public void LogMissingIdError(string descriptor, RelativeId id) => LogMissingIdError(descriptor, id.LocalId, id.SourceId);

        public void LogMissingIdError(string descriptor, int localId, int SourceId) => Message($"{descriptor} with local id {localId} and mod id {SourceId}, but no mod with that id exists. Make sure you're obtaining your mod ids correctly by looking the mod up from the {nameof(ModInterface)}.");
    }
}
