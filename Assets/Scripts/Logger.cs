public enum LogLevel
{
    Info,
    Warning,
    Error
}

public class Logger
{
    private readonly bool enabled;
    private readonly LogLevel level;

    public Logger(bool enabled = true, LogLevel level = LogLevel.Info)
    {
        this.enabled = enabled;
        this.level = level;
    }

    public void Info(object message)
    {
        if (!enabled || level > LogLevel.Info) return;
        UnityEngine.Debug.Log(message);
    }

    public void Warning(object message)
    {
        if (!enabled || level > LogLevel.Warning) return;
        UnityEngine.Debug.LogWarning(message);
    }

    public void Error(object message)
    {
        if (!enabled || level > LogLevel.Error) return;
        UnityEngine.Debug.LogError(message);
    }
}
