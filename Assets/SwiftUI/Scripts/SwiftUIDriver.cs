using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Collections.Generic;
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
    }

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
        SendMessageToSwiftUI($"protocolChange|{protocolJson}");
    }

    public void OnStepChange(ProtocolState.StepState stepState)
    {
        var stepStateData = new StepStateData
        {
            CurrentStepIndex = ProtocolState.Instance.CurrentStep.Value,
            IsSignedOff = stepState.SignedOff.Value,
            ChecklistState = stepState.Checklist?.Select(item => new CheckItemStateData
            {
                IsChecked = item.IsChecked.Value,
                CheckIndex = ProtocolState.Instance.CurrentStepState.Value.Checklist.IndexOf(item)
            }).ToList() ?? new List<CheckItemStateData>()
        };
        string stepStateJson = JsonConvert.SerializeObject(stepStateData);
        SendMessageToSwiftUI($"stepChange|{stepStateJson}");
    }

    public void OnCheckItemChange(List<ProtocolState.CheckItemState> checkItemStates)
    {
        if (checkItemStates == null) return;
        
        var checkItemStateDataList = checkItemStates.Select((checkItemState, index) => new CheckItemStateData
        {
            IsChecked = checkItemState.IsChecked.Value,
            CheckIndex = index
        }).ToList();

        string checkItemStatesJson = JsonConvert.SerializeObject(checkItemStateDataList);
        SendMessageToSwiftUI($"checkItemChange|{checkItemStatesJson}");

        var currentCheckItem = ProtocolState.Instance.CurrentCheckItemDefinition;
        if (currentCheckItem != null)
        {
            foreach(var arAction in currentCheckItem.arActions)
            {
                if (arAction.actionType == "Timer")
                {
                    if (arAction.properties.TryGetValue("duration", out object durationObj) && 
                        int.TryParse(durationObj.ToString(), out int seconds))
                    {
                        DisplayTimer(seconds);
                    }
                }
            }
        }
    }

    public void OnChatMessageReceived(string message)
    {
        SendMessageToSwiftUI($"LLMChatMessage|{message}");
    }

    public void SendAuthStatus(bool isAuthenticated)
    {
        Debug.Log("######LABLIGHT SWIFTUIDRIVER sending auth status to Swift: " + isAuthenticated);
        SendMessageToSwiftUI($"authStatus|{isAuthenticated}");
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

    public void CheckItemCallback(int index)
    {
        var currentStepState = ProtocolState.Instance.CurrentStepState.Value;
        if (currentStepState == null || currentStepState.Checklist == null)
        {
            return;
        }

        if (index < 0 || index >= currentStepState.Checklist.Count)
        {
            return;
        }

        currentStepState.Checklist[index].IsChecked.Value = true;
        currentStepState.Checklist[index].CompletionTime.Value = DateTime.Now;
        
        if(index + 1 < currentStepState.Checklist.Count)
        {
            ProtocolState.Instance.SetCheckItem(index + 1);
        }
        else
        {
            ProtocolState.Instance.SetCheckItem(index);
        }
    }

    public void UncheckItemCallback(int index)
    {
        ProtocolState.Instance.CurrentStepState.Value.Checklist[index].IsChecked.Value = false;
        if(index - 1 >= 0)
        {
            ProtocolState.Instance.SetCheckItem(index - 1);
        }
        else
        {
            ProtocolState.Instance.SetCheckItem(index);
        }
    }

    public void SignOffChecklistCallback()
    {
        ProtocolState.Instance.SignOff();
        var currentStep = ProtocolState.Instance.CurrentStepState.Value;
        var stepStateData = new StepStateData
        {
            CurrentStepIndex = ProtocolState.Instance.CurrentStep.Value,
            IsSignedOff = currentStep.SignedOff.Value,
            ChecklistState = currentStep.Checklist?.Select(item => new CheckItemStateData
            {
                IsChecked = item.IsChecked.Value,
                CheckIndex = currentStep.Checklist.IndexOf(item)
            }).ToList()
        };
        
        string json = JsonConvert.SerializeObject(stepStateData);
        SendMessageToSwiftUI($"stepChange|{json}");
    }

    public void ProtocolSelectionCallback(string protocolDescriptorJson)
    {
        try
        {
            var protocolDescriptor = JsonConvert.DeserializeObject<ProtocolDescriptor>(protocolDescriptorJson);
            ServiceRegistry.GetService<IProtocolDataProvider>().GetOrCreateProtocolDefinition(protocolDescriptor).First().Subscribe(protocol =>
            {
                Debug.Log(protocol.title + " loaded");
                ProtocolState.Instance.SetProtocolDefinition(protocol);
                SceneLoader.Instance.LoadSceneClean("Protocol");
            }, (e) =>
            {
                Debug.Log("Error fetching protocol from resources, checking local files");
                var lfdp = new LocalFileDataProvider();
                lfdp.LoadProtocolDefinitionAsync(protocolDescriptor).ToObservable<ProtocolDefinition>().Subscribe(protocol =>
                {
                    ProtocolState.Instance.SetProtocolDefinition(protocol);
                    SceneLoader.Instance.LoadSceneClean("Protocol");
                }, (e) =>
                {
                    Debug.Log("Error fetching protocol from local files");
                });
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deserializing protocol descriptor: {e.Message}");
        }
    }

    public void CloseProtocolCallback()
    {
        ProtocolState.Instance.ActiveProtocol.Value = null;
        SceneLoader.Instance.LoadSceneClean("ProtocolMenu");
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
                        SendAuthStatus(result);
                    },
                    error => {
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
        Debug.Log("######LABLIGHT SWIFTUIDRIVER Message Received from SwiftUI " + message);
        
        if (ProtocolState.Instance == null)
        {
            Debug.LogError("######LABLIGHT SWIFTUIDRIVER HandleMessage - ProtocolState.Instance is null");
            return;
        }
        
        string[] parts = message.Split('|');
        string command = parts[0];
        string data = parts.Length > 1 ? parts[1] : string.Empty;

        try
        {
            switch (command)
            {
                case "stepNavigation":
                    StepNavigationCallback(int.Parse(data));
                    break;
                case "checkItem":
                    CheckItemCallback(int.Parse(data));
                    break;
                case "uncheckItem":
                    UncheckItemCallback(int.Parse(data));
                    break;
                case "selectProtocol":
                    ProtocolSelectionCallback(data);
                    break;
                case "checklistSignOff":
                    SignOffChecklistCallback();
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
                case "requestVideo":
                    Debug.Log($"######LABLIGHT SWIFTUIDRIVER displaying video: {data}");
                    DisplayVideoPlayer(data);
                    break;
                case "requestPDF":
                    Debug.Log($"######LABLIGHT SWIFTUIDRIVER displaying PDF: {data}");
                    DisplayPDFReader(data);
                    break;
                case "requestTimer":
                    if (int.TryParse(data, out int seconds))
                    {
                        Debug.Log($"######LABLIGHT SWIFTUIDRIVER displaying timer: {seconds} seconds");
                        DisplayTimer(seconds);
                    }
                    else
                    {
                        Debug.LogError($"######LABLIGHT SWIFTUIDRIVER invalid timer duration: {data}");
                    }
                    break;
                case "requestWebpage":
                    Debug.Log($"######LABLIGHT SWIFTUIDRIVER opening webpage: {data}");
                    DisplayWebPage(data);
                    break;
                case "closeProtocol":
                    CloseProtocolCallback();
                    break;
                // Add more cases as needed
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"######LABLIGHT SWIFTUIDRIVER HandleMessage - Exception in command {command}: {ex.Message}\nStackTrace: {ex.StackTrace}");
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
            var protocolDescriptions = await protocolDataProvider.GetProtocolList();
            string protocolDescriptionsJson = JsonConvert.SerializeObject(protocolDescriptions);
            SendMessageToSwiftUI($"protocolDescriptions|{protocolDescriptionsJson}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading protocol list: {ex.Message}");
        }
    }

    public class StepStateData
    {
        public int CurrentStepIndex { get; set; }
        public bool IsSignedOff { get; set; }
        public List<CheckItemStateData> ChecklistState { get; set; }
    }

    public class CheckItemStateData
    {
        public bool IsChecked { get; set; }
        public int CheckIndex { get; set; }
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
