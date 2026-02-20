public class Logger
{
    public bool Enabled { get; set; } = true;

    public Logger(bool enabled = true)
    {
        Enabled = enabled;
    }

    public void Log(string message)
    {
        if (!Enabled) return;
        UnityEngine.Debug.Log(message);
    }

    public void Warning(string message)
    {
        if (!Enabled) return;
        UnityEngine.Debug.LogWarning(message);
    }

    public void Error(string message)
    {
        UnityEngine.Debug.LogError(message);
    }
}
