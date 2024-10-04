using UnityEngine;
using System;

public interface IUIDriver
{
    //UI Update methods
    void OnProtocolChange(string protocolJson);
    void OnStepChange(int index, bool isSignedOff);
    void OnCheckItemChange(int index);
    void SendChatMessage(string message);
    void SendAuthStatus(bool isAuthenticated);

    //UI Display methods
    void DisplayTimer(int seconds);
    void DisplayCalculator();
    void DisplayWebPage(string url);
    void DisplayLLMChat();
    void DisplayVideoPlayer(string url);
    void DisplayPDFReader(string url);

    //Unity Callback Methods
    void SetStepNavigationCallback(System.Action<int> callback);
    void SetChecklistItemToggleCallback(System.Action<int, bool> callback);
    void SetProtocolSelectionCallback(System.Action<string> callback);
    void SetChecklistSignOffCallback(System.Action<bool> callback);
    void SetChatMessageCallback(Action<string> callback);
    void SetLoginCallback(Action<string, string> callback);
}
