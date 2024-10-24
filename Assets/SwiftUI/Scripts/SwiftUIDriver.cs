using UnityEngine;
using System.Runtime.InteropServices;
using System;
using AOT;
using UniRx;
using System.Linq;
using Newtonsoft.Json;

public class SwiftUIDriver : IUIDriver, IDisposable
{
    private static SwiftUIDriver _instance;
    public static SwiftUIDriver Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SwiftUIDriver();
            }
            return _instance;
        }
    }

    private CompositeDisposable _disposables = new CompositeDisposable();

    public SwiftUIDriver()
    {
        SetNativeCallback(OnMessageReceived);
        // Remove Initialize() call from constructor
    }

    // Make Initialize public and call it after ensuring dependencies are ready
    public void Initialize()
    {
        if (ProtocolState.Instance != null)
        {
            _disposables.Add(ProtocolState.Instance.ProtocolStream.Subscribe(OnProtocolChange));
            _disposables.Add(ProtocolState.Instance.StepStream.Subscribe(OnStepChange));
            _disposables.Add(ProtocolState.Instance.ChecklistStream.Subscribe(OnCheckItemChange));
        }
        else
        {
            Debug.LogWarning("ProtocolState.Instance is null during SwiftUIDriver initialization");
        }

        var chatProvider = ServiceRegistry.GetService<ILLMChatProvider>();
        if (chatProvider != null)
        {
            chatProvider.OnResponse.AddListener(OnChatMessageReceived);
        }
        else
        {
            Debug.LogWarning("ILLMChatProvider is null during SwiftUIDriver initialization");
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
        var chatProvider = ServiceRegistry.GetService<ILLMChatProvider>();
        if (chatProvider != null)
        {
            chatProvider.OnResponse.RemoveListener(OnChatMessageReceived);
        }
    }

    // Swift UI Update methods
    public void OnProtocolChange(ProtocolDefinition protocol)
    {
        Debug.Log("######LABLIGHT SWIFTUIDRIVER OnProtocolChange: " + protocol.title);
        string protocolJson = JsonConvert.SerializeObject(protocol);
        SendMessageToSwiftUI($"protocolChange:{protocolJson}");
    }

    public void OnStepChange(ProtocolState.StepState stepState)
    {
        var currentStep = ProtocolState.Instance.CurrentStepDefinition;
        SendMessageToSwiftUI($"stepChange:{ProtocolState.Instance.CurrentStep}:{(stepState.SignedOff.Value ? 1 : 0)}");

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
        Debug.Log("######LABLIGHT SWIFTUIDRIVER sending auth status to Swift: " + isAuthenticated);
        SendMessageToSwiftUI($"authStatus:{isAuthenticated}");
    }

    //Swift UI Display methods
    public void DisplayProtocolMenu()
    {
        OpenSwiftUIWindow("ProtocolMenu");
    }
    
    public void DisplayTimer(int seconds)
    {
        OpenSwiftTimerWindow(seconds);
    }

    public void DisplayCalculator()
    {
        OpenSwiftUIWindow("Calculator");
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
    public void StepNavigationCallback(int stepIndex)
    {
        ProtocolState.Instance.SetStep(stepIndex);
    }

    public void ChecklistItemToggleCallback(int index, bool isChecked)
    {
        Debug.Log("check item " + index.ToString() + " " + isChecked.ToString());
    }

    public void ProtocolSelectionCallback(string protocolTitle)
    {
         ServiceRegistry.GetService<IProtocolDataProvider>().GetOrCreateProtocolDefinition(protocolTitle).First().Subscribe(protocol =>
        {
            Debug.Log(protocol.title + " loaded");
            ProtocolState.Instance.SetProtocolDefinition(protocol);
            SceneLoader.Instance.LoadSceneClean("Protocol");
        }, (e) =>
        {
            Debug.Log("Error fetching protocol from resources, checking local files");
            var lfdp = new LocalFileDataProvider();
            lfdp.LoadProtocolDefinitionAsync(protocolTitle).ToObservable<ProtocolDefinition>().Subscribe(protocol =>
            {
                ProtocolState.Instance.SetProtocolDefinition(protocol);
                SceneLoader.Instance.LoadSceneClean("Protocol");
            }, (e) =>
            {
                Debug.Log("Error fetching protocol from local files");
            });
        });
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
        Debug.Log("######LABLIGHT SWIFTUIDRIVER triggering Login Callback: " + username + " " + password);
        var authProvider = ServiceRegistry.GetService<IUserAuthProvider>();
        if (authProvider != null)
        {
            authProvider.TryAuthenticateUser(username, password)
                .ToObservable()
                .Subscribe(
                    result => {
                        // Handle successful authentication
                        SendAuthStatus(result);
                    },
                    error => {
                        // Handle authentication error
                        Debug.LogError("Authentication failed: " + error.Message);
                        SendAuthStatus(false);
                    }
                );
        }
        else
        {
            Debug.LogError("IUserAuthProvider is not available");
            SendAuthStatus(false);
        }
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
        Debug.Log("######LABLIGHT SWIFTUIDRIVER Message Recieved from SwiftUI " + message);
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
            case "selectProtocol":
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
                    Debug.Log("######LABLIGHT SWIFTUIDRIVER triggering Login Callback: " + loginData[0] + " " + loginData[1]);
                    LoginCallback(loginData[0], loginData[1]);
                }
                break;
            case "requestProtocolDescriptions":
                LoadProtocolDescriptions();
                break;
            // Add more cases as needed
        }
    }

    private async void LoadProtocolDescriptions()
    {
        var protocolDataProvider = ServiceRegistry.GetService<IProtocolDataProvider>();
        if (protocolDataProvider == null)
        {
            Debug.LogError("IProtocolDataProvider not found in ServiceRegistry");
            return;
        }

        try
        {
            var protocols = await protocolDataProvider.GetProtocolList();
            string protocolsJson = JsonConvert.SerializeObject(protocols);
            SendMessageToSwiftUI($"protocolDescriptions:{protocolsJson}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading protocol list: {ex.Message}");
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
    private static void OpenSwiftSafariWindow(string urlString) { Debug.Log($"OpenSwiftSafariWindow: {urlString}"); }
    private static void OpenSwiftVideoWindow(string videoTitle) { Debug.Log($"OpenSwiftVideoWindow: {videoTitle}"); }
    private static void OpenSwiftPdfWindow(string pdfUrlString) { Debug.Log($"OpenSwiftPdfWindow: {pdfUrlString}"); }
    #endif

    private delegate void CallbackDelegate(string command);
}