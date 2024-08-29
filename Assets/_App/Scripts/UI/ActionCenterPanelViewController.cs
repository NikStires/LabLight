using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
 

/// <summary>
/// Controls the behavior of the action center panel in the scene.
/// </summary>
public class ActionCenterPanelViewController : MonoBehaviour
{
    [SerializeField] private HeadPlacementEventChannel headPlacementEventChannel;
    [SerializeField] GameObject timerPrefab;
    [SerializeField] GameObject hazardZonePanelPrefab;
    [SerializeField] UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable recordingButton;
    bool isRecording = false;
    [SerializeField] UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable replayButton;
    bool isReplaying = false;
    [SerializeField] UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable internetBrowserButton;
    bool isBrowserOpen = false;

    void Start()
    {
        recordingButton.selectExited.AddListener(_ => ToggleLighthouseRecording());
        replayButton.selectExited.AddListener(_ => ToggleLighthouseReplay());
        internetBrowserButton.selectExited.AddListener(_ => OpenInternetBrowser());
    }

    /// <summary>
    /// Spawns a timer object at the same position and rotation as the action center panel, and deactivates the panel.
    /// </summary>
    public void SpawnTimer()
    {
        var timer = Instantiate(timerPrefab);
        timer.transform.position = transform.position;
        timer.transform.rotation = transform.rotation;
    }

    public void SpawnHazardZonePanel()
    {
        var hazardZonePanel = Instantiate(hazardZonePanelPrefab);
        hazardZonePanel.transform.position = transform.position;
        hazardZonePanel.transform.rotation = transform.rotation;
    }

    /// <summary>
    /// Starts the calibration scene additively and deactivates the action center panel.
    /// </summary>
    public void StartCalibration()
    {
        SceneLoader.Instance.LoadSceneAdditive("Calibration");
    }

    public void OpenSettings()
    {
        SceneLoader.Instance.LoadSceneAdditive("Settings");
    }

    public void OpenSpatialNotesEditor()
    {
        this.gameObject.SetActive(false);
        SceneLoader.Instance.LoadSceneAdditive("SpatialNotesEditor");
    }    

    /// <summary>
    /// Toggles the visibility of generic visualizations for AR objects.
    /// </summary>
    public void ToggleTestARObjects()
    {
        SessionState.enableGenericVisualizations.Value = !SessionState.enableGenericVisualizations.Value;
    }

    void ToggleLighthouseRecording()
    {
        if (isRecording)
        {
            ServiceRegistry.GetService<ILighthouseControl>().StopRecordingVideo();
        }
        else
        {
            ServiceRegistry.GetService<ILighthouseControl>().StartRecordingVideo();
        }
        isRecording = !isRecording;
    }

    void ToggleLighthouseReplay()
    {
        if (isReplaying)
        {
            ServiceRegistry.GetService<ILighthouseControl>().StopPlayingVideo();
        }
        else
        {
            ServiceRegistry.GetService<ILighthouseControl>().StartPlayingVideo();
        }
        isReplaying = !isReplaying;
    }

    void OpenInternetBrowser()
    {
        ServiceRegistry.GetService<IWebPageProvider>().OpenWebPage("");
    }
}
