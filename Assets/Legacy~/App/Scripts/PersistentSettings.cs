using UnityEngine;
using UniRx;
using ACAM2.MessagePack;

/// <summary>
/// Storage and retrieval of settings that come from Lighthouse
/// In case the app is started and Lighthouse is not running the settings stored earlier will be used (for instance for aruco detection)
/// </summary>
public class PersistentSettings : MonoBehaviour
{
    private const string ModeDetectorKey = "ModeDetector";
    private const string FpsKey = "Fps";
    private const string ModeTrackingKey = "ModeTracking";
    private const string DictionaryTypeKey = "DictionaryType";
    private const string BoardNumXKey = "BoardNumX";
    private const string BoardNumYKey = "BoardNumY";
    private const string MarkerSizeKey = "MarkerSize";
    private const string BoardSquareSizeKey = "BoardSquareSize";
    private const string ModeArucoKey = "ModeAruco";

    private void OnEnable()
    {
        LoadSettings();

        // Whenever ArucoSettings change in Lighthouse we save them immediately for future use
        SessionState.ArucoSettings.Subscribe(_ => { SaveSettings(); }).AddTo(this);
    }

    public void LoadSettings()
    {
        // create default settings to transition to server based settings
        if (SessionState.ArucoSettings.Value == null)
        {
            SessionState.ArucoSettings.Value = new ArucoSettings() { ModeAruco = ArucoMode.CharucoBoards, BoardNumX = 5, BoardNumY = 5, BoardSquareSize = 40, ModeDetector = DetectorMode.FreeRunning };
        }

        if (PlayerPrefs.HasKey(ModeArucoKey))
        {
            SessionState.ArucoSettings.Value.ModeDetector = (DetectorMode)PlayerPrefs.GetInt(ModeDetectorKey);
            SessionState.ArucoSettings.Value.Fps = PlayerPrefs.GetFloat(FpsKey);
            SessionState.ArucoSettings.Value.ModeTracking = (TrackingMode)PlayerPrefs.GetInt(ModeTrackingKey);
            SessionState.ArucoSettings.Value.DictionaryType = (DictionaryType)PlayerPrefs.GetInt(DictionaryTypeKey);
            SessionState.ArucoSettings.Value.BoardNumX = (byte)PlayerPrefs.GetInt(BoardNumXKey);
            SessionState.ArucoSettings.Value.BoardNumY = (byte)PlayerPrefs.GetInt(BoardNumYKey);
            SessionState.ArucoSettings.Value.MarkerSize = PlayerPrefs.GetFloat(MarkerSizeKey);
            SessionState.ArucoSettings.Value.BoardSquareSize = PlayerPrefs.GetFloat(BoardSquareSizeKey);
            SessionState.ArucoSettings.Value.ModeAruco = (ArucoMode)PlayerPrefs.GetInt(ModeArucoKey);
        }
    }

    public void SaveSettings()
    {
        if (SessionState.ArucoSettings.Value != null)
        {
            Debug.Log("Saving Aruco Settings");

            PlayerPrefs.SetInt(ModeDetectorKey, (int)SessionState.ArucoSettings.Value.ModeDetector);
            PlayerPrefs.SetFloat(FpsKey, SessionState.ArucoSettings.Value.Fps);
            PlayerPrefs.SetInt(ModeTrackingKey, (int)SessionState.ArucoSettings.Value.ModeTracking);
            PlayerPrefs.SetInt(DictionaryTypeKey, (int)SessionState.ArucoSettings.Value.DictionaryType);
            PlayerPrefs.SetInt(BoardNumXKey, (int)SessionState.ArucoSettings.Value.BoardNumX);
            PlayerPrefs.SetInt(BoardNumYKey, (int)SessionState.ArucoSettings.Value.BoardNumY);
            PlayerPrefs.SetFloat(MarkerSizeKey, SessionState.ArucoSettings.Value.MarkerSize);
            PlayerPrefs.SetFloat(BoardSquareSizeKey, SessionState.ArucoSettings.Value.BoardSquareSize);
            PlayerPrefs.SetInt(ModeArucoKey, (int)SessionState.ArucoSettings.Value.ModeAruco);
            PlayerPrefs.Save();
        }
    }
}

