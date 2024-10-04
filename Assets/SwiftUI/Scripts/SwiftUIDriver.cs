using UnityEngine;
using System.Runtime.InteropServices;
using System;
using AOT;

public class SwiftUIDriver : MonoBehaviour, IUIDriver
{
    private static SwiftUIDriver _instance;
    public static SwiftUIDriver Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SwiftUIDriver>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SwiftUIDriver");
                    _instance = go.AddComponent<SwiftUIDriver>();
                }
            }
            return _instance;
        }
    }

    // Unity Callback Methods
    private Action<int> stepNavigationCallback;
    private Action<int, bool> checklistItemToggleCallback;
    private Action<string> protocolSelectionCallback;
    private Action<bool> checklistSignOffCallback;
    private Action<string> chatMessageCallback;
    private Action<string, string> loginCallback;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        SetNativeCallback(OnMessageReceived);
    }

    // Swift UI Update methods
    public void OnProtocolChange(string protocolJson)
    {
        SendMessageToSwiftUI($"protocolChange:{protocolJson}");
    }

    public void OnStepChange(int index, bool isSignedOff)
    {
        SendMessageToSwiftUI($"stepChange:{index}:{isSignedOff}");
    }

    public void OnCheckItemChange(int index)
    {
        SendMessageToSwiftUI($"checkItemChange:{index}");
    }

    public void SendChatMessage(string message)
    {
        SendMessageToSwiftUI($"sendChatMessage:{message}");
    }

    public void SendAuthStatus(bool isAuthenticated)
    {
        SendMessageToSwiftUI($"authStatus:{isAuthenticated}");
    }

    //Swift UI Display methods
    public void DisplayTimer(int seconds)
    {
        OpenSwiftTimerWindow(seconds);
    }

    public void DisplayCalculator()
    {
        OpenSwiftCalculatorWindow();
    }

    public void DisplayWebPage(string url)
    {
        OpenSwiftSafariWindow(url);
    }

    public void DisplayLLMChat()
    {
        OpenSwiftUIWindow("LLMChat");
    }

    public void DisplayVideoPlayer(string url)
    {
        OpenSwiftVideoWindow(url);
    }

    public void DisplayPDFReader(string url)
    {
        OpenSwiftPdfWindow(url);
    }

    // Input Handling Methods
    public void SetStepNavigationCallback(Action<int> callback)
    {
        stepNavigationCallback = callback;
    }

    public void SetChecklistItemToggleCallback(Action<int, bool> callback)
    {
        checklistItemToggleCallback = callback;
    }

    public void SetProtocolSelectionCallback(Action<string> callback)
    {
        protocolSelectionCallback = callback;
    }

    public void SetChecklistSignOffCallback(Action<bool> callback)
    {
        checklistSignOffCallback = callback;
    }

    public void SetChatMessageCallback(Action<string> callback)
    {
        chatMessageCallback = callback;
    }

    public void SetLoginCallback(Action<string, string> callback)
    {
        loginCallback = callback;
    }

    // Native callback handler
    [MonoPInvokeCallback(typeof(CallbackDelegate))]
    private static void OnMessageReceived(string message)
    {
        Instance.HandleMessage(message);
    }

    private void HandleMessage(string message)
    {
        string[] parts = message.Split(':');
        if (parts.Length < 2) return;

        string command = parts[0];
        string data = parts[1];

        switch (command)
        {
            case "stepNavigation":
                stepNavigationCallback?.Invoke(int.Parse(data));
                break;
            case "checklistItemToggle":
                string[] toggleData = data.Split(',');
                checklistItemToggleCallback?.Invoke(int.Parse(toggleData[0]), bool.Parse(toggleData[1]));
                break;
            case "protocolSelection":
                protocolSelectionCallback?.Invoke(data);
                break;
            case "checklistSignOff":
                checklistSignOffCallback?.Invoke(bool.Parse(data));
                break;
            case "sendMessage":
                chatMessageCallback?.Invoke(data);
                break;
            case "login":
                string[] loginData = data.Split(',');
                if (loginData.Length == 2)
                {
                    loginCallback?.Invoke(loginData[0], loginData[1]);
                }
                break;
            // Add more cases as needed
        }
    }

    // DllImports
    #if UNITY_VISIONOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SendMessageToSwiftUI(string message);

    [DllImport("__Internal")]
    private static extern void SetNativeCallback(CallbackDelegate callback);

    [DllImport("__Internal")]
    private static extern void OpenSwiftUIWindow(string name);

    [DllImport("__Internal")]
    private static extern void CloseSwiftUIWindow(string name);

    [DllImport("__Internal")]
    private static extern void OpenSwiftTimerWindow(int duration);

    [DllImport("__Internal")]
    private static extern void OpenSwiftCalculatorWindow();

    [DllImport("__Internal")]
    private static extern void OpenSwiftSafariWindow(string urlString);

    [DllImport("__Internal")]
    private static extern void OpenSwiftVideoWindow(string videoTitle);

    [DllImport("__Internal")]
    private static extern void OpenSwiftPdfWindow(string pdfUrlString);
    #else
    private static void SendMessageToSwiftUI(string message) { Debug.Log($"SendMessageToSwiftUI: {message}"); }
    private static void SetNativeCallback(CallbackDelegate callback) { }
    private static void OpenSwiftUIWindow(string name) { Debug.Log($"OpenSwiftUIWindow: {name}"); }
    private static void CloseSwiftUIWindow(string name) { Debug.Log($"CloseSwiftUIWindow: {name}"); }
    private static void OpenSwiftTimerWindow(int duration) { Debug.Log($"OpenSwiftTimerWindow: {duration}"); }
    private static void OpenSwiftCalculatorWindow() { Debug.Log("OpenSwiftCalculatorWindow"); }
    private static void OpenSwiftSafariWindow(string urlString) { Debug.Log($"OpenSwiftSafariWindow: {urlString}"); }
    private static void OpenSwiftVideoWindow(string videoTitle) { Debug.Log($"OpenSwiftVideoWindow: {videoTitle}"); }
    private static void OpenSwiftPdfWindow(string pdfUrlString) { Debug.Log($"OpenSwiftPdfWindow: {pdfUrlString}"); }
    #endif

    private delegate void CallbackDelegate(string command);
}