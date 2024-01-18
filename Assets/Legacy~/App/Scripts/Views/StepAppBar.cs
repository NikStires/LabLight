using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Profiling;

public class StepAppBar : MonoBehaviour
{
    public Interactable previousStepButton;
    public Interactable nextStepButton;
    public Transform background;
    public TextMeshProUGUI stepText;

    public GameObject confirmationPanel;

    //public GameObject emailEntryPanel;

    private IAudio audioPlayer;
    private Action disposeVoice;
    private ConfirmationPanelViewController confirmationPanelVC;

    private void Awake()
    {
        audioPlayer = ServiceRegistry.GetService<IAudio>();
    }

    void Start()
    {
        if (confirmationPanel != null)
            confirmationPanelVC = confirmationPanel.GetComponent<ConfirmationPanelViewController>();
    }

    private void OnEnable()
    {
        SetupVoiceCommands();

        nextStepButton.OnClick.AsObservable().Subscribe(_ => nextStep()).AddTo(this);
        previousStepButton.OnClick.AsObservable().Subscribe(_ => previousStep()).AddTo(this);

        SessionState.modeStream.Subscribe(modeChanged).AddTo(this);
        ProtocolState.procedureDef.Subscribe(_ => UpdateVisualState()).AddTo(this);
        ProtocolState.stepStream.Subscribe(_ =>UpdateVisualState()).AddTo(this);

        SessionState.ConfirmationPanelVisible.Subscribe(value =>
        {
            if (value)
                disableStepButtons();
            else
                enableStepButtons();
        });

//        SessionState.Operations.ObserveCountChanged().Subscribe(_ => UpdateVisualState()).AddTo(this);
        UpdateVisualState();

        // Restore  app bar position
        var persistentLocation = GetComponent<PersistentLocation>();
        if (persistentLocation)
        {
            persistentLocation.LoadLocation();
        }
    }
    private void OnDisable()
    {
        nextStepButton.OnClick.RemoveAllListeners();
        previousStepButton.OnClick.RemoveAllListeners();

        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    void SetupVoiceCommands()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;

        // Setup voice
        var commands = new Dictionary<string, Action>();

        commands.Add("next", () =>
        {
            nextStep();
        });

        commands.Add("next step", () =>
        {
            nextStep();
        });

        commands.Add("previous", () =>
        {
            previousStep();
        });

        commands.Add("previous step", () =>
        {
            previousStep();
        });

        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(commands);
    }

    public void nextStep()
    {
        //if there is a checklist that has not been signed off verify that the operator wants to progress
        if (ProtocolState.Steps[ProtocolState.Step].Checklist != null & !ProtocolState.Steps[ProtocolState.Step].SignedOff & confirmationPanelVC != null)
        {
            //if all items are checked but checklist is not signed off
            if (ProtocolState.CheckItem == ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1 &&
                ProtocolState.Steps[ProtocolState.Step].Checklist[ProtocolState.CheckItem].IsChecked.Value)
            {
                //update confirmation panel UI and button controls
                confirmationPanelVC.SignOffMessage();
                return;
            }

            if(ProtocolState.LockingTriggered.Value)
            {
                audioPlayer?.Play(AudioEventEnum.Error);
                Debug.LogWarning("cannot navigate to next step: locking in progress");
                return;
            }

            //update confirmation panel UI and button controls
            confirmationPanelVC.ChecklistIncompleteMessage();
            return;
        }
        audioPlayer?.Play(AudioEventEnum.NextStep);
        ProtocolState.SetStep(ProtocolState.Step + 1);

/*        //if this is the final step of the procedure and there is no checklist send the CSV
        if ((SessionState.Step + 1) == SessionState.procedureDef.Value.steps.Count & SessionState.procedureDef.Value.steps[SessionState.Step].checklist == null)
        {
            emailEntryPanel.SetActive(true);
        }*/
    }

    public void previousStep()
    {
        if (ProtocolState.LockingTriggered.Value)
        {
            audioPlayer?.Play(AudioEventEnum.Error);
            Debug.LogWarning("cannot navigate to previous step: locking in progress");
            return;
        }
        if (!SessionState.ConfirmationPanelVisible.Value)
        {
            audioPlayer?.Play(AudioEventEnum.PreviousStep);
            ProtocolState.SetStep(ProtocolState.Step - 1);
        }
    }

    // Next step or checklist item
    void progress()
    {
        // TODO handle step and checklist item
        nextStep();
    }

    // Previous step checklist item
    void regress()
    {
        // TODO handle step and checklist item
        previousStep();
    }

    void hideStepButtons()
    {
        previousStepButton.gameObject.SetActive(false);
        nextStepButton.gameObject.SetActive(false);
    }

    void showStepButtons()
    {
        if (ProtocolState.procedureDef.Value == null || SessionState.RunningMode == Mode.Observer) return;

        previousStepButton.gameObject.SetActive(true);
        nextStepButton.gameObject.SetActive(true);
    }

    void disableStepButtons()
    {
        previousStepButton.IsEnabled = false;
        nextStepButton.IsEnabled = false;
    }

    void enableStepButtons()
    {
        previousStepButton.IsEnabled = true;
        nextStepButton.IsEnabled = true;
    }

    void UpdateVisualState()
    {
        if (ProtocolState.procedureDef.Value == null)
        {
            stepText.text = "0/0";
            return;
        }

        int stepCount = (ProtocolState.procedureDef.Value.steps != null) ? ProtocolState.procedureDef.Value.steps.Count : 0;

        stepText.text = string.Format("<size=4>Step:</size> {0}/{1}", Math.Min(stepCount, ProtocolState.Step + 1), stepCount);

        previousStepButton.IsEnabled = ProtocolState.Step > 0;
        nextStepButton.IsEnabled = (ProtocolState.Step + 1) < stepCount;

        if (ProtocolState.procedureDef.Value.steps[ProtocolState.Step].isCritical || (ProtocolState.Step > 0 && ProtocolState.procedureDef.Value.steps[ProtocolState.Step-1].isCritical))
        {
            if (SessionState.Recording.Value)
            {
                ServiceRegistry.GetService<ILighthouseControl>()?.StopRecordingVideo();
            }
            if (ProtocolState.procedureDef.Value.steps[ProtocolState.Step].isCritical & confirmationPanelVC != null)
            {
                confirmationPanelVC.CriticalStepMessage();
            }
        }
    }

    void modeChanged(Mode mode)
    {
        if (mode == Mode.Observer)
        {
            hideStepButtons();
        }
        else
        {
            showStepButtons();
        }
    }
}
