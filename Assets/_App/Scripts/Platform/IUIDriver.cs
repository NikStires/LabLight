using UnityEngine;
using System;

public interface IUIDriver
{
    //UI Update methods
    void OnProtocolChange(string protocolJson);
    void OnStepChange(ProtocolState.StepState stepState);
    void OnCheckItemChange(int index);
    void OnChatMessageReceived(string message);
    void SendAuthStatus(bool isAuthenticated);

    //UI Display methods 
    void DisplayProtocolMenu();
    void DisplayTimer(int seconds);
    void DisplayCalculator();
    void DisplayWebPage(string url);
    void DisplayLLMChat();
    void DisplayVideoPlayer(string url);
    void DisplayPDFReader(string url);

    //Unity Callback Methods
    void StepNavigationCallback(int navigationDirection); //-1 for previous, 1 for next
    void ChecklistItemToggleCallback(int index, bool isChecked);
    void ProtocolSelectionCallback(string protocolJSON);
    void ChecklistSignOffCallback(bool isSignedOff);
    void ChatMessageCallback(string message);
    void LoginCallback(string username, string password);
}
