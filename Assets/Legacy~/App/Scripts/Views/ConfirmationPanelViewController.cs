using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UniRx;
using TMPro;


public class ConfirmationPanelViewController : MonoBehaviour
{
    public Interactable confirmButton;
    public Interactable denyButton;
    public TextMeshProUGUI confirmationText;

    public Transform emailEntryPanel;

    private IAudio audioPlayer;
    private Action disposeVoice;

    void Awake()
    {
        audioPlayer = ServiceRegistry.GetService<IAudio>();
    }

    void OnDisable()
    {
        confirmButton.OnClick.RemoveAllListeners();
        denyButton.OnClick.RemoveAllListeners();

        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    void OnEnable()
    {
        audioPlayer?.Play(AudioEventEnum.Error);
    }

    public void ChecklistIncompleteMessage()
    {
        confirmationText.text = "The checklist has not been completed. Would you still like to continue to the next step?";
        this.gameObject.SetActive(true);
        SessionState.ConfirmationPanelVisible.Value = true;
        confirmButton.OnClick.AsObservable().Subscribe(_ => confirmStepNavigation()).AddTo(this);
        denyButton.OnClick.AsObservable().Subscribe(_ => denyStepNavigation()).AddTo(this);
    }

    public void SignOffMessage()
    {
        confirmationText.text = "Looks like all steps are complete, would you like to sign off and continue to the next step?";
        this.gameObject.SetActive(true);
        SessionState.ConfirmationPanelVisible.Value = true;
        confirmButton.OnClick.AsObservable().Subscribe(_ => confirmStepNavigation()).AddTo(this);
        denyButton.OnClick.AsObservable().Subscribe(_ => denyStepNavigation()).AddTo(this);
    }

    public void CriticalStepMessage()
    {
        //update confirmation panel UI and activate it
        confirmationText.text = "You have begun a critical step, would you like to start recording?";
        this.gameObject.SetActive(true);
        SessionState.ConfirmationPanelVisible.Value = true;
        confirmButton.OnClick.AsObservable().Subscribe(_ => confirmCriticalStepRecording()).AddTo(this);
        denyButton.OnClick.AsObservable().Subscribe(_ => denyStepNavigation()).AddTo(this);
    }

    //Confirmation panel popup button events

    void confirmStepNavigation()
    {
        this.gameObject.SetActive(false);
        SessionState.ConfirmationPanelVisible.Value = false;
        confirmButton.OnClick.RemoveAllListeners();
        denyButton.OnClick.RemoveAllListeners();

        //if all items are checked sign off before continuing to the next step
        if (ProtocolState.CheckItem == ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1 &&
            ProtocolState.Steps[ProtocolState.Step].Checklist[ProtocolState.CheckItem].IsChecked.Value)
        {
            ProtocolState.Steps[ProtocolState.Step].SignedOff = true;
        }

        audioPlayer?.Play(AudioEventEnum.NextStep);
        ProtocolState.SetStep(ProtocolState.Step + 1);

        //if this is the final step of the procedure and there is no checklist send the CSV
        if ((ProtocolState.Step + 1) == ProtocolState.Steps.Count & ProtocolState.Steps[ProtocolState.Step].Checklist == null)
        {
            emailEntryPanel.gameObject.SetActive(true);
        }
    }

    void denyStepNavigation()
    {
        this.gameObject.SetActive(false);
        SessionState.ConfirmationPanelVisible.Value = false;
        confirmButton.OnClick.RemoveAllListeners();
        denyButton.OnClick.RemoveAllListeners();
    }

    void confirmCriticalStepRecording()
    {
        this.gameObject.SetActive(false);
        SessionState.ConfirmationPanelVisible.Value = false;
        confirmButton.OnClick.RemoveAllListeners();
        denyButton.OnClick.RemoveAllListeners();
        ServiceRegistry.GetService<ILighthouseControl>()?.StartRecordingVideo();
    }

    void denyCriticalStepRecording()
    {
        this.gameObject.SetActive(false);
        SessionState.ConfirmationPanelVisible.Value = false;
        confirmButton.OnClick.RemoveAllListeners();
        denyButton.OnClick.RemoveAllListeners();
    }
}
