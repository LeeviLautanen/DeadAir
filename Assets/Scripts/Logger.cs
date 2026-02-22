public enum LogLevel
{
    Log,
    Warning,
    Error
}

public class Logger
{
    private readonly bool enabled;
    private readonly LogLevel level;

    public Logger(bool enabled = true, LogLevel level = LogLevel.Log)
    {
        this.enabled = enabled;
        this.level = level;
    }

    public void Log(string message)
    {
        if (!enabled) return;
        UnityEngine.Debug.Log(message);
    }

    public void Warning(string message)
    {
        if (!enabled || level < LogLevel.Warning) return;
        UnityEngine.Debug.LogWarning(message);
    }

    public void Error(string message)
    {
        if (!enabled || level < LogLevel.Error) return;
        UnityEngine.Debug.LogError(message);
    }
}
