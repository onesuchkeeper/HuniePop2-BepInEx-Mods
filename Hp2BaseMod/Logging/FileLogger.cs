using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Hp2BaseMod
{
    /// <summary>
    /// A log for modded info.
    /// </summary>
    public class FileLog : ILogger, IDisposable
    {
        /// <summary>
        /// If debug messages should be printed to the log.
        /// </summary>
        public bool ShowDebug { get; set; } = false;

        /// <summary>
        /// How indented the log messages will be.
        /// </summary>
        private int _indent;

        private readonly List<string> _errors = new();

        private readonly StreamWriter _writer;
        private readonly object _lock = new();

        public FileLog(string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            _writer = new StreamWriter(path, append: true)
            {
                AutoFlush = true
            };
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _writer.Dispose();
            }
        }

        public ILogger.Indent MakeIndent(string title = null)
        {
            if (title != null)
                Message(title);

            return new ILogger.Indent(this);
        }

        /// <summary>
        /// Increases how indented the log messages will be.
        /// </summary>
        public void IncreaseIndent()
        {
            _indent++;
        }

        /// <summary>
        /// Decreases how indented the log messages will be.
        /// </summary>
        public void DecreaseIndent()
        {
            _indent = Math.Max(0, _indent - 1);
        }

        public void Debug(string message)
        {
            if (ShowDebug)
                Message(message);
        }

        /// <summary>
        /// Outputs a formatted error message to the log.
        /// </summary>
        public void Error(string message)
        {
            _errors.Add(message);

            var lines = message.Split(
                [Environment.NewLine],
                StringSplitOptions.None);

            var tab = new string('-', Math.Max(1, (_indent * 2) - 5));

            foreach (var line in lines)
                WriteLine($"-ERROR{tab} {line}");
        }

        /// <summary>
        /// Outputs a formatted error message to the log.
        /// </summary>
        public void Error(string context, Exception exception)
        {
            var lines = exception
                .ToString()
                .Split([Environment.NewLine], StringSplitOptions.None);

            var tab = new string('-', Math.Max(1, (_indent * 2) - 5));

            foreach (var line in lines.Prepend(context))
                WriteLine($"-ERROR{tab} {line}");
        }

        /// <summary>
        /// For debugging, logs an error if the target is null.
        /// Returns true if the target is null, false otherwise.
        /// </summary>
        public bool IsNull(object target, string name = null)
        {
            var isNull = target == null;

            if (isNull)
                Error($"{name} is null");

            return isNull;
        }

        /// <summary>
        /// Outputs to the log.
        /// </summary>
        public void Message([CallerMemberName] string line = "")
        {
            if (line == null)
                line = "null";

            var lines = line.Split(
                [Environment.NewLine],
                StringSplitOptions.None);

            var tab = new string(' ', _indent * 2);

            foreach (var l in lines)
                WriteLine(tab + l);
        }

        /// <summary>
        /// Outputs to the log.
        /// </summary>
        public void Message(IEnumerable values)
        {
            Message(values == null ? null : $"[{string.Join(", ", values)}]");
        }

        public void Warning(string line)
        {
            if (line == null)
                line = "null";

            var lines = line.Split(
                [Environment.NewLine],
                StringSplitOptions.None);

            var tab = new string(' ', _indent * 2);

            foreach (var l in lines)
                WriteLine($"-WARNING {tab}{l}");
        }

        public void LogMissingIdError(string descriptor, RelativeId id)
        {
            LogMissingIdError(descriptor, id.LocalId, id.SourceId);
        }

        public void LogMissingIdError(
            string descriptor,
            int localId,
            int SourceId)
        {
            Message(
                $"{descriptor} with local id {localId} and mod id {SourceId}, " +
                $"but no mod with that id exists. Make sure you're obtaining " +
                $"your mod ids correctly by looking the mod up from the " +
                $"{nameof(ModInterface)}.");
        }

        public void DisplayErrors()
        {
            if (_errors.Any())
            {
                ErrorPopup.Show(string.Join("\n\n", _errors));
                _errors.Clear();
            }
        }

        private void WriteLine(string line)
        {
            lock (_lock)
            {
                _writer.WriteLine(line);
            }
        }
    }
}