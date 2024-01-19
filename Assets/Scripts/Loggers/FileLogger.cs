using System.IO;

/// <summary>
/// Logger implementation that logs to file
/// </summary>
class FileLogger : LoggerImpl
{
    StreamWriter writer;

    public FileLogger(string filePath)
    {
        writer = new StreamWriter(filePath);
    }

    public override void Log(string what)
    {
        writer.Write(string.Format("[{0}] {1}\n", Now, what));
        writer.Flush();
        LogNext(what);
    }

    public override void LogError(string what)
    {
        writer.Write(string.Format("[{0}] ERROR {1}\n", Now, what));
        writer.Flush();
        LogErrorNext(what);
    }
}