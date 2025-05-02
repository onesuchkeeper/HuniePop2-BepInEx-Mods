using System;
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
        public class ModLogIndent : IDisposable
        {
            private readonly ModLog _modLog;

            public ModLogIndent(ModLog modLog)
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

        public ModLogIndent MakeIndent() => new ModLogIndent(this);

        /// <summary>
        /// Increases how indented the log messages will be
        /// </summary>
        public void IncreaseIndent()// => _indent++;
        {
            //LogLine("{");
            _indent++;
        }
        /// <summary>
        /// Decreases how indented the log messages will be
        /// </summary>
        public void DecreaseIndent()// => _indent = Math.Max(0, _indent - 1);
        {
            _indent = Math.Max(0, _indent - 1);
            //LogLine("}");
        }

        /// <summary>
        /// outputs a formatted error message to the log
        /// </summary>
        /// <param name="message"></param>
        public void LogError(string message)
        {
            var lines = message.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
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
        public void LogError(string context, Exception exception)
        {
            IEnumerable<string> lines = exception.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

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
        public bool LogIsNull(object target, string name = null)
        {
            var isNull = target == null;
            if (isNull)
            {
                LogError($"{name} is null");
            }
            return isNull;
        }

        /// <summary>
        /// outputs to the log
        /// </summary>
        /// <param name="line"></param>

        public void LogInfo([System.Runtime.CompilerServices.CallerMemberName] string line = "")
        {
            if (line == null) { line = "null"; }

            var lines = line.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var tab = new string(' ', _indent * 2);

            foreach (var l in lines)
            {
                _logSource.LogInfo(tab + l);
            }
        }

        public void LogWarning(string line)
        {
            if (line == null) { line = "null"; }

            var lines = line.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var tab = new string(' ', _indent * 2);

            foreach (var l in lines)
            {
                _logSource.LogWarning(tab + l);
            }
        }

        /// <summary>
        /// outputs a formatted title to the log
        /// </summary>
        /// <param name="line"></param>
        public void LogTitle(string line)
        {
            LogInfo($"-----{line}-----");
        }

        public void LogMissingIdError(string descriptor, RelativeId id) => LogMissingIdError(descriptor, id.LocalId, id.SourceId);

        public void LogMissingIdError(string descriptor, int localId, int SourceId) => LogInfo($"{descriptor} with local id {localId} and mod id {SourceId}, but no mod with that id exists. Make sure you're obtaining your mod ids correctly by looking the mod up from the {nameof(ModInterface)}.");
    }
}
