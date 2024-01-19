using UnityEngine;
using UniRx;
using UnityEditor;

/// <summary>
/// Load from and save settings to playerprefs
/// </summary>
public class SettingsManager : MonoBehaviour
{
    //private const string ShowGridSetting = "ShowGridSetting";
    private const string ShowWorkspaceOrigin = "ShowWorkspaceOrigin";

    //well plate settings
    private const string ShowRowColIndicatorsSetting = "ShowRolColIndicatorsSetting";
    private const string ShowRowColIndicatorHighlightSetting = "ShowRowColIndicatorHighlightSetting";
    private const string ShowRowColHighlightsSetting = "ShowRowColHighlightSetting";
    private const string ShowInformationPanelSetting = "ShowInformationPanelSetting";
    private const string ShowMarkerSetting = "ShowMarkerSetting";
    private const string ShowSourceContentsSetting = "ShowSourceContentsSetting";
    private const string ShowSourceTransformSetting = "ShowSourceContentsSetting";

    void Start()
    {
        // Initialize from player preferences
        //SessionState.ShowGrid.Value = PlayerPrefs.GetInt(ShowGridSetting) == 1;
        SessionState.ShowWorkspaceOrigin.Value = PlayerPrefs.GetInt(ShowWorkspaceOrigin) == 1;

        //initialize well plate from player preferences
        //SessionState.ShowRowColIndicators.Value = PlayerPrefs.GetInt(ShowRowColIndicatorsSetting) == 1;
        //SessionState.ShowRowColIndicatorHighlight.Value = PlayerPrefs.GetInt(ShowRowColIndicatorHighlightSetting) == 1;
        //SessionState.ShowRowColHighlights.Value = PlayerPrefs.GetInt(ShowRowColHighlightsSetting) == 1;
        //SessionState.ShowInformationPanel.Value = PlayerPrefs.GetInt(ShowInformationPanelSetting) == 1;
        //SessionState.ShowMarker.Value = PlayerPrefs.GetInt(ShowBBSetting) == 1;
        //SessionState.ShowSourceContents.Value = PlayerPrefs.GetInt(ShowTubeContentsSetting) == 1;
        //SessionState.ShowSourceTransform.Value = PlayerPrefs.GetInt(ShowTubeCapsSetting) == 1;

        // Setup change handler to save to preferences

        //SessionState.ShowGrid.Subscribe(value =>
        //{
        //    PlayerPrefs.SetInt(ShowGridSetting, value ? 1 : 0);
        //    PlayerPrefs.Save();
        //}).AddTo(this);

        SessionState.ShowWorkspaceOrigin.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowWorkspaceOrigin, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        //well plate settings


        SessionState.ShowRowColIndicators.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowRowColIndicatorsSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowRowColIndicatorHighlight.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowRowColIndicatorHighlightSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowRowColHighlights.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowRowColHighlightsSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowInformationPanel.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowInformationPanelSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowMarker.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowMarkerSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowSourceContents.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowSourceContentsSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);

        SessionState.ShowSourceTransform.Subscribe(value =>
        {
            PlayerPrefs.SetInt(ShowSourceTransformSetting, value ? 1 : 0);
            PlayerPrefs.Save();
        }).AddTo(this);
    }
}
