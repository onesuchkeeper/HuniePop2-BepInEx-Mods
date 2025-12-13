using System;
using System.Linq;
using AssetStudio;

public class Logger : ILogger
{
    private class Indent : IDisposable
    {
        private Logger _parent;

        public Indent(Logger parent)
        {
            _parent = parent;
            _parent.IncreaseIndent();
        }

        public void Dispose()
        {
            _parent.DecreaseIndent();
        }
    }

    public bool ShowDebug { get; set; }
    public bool ShowVerbose { get; set; }

    private int _indentLevel = 0;

    private string _name;
    private ITextWriter _writer;

    public Logger(ITextWriter writer, string name = null)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _name = name ?? "Logger";
    }

    public void DecreaseIndent()
    {
        _indentLevel--;
        if (_indentLevel < 0) _indentLevel = 0;
    }

    public void IncreaseIndent() => _indentLevel++;

    public IDisposable MakeIndent() => new Indent(this);
    public IDisposable MakeIndent(string message)
    {
        _writer.SetColor(ConsoleColor.Green);
        Message(message);
        _writer.ResetColor();
        return MakeIndent();
    }

    public void Message(string value)
    {
        value ??= "null";
        value = $"[{_name}]\t{string.Concat(Enumerable.Repeat("\t", _indentLevel))}{value}";
        _writer.WriteLine(value);
    }

    public void Error(string value)
    {
        _writer.SetColor(ConsoleColor.Red);
        Message(value);
        _writer.ResetColor();
    }

    public void Warning(string value)
    {
        _writer.SetColor(ConsoleColor.Yellow);
        Message(value);
        _writer.ResetColor();
    }

    public void Log(LoggerEvent loggerEvent, string message)
    {
        switch (loggerEvent)
        {
            case LoggerEvent.Verbose:
                if (ShowVerbose) Message(message);
                break;
            case LoggerEvent.Debug:
                if (ShowDebug) Message(message);
                break;
            case LoggerEvent.Info:
                Message(message);
                break;
            case LoggerEvent.Warning:
                Warning(message);
                break;
            case LoggerEvent.Error:
                Error(message);
                break;
        }
    }
}