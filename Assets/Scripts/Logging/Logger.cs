using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public enum LogLevel
{
    Info,
    Warning,
    Error
}

public class Logger
{
    private readonly string key;
    private readonly Dictionary<string, LogLevel> config = null;

    public Logger(string key)
    {
#if UNITY_EDITOR
        this.key = key;

        string configPath = Application.dataPath + "/Scripts/Logging/logger_config.json";

        if (!System.IO.File.Exists(configPath))
        {
            Debug.LogError($"Logger configuration file not found at path: {configPath}");
            return;
        }

        string json = System.IO.File.ReadAllText(configPath);
        config = JsonConvert.DeserializeObject<Dictionary<string, LogLevel>>(json);
#endif
    }

    private bool Enabled(LogLevel msgLevel)
    {
        if (config == null)
        {
            return false;
        }

        var level = config.TryGetValue(key, out var lvl) ? lvl : LogLevel.Error;
        return msgLevel >= level;
    }

    [HideInCallstack]
    public void Info(object message)
    {
        if (!Enabled(LogLevel.Info)) return;
        Debug.Log(message);
    }

    [HideInCallstack]
    public void Warning(object message)
    {
        if (!Enabled(LogLevel.Warning)) return;
        Debug.LogWarning(message);
    }

    [HideInCallstack]
    public void Error(object message)
    {
        if (!Enabled(LogLevel.Error)) return;
        Debug.LogError(message);
    }
}
