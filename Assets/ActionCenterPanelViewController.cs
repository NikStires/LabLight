using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the behavior of the action center panel in the scene.
/// </summary>
public class ActionCenterPanelViewController : MonoBehaviour
{
    [SerializeField] GameObject timerPrefab;

    /// <summary>
    /// Spawns a timer object at the same position and rotation as the action center panel, and deactivates the panel.
    /// </summary>
    public void SpawnTimer()
    {
        var timer = Instantiate(timerPrefab);
        timer.transform.position = transform.position;
        timer.transform.rotation = transform.rotation;
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Starts the calibration scene additively and deactivates the action center panel.
    /// </summary>
    public void StartCalibration()
    {
        SceneLoader.Instance.LoadSceneAdditive("Calibration");
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Toggles the visibility of generic visualizations for AR objects.
    /// </summary>
    public void ToggleTestARObjects()
    {
        SessionState.enableGenericVisualizations.Value = !SessionState.enableGenericVisualizations.Value;
    }
}
