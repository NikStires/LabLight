using UniRx;

/// <summary>
/// Logger implementation that logs to http server
/// </summary>
class AcamMailboxLogger : LoggerImpl
{
    IHttp http;
    string url;

    public AcamMailboxLogger(IHttp http, string url)
    {
        this.http = http;
        this.url = url;
    }

    public override void Log(string what)
    {
        http.Post(url, string.Format("[{0}] {1}", Now, what)).Subscribe();
        LogNext(what);
    }

    public override void LogError(string what)
    {
        http.Post(url, string.Format("[{0}] ERROR {1}", Now, what)).Subscribe();
        LogErrorNext(what);
    }
}