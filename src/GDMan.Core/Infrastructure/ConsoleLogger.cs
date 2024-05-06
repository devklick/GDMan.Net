namespace GDMan.Core.Infrastructure;

public class ConsoleLogger(LogLevel minLogLevel = LogLevel.Information)
{
    private readonly LogLevel _minLogLevel = minLogLevel;

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

    public void LogFatal(Exception ex)
        => Log(LogLevel.Fatal, $"{ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}");

    public void LogWarning(string message)
        => Log(LogLevel.Warning, message);

    public void Log(LogLevel level, string message)
    {
        if (level < _minLogLevel) return;

        var backOld = Console.BackgroundColor;
        var frontOld = Console.ForegroundColor;

        if (level == LogLevel.Warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        else if (level >= LogLevel.Error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        Console.WriteLine(message);

        Console.BackgroundColor = backOld;
        Console.ForegroundColor = frontOld;
    }
}