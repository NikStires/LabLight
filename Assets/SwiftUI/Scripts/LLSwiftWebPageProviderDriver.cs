using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class LLSwiftWebPageProviderDriver : IWebPageProvider
{
    public void OpenWebPage(string url)
    {
        Debug.Log("OpenWebPage: " + url);
        OpenSwiftSafariWindow(url);
    }

    #if UNITY_VISIONOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void OpenSwiftSafariWindow(string urlString);
    #else
    static void OpenSwiftSafariWindow(string urlString) {}
    #endif
}
