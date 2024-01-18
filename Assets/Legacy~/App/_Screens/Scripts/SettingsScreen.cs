using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UniRx;

/// <summary>
/// Controller for displaying and changing the current settings
/// </summary>
public class SettingsScreen : ScreenViewController
{
    [SerializeField]
    private Interactable showIntroInteractable;

    [SerializeField]
    private Interactable showHandMenuInteractable;

    [SerializeField]
    private Interactable multiplePinLockingInteractable;

    [SerializeField]
    private Interactable lockStepControlsInteractable;

    [SerializeField]
    private Interactable showGridInteractable;

    [SerializeField]
    private Interactable showHandRaysInteractable;

    [SerializeField]
    private Interactable showCalibrationAxesInteractable;

    [SerializeField]
    private Interactable textToSpeechInteractable;

    private void Start()
    {
        // Update UI by subscribing to changes
        SessionState.ShowIntro.Subscribe(value =>
        {
            showIntroInteractable.IsToggled = value;
        }).AddTo(this);

        SessionState.EnableHandMenu.Subscribe(value =>
        {
            showHandMenuInteractable.IsToggled = value;
        }).AddTo(this);

        SessionState.MultiplePinLocking.Subscribe(value =>
        {
            multiplePinLockingInteractable.IsToggled = value;
        }).AddTo(this);

        SessionState.LockStepControls.Subscribe(value =>
        {
            lockStepControlsInteractable.IsToggled = value;
        }).AddTo(this);

        SessionState.ShowGrid.Subscribe(value =>
        {
            showGridInteractable.IsToggled = value;
        }).AddTo(this);

        SessionState.ShowHandRays.Subscribe(value =>
        {
            showHandRaysInteractable.IsToggled = value;
        }).AddTo(this);

        SessionState.ShowCalibrationAxes.Subscribe(value =>
        {
            showCalibrationAxesInteractable.IsToggled = value;
        }).AddTo(this);

        SessionState.TextToSpeech.Subscribe(value =>
        {
            textToSpeechInteractable.IsToggled = value;
        }).AddTo(this);

        // Update value by setting click handlers
        showIntroInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowIntro.Value = showIntroInteractable.IsToggled; });

        showHandMenuInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.EnableHandMenu.Value = showHandMenuInteractable.IsToggled; });

        multiplePinLockingInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.MultiplePinLocking.Value = multiplePinLockingInteractable.IsToggled; });

        lockStepControlsInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.LockStepControls.Value = lockStepControlsInteractable.IsToggled; });

        showGridInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowGrid.Value = showGridInteractable.IsToggled; });

        showHandRaysInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowHandRays.Value = showHandRaysInteractable.IsToggled; });

        showCalibrationAxesInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.ShowCalibrationAxes.Value = showCalibrationAxesInteractable.IsToggled; });

        textToSpeechInteractable.OnClick.AsObservable().
            Subscribe(_ => { SessionState.TextToSpeech.Value = textToSpeechInteractable.IsToggled; });
    }
    public void GoBack()
    {
        SessionManager.Instance.GoBack();
    }
}