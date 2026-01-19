using AssetStudio;
using Hp2BaseMod;

public class Logger : ILogger
{
    public bool ShowDebug { get; set; }
    public bool ShowVerbose { get; set; }

    public void Message(string value) => ModInterface.Log.Message(value);

    public void Error(string value) => ModInterface.Log.Error(value);

    public void Warning(string value) => ModInterface.Log.Warning(value);

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
