using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hp2BaseMod
{
    /// <summary>
    /// Writes formatted log messages to a text stream.
    /// </summary>
    public class StreamLog : ILogger, IDisposable
    {
        /// <summary>
        /// If debug messages should be written.
        /// </summary>
        public bool ShowDebug { get; set; }

        /// <summary>
        /// How indented log messages will be.
        /// </summary>
        private int _indent;

        private readonly List<string> _errors = new();
        private readonly TextWriter _writer;
        private readonly bool _leaveOpen;

        public StreamLog(Stream stream, bool leaveOpen = false)
            : this(new StreamWriter(stream)
            {
                AutoFlush = true
            }, leaveOpen)
        {
        }

        public StreamLog(TextWriter writer, bool leaveOpen = false)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _leaveOpen = leaveOpen;
        }

        public ILogger.Indent MakeIndent(string title = null)
        {
            if (title != null) Message(title);

            return new ILogger.Indent(this);
        }

        public void IncreaseIndent() => _indent++;

        public void DecreaseIndent() => _indent = Math.Max(0, _indent - 1);

        public void Debug(string message)
        {
            if (ShowDebug) Message(message);
        }

        public void Error(string message)
        {
            _errors.Add(message);

            var tab = new string('-', Math.Max(1, (_indent * 2) - 5));

            foreach (var line in message.Split([Environment.NewLine], StringSplitOptions.None))
            {
                _writer.WriteLine($"-ERROR{tab} {line}");
            }

            _writer.Flush();
        }

        public void Error(string context, Exception exception)
        {
            var tab = new string('-', Math.Max(1, (_indent * 2) - 5));

            foreach (var line in exception
                .ToString()
                .Split([Environment.NewLine], StringSplitOptions.None)
                .Prepend(context))
            {
                _writer.WriteLine($"-ERROR{tab} {line}");
            }

            _writer.Flush();
        }

        public bool IsNull(object target, string name = null)
        {
            if (target != null) return false;

            Error($"{name ?? "Object"} is null");
            return true;
        }

        public void Message([System.Runtime.CompilerServices.CallerMemberName] string line = "")
        {
            line ??= "null";

            var tab = new string(' ', _indent * 2);

            foreach (var text in line.Split([Environment.NewLine], StringSplitOptions.None))
            {
                _writer.WriteLine(tab + text);
            }

            _writer.Flush();
        }

        public void Message(IEnumerable values)
        {
            Message(values == null
                ? null
                : $"[{string.Join(", ", values.Cast<object>())}]");
        }

        public void Warning(string message)
        {
            message ??= "null";

            var tab = new string(' ', _indent * 2);

            foreach (var line in message.Split([Environment.NewLine], StringSplitOptions.None))
            {
                _writer.WriteLine($"{tab}WARNING: {line}");
            }

            _writer.Flush();
        }

        public void LogMissingIdError(string descriptor, RelativeId id) =>
            LogMissingIdError(descriptor, id.LocalId, id.SourceId);

        public void LogMissingIdError(string descriptor, int localId, int sourceId)
        {
            Message(
                $"{descriptor} with local id {localId} and mod id {sourceId}, " +
                $"but no mod with that id exists. Make sure you're obtaining " +
                $"your mod ids correctly by looking the mod up from the " +
                $"{nameof(ModInterface)}.");
        }

        public void DisplayErrors()
        {
            if (!_errors.Any()) return;

            ErrorPopup.Show(string.Join("\n\n", _errors));
            _errors.Clear();
        }

        public void Dispose()
        {
            if (!_leaveOpen) _writer.Dispose();
        }
    }
}