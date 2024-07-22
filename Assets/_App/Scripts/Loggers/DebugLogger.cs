using UnityEngine;

/// <summary>
/// Logger implementation that logs Unity Debug logger
/// </summary>
public class DebugLogger : LoggerImpl
{
    public override void Log(string what)
    {
        Debug.Log(what);
        LogNext(what);
    }

    public override void LogError(string what)
    {
        Debug.LogError("ERROR: " + what);
        LogErrorNext(what);
    }
}