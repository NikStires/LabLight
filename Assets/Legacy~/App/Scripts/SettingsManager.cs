using UnityEngine;
using UniRx;
using UnityEditor;
using Microsoft.MixedReality.Toolkit;

/// <summary>
/// Load from and save settings to playerprefs
/// </summary>
public class SettingsManager : MonoBehaviour
{
    private const string ShowIntroSetting = "ShowIntroSetting";
    private const string EnableHandMenuSetting = "EnableHandMenuSetting";
    private const string MultiplePinLockingSetting = "MultiplePinLockingSetting";
    private const string LockStepControlsSetting = "LockStepControlsSetting";
    private const string ShowGridSetting = "ShowGridSetting";
    private const string ShowHandRaysSetting = "ShowHandRaysSetting";
    private const string ShowCalibrationAxesSetting = "ShowCalibrationAxesSetting";

    //well plate settings
    private const string ShowRowColIndicatorsSetting = "ShowRolColIndicatorsSetting";
    private const string ShowRowColIndicatorHighlightSetting = "ShowRowColIndicatorHighlightSetting";
    private const string ShowRowColHighlightsSetting = "ShowRowColHighlightSetting";
    private const string ShowInformationPanelSetting = "ShowInformationPanelSetting";
    private const string ShowBBSetting = "ShowBBSetting";
    private const string ShowTubeContentsSetting = "ShowTubeContentsSetting";
    private const string ShowTubeCapsSetting = "ShowTubeCapsSetting";

    [SerializeField]
    private MixedRealityToolkitConfigurationProfile HandRaysConfiguriationProfile;
    [SerializeField]
    private MixedRealityToolkitConfigurationProfile NoHandRaysConfiguriationProfile;

    void Start()
    {
        // Initialize from player preferences
        SessionState.ShowIntro.Value = PlayerPrefs.GetInt(ShowIntroSetting) == 1;
        SessionState.EnableHandMenu.Value = PlayerPrefs.GetInt(EnableHandMenuSetting) == 1;
        SessionState.MultiplePinLocking.Value = PlayerPrefs.GetInt(MultiplePinLockingSetting) == 1;
        SessionState.LockStepControls.Value = PlayerPrefs.GetInt(LockStepControlsSetting) == 1;
        SessionState.ShowGrid.Value = PlayerPrefs.GetInt(ShowGridSetting) == 1;
        SessionState.ShowHandRays.Value = PlayerPrefs.GetInt(ShowHandRaysSetting) == 1;
        SessionState.ShowCalibrationAxes.Value = PlayerPrefs.GetInt(ShowCalibrationAxesSetting) == 1;

        //initialize well plate from player preferences
        SessionState.ShowRowColIndicators.Value = PlayerPrefs.GetInt(ShowRowColIndicatorsSetting) == 1;
        SessionState.ShowRowColIndicatorHighlight.Value = PlayerPrefs.GetInt(ShowRowColIndicatorHighlightSetting) == 1;
        SessionState.ShowRowColHighlights.Value = PlayerPrefs.GetInt(ShowRowColHighlightsSetting) == 1;
        SessionState.ShowInformationPanel.Value = PlayerPrefs.GetInt(ShowInformationPanelSetting) == 1;
        SessionState.ShowBB.Value = PlayerPrefs.GetInt(ShowBBSetting) == 1;
        SessionState.ShowTubeContents.Value = PlayerPrefs.GetInt(ShowTubeContentsSetting) == 1;
        SessionState.ShowTubeCaps.Value = PlayerPrefs.GetInt(ShowTubeCapsSetting) == 1;

        // Setup change handler to save to preferences
        SessionState.ShowIntro.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowIntroSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.EnableHandMenu.Subscribe(value =>
        {
            PlayerPrefs.SetInt(EnableHandMenuSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.MultiplePinLocking.Subscribe(value =>
        {
            PlayerPrefs.SetInt(MultiplePinLockingSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.LockStepControls.Subscribe(value =>
        {
            PlayerPrefs.SetInt(LockStepControlsSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowGrid.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowGridSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowHandRays.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowHandRaysSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowHandRays.Subscribe(value =>
        {
            // RS Dynamically switching profiles causes all kinds of unwanted side effects. So button & implementation are disabled.

            if (value)
            {
                if (MixedRealityToolkit.Instance.ActiveProfile != HandRaysConfiguriationProfile)
                {
                    MixedRealityToolkit.Instance.ActiveProfile = HandRaysConfiguriationProfile;
                }
            }
            else
            {
                if (MixedRealityToolkit.Instance.ActiveProfile != NoHandRaysConfiguriationProfile)
                {
                    MixedRealityToolkit.Instance.ActiveProfile = NoHandRaysConfiguriationProfile;
                }
            }
        }).AddTo(this);

        SessionState.ShowCalibrationAxes.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowCalibrationAxesSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        //well plate settings


        SessionState.ShowRowColIndicators.Subscribe(value => 
        {
            PlayerPrefs.SetInt(ShowRowColIndicatorsSetting,value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);
        
        SessionState.ShowRowColIndicatorHighlight.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowRowColIndicatorHighlightSetting,value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowRowColHighlights.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowRowColHighlightsSetting,value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowInformationPanel.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowInformationPanelSetting,value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowBB.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowBBSetting,value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowTubeContents.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowTubeContentsSetting,value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowTubeCaps.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowTubeCapsSetting,value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);
    }
}
