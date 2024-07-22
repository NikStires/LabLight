using System.IO;
using UnityEngine;

public class Config
{
    // NULL hostname means not loaded
    private static string _hostname;

    public static string Hostname
    {
        get
        {
            if (_hostname == null)
            {
                var hostPath = Path.Combine(Application.persistentDataPath, "host.txt");

#if UNITY_EDITOR
                var defaultHost = "127.0.0.1";
#else
        var defaultHost = "127.0.0.2";
#endif

                _hostname = File.Exists(hostPath) ? File.ReadAllText(hostPath).Trim() : defaultHost;
            }
            return _hostname;
        }
        set
        {
            var hostPath = Path.Combine(Application.persistentDataPath, "host.txt");
            _hostname = value;
            File.WriteAllText(hostPath, _hostname);
        }
    }

    public static string GetResourcePath(string resource, string hostname = null)
    {
        return GetResourcePathHostname(resource, hostname == null ? Hostname : hostname);
    }

    public static string GetResourcePathHostname(string resource, string hostname)
    {
        return "http://" + hostname + ":8000" + resource;
    }
}