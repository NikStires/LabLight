using UnityEngine;
using System.Runtime.InteropServices;
using System;
using AOT;
using UniRx;
using System.Linq;

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

    void Start()
    {
        ProtocolState.Instance.ProtocolStream.Subscribe(OnProtocolChange);
        ProtocolState.Instance.StepStream.Subscribe(OnStepChange);
        ProtocolState.Instance.ChecklistStream.Subscribe(OnCheckItemChange);
        ServiceRegistry.GetService<ILLMChatProvider>().OnResponse.AddListener(OnChatMessageReceived);
    }

    // Swift UI Update methods
    public void OnProtocolChange(string protocolJson)
    {
        SendMessageToSwiftUI($"protocolChange:{protocolJson}");
    }

    public void OnStepChange(ProtocolState.StepState stepState)
    {
        var currentStep = ProtocolState.Instance.CurrentStepDefinition;
        SendMessageToSwiftUI($"stepChange:{ProtocolState.Instance.CurrentStep}:{stepState.SignedOff.Value}");

        foreach(var contentItem in currentStep.contentItems)
        {
            switch(contentItem.contentType)
            {
                case ContentType.Video:
                    var videoItem = (VideoItem)contentItem;
                    DisplayVideoPlayer(videoItem.url);
                    break;
            }
        }
    }

    public void OnCheckItemChange(int index)
    {
        var currentCheckItem = ProtocolState.Instance.CurrentCheckItemDefinition;
        SendMessageToSwiftUI($"checkItemChange:{index}");

        if(ProtocolState.Instance.HasCurrentCheckItem())
        {
            if(currentCheckItem.activateTimer)
            {
                int seconds = currentCheckItem.hours * 3600 + currentCheckItem.minutes * 60 + currentCheckItem.seconds;
                DisplayTimer(seconds);
            }
            foreach(var contentItem in currentCheckItem.contentItems)
            {
                switch(contentItem.contentType)
                {
                    case ContentType.Video:
                        var videoItem = (VideoItem)contentItem;
                        DisplayVideoPlayer(videoItem.url);
                        break;
                }
            }
        }
    }

    public void OnChatMessageReceived(string message)
    {
        SendMessageToSwiftUI($"LLMChatMessage:{message}");
    }

    public void SendAuthStatus(bool isAuthenticated)
    {
        Debug.Log("######LABLIGHT sending auth status to Swift: " + isAuthenticated);
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

    // Unity Callback Methods
    public void StepNavigationCallback(int navigationDirection)
    {
        ProtocolState.Instance.SetStep(ProtocolState.Instance.CurrentStep.Value + navigationDirection);
    }

    public void ChecklistItemToggleCallback(int index, bool isChecked)
    {
        Debug.Log("check item " + index.ToString() + " " + isChecked.ToString());
    }

    public void ProtocolSelectionCallback(string protocolJSON)
    {
        Debug.Log(protocolJSON);
    }

    public void ChecklistSignOffCallback(bool isSignedOff)
    {
        if(isSignedOff)
        {
            ProtocolState.Instance.SignOff();
        }
    }

    public void ChatMessageCallback(string message)
    {
        ServiceRegistry.GetService<ILLMChatProvider>().QueryAsync(message);
    }

    public void LoginCallback(string username, string password)
    {
        StartCoroutine(ServiceRegistry.GetService<IUserAuthProvider>().TryAuthenticateUser(username, password));
    }

    // Native callback handler
    [MonoPInvokeCallback(typeof(CallbackDelegate))]
    private static void OnMessageReceived(string message)
    {
        Instance.HandleMessage(message);
    }

    //Handle message passing from SwiftUI
    private void HandleMessage(string message)
    {
        Debug.Log("######LABLIGHT Message Recieved from SwiftUI " + message);
        string[] parts = message.Split(':');
        if (parts.Length < 2) return;

        string command = parts[0];
        string data = parts[1];

        switch (command)
        {
            case "stepNavigation":
                StepNavigationCallback(int.Parse(data));
                break;
            case "checklistItemToggle":
                string[] toggleData = data.Split(',');
                ChecklistItemToggleCallback(int.Parse(toggleData[0]), bool.Parse(toggleData[1]));
                break;
            case "protocolSelection":
                ProtocolSelectionCallback(data);
                break;
            case "checklistSignOff":
                ChecklistSignOffCallback(bool.Parse(data));
                break;
            case "sendMessage":
                ChatMessageCallback(data);
                break;
            case "login":
                string[] loginData = data.Split(',');
                if (loginData.Length == 2)
                {
                    Debug.Log("######LABLIGHT triggering Login Callback: " + loginData[0] + " " + loginData[1]);
                    LoginCallback(loginData[0], loginData[1]);
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