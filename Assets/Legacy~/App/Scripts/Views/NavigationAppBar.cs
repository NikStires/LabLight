using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class NavigationAppBar : MonoBehaviour
{
    public Interactable calibrateButton;
    public Interactable operatorButton;
    public Interactable settingsButton;
    public Interactable wellPlateSettingsButton;
    private Action disposeVoice;

    private void OnEnable()
    {
        // Calibration button
        calibrateButton.OnClick.AsObservable().Subscribe(_ =>
        {
            SessionManager.Instance.GotoScreen(ScreenType.Calibration);
        }).AddTo(this);

        operatorButton.OnClick.AsObservable().Subscribe(_ =>
        {
            ServiceRegistry.GetService<ISharedStateController>().SetMaster(SessionState.deviceId);
        }).AddTo(this);

        settingsButton.OnClick.AsObservable().Subscribe(_ =>
        {
            SessionManager.Instance.GotoScreen(ScreenType.Settings);
        }).AddTo(this);

        wellPlateSettingsButton.OnClick.AsObservable().Subscribe(_ =>
        {
            SessionManager.Instance.GotoScreen(ScreenType.WellPlateSettings);
        }).AddTo(this);

        SetupVoiceCommands();
    }

    private void OnDisable()
    {
        disposeVoice?.Invoke();
        disposeVoice = null; 
    }

    void SetupVoiceCommands()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;

        // Setup voice
        var commands = new Dictionary<string, Action>();

        // commands.Add("operator", () =>
        // {
        //     ServiceRegistry.GetService<ISharedStateController>().SetMaster(SessionState.deviceId);
        // });

        commands.Add("calibrate", () =>
        {
            if (SessionManager.Instance.ActiveScreen.ScreenData.Type != ScreenType.Calibration)
            {
                SessionManager.Instance.GotoScreen(ScreenType.Calibration);
            }
        });

        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(commands);
    }
}