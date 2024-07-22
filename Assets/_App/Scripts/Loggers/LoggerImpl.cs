/// <summary>
/// Abstract Logger implementation with optional chaining of loggers
/// </summary>
public abstract class LoggerImpl
{
    LoggerImpl _next;
    public void SetNext(LoggerImpl next)
    {
        _next = next;
    }

    // Helper
    protected string Now
    {
        get
        {
            return System.DateTime.Now.ToString("HH:mm:ss");
        }
    }

    protected void LogNext(string what)
    {
        if (_next != null)
        {
            _next.Log(what);
        }
    }

    protected void LogErrorNext(string what)
    {
        if (_next != null)
        { 
            _next.LogError(what); 
        }
    }

    abstract public void Log(string what);
    abstract public void LogError(string what);
}