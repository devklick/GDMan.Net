using GDMan.Core.Infrastructure;

public class ConsoleLogger
{
    private readonly LogLevel _minLogLevel;

    public ConsoleLogger(LogLevel minLogLevel)
    {
        _minLogLevel = minLogLevel;
    }

    public void LogInformation(string message)
        => Log(LogLevel.Information, message);

    public void LogTrace(string message)
        => Log(LogLevel.Trace, message);

    public void LogDebug(string message)
        => Log(LogLevel.Debug, message);

    public void LogError(string message)
        => Log(LogLevel.Error, message);

    public void LogFatal(string message)
        => Log(LogLevel.Fatal, message);

    public void LogWarning(string message)
        => Log(LogLevel.Warning, message);

    public void Log(LogLevel level, string message)
    {
        if (level < _minLogLevel) return;
        Console.WriteLine(message);
    }
}