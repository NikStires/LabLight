using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActionCenterPanelViewController : MonoBehaviour
{
    [SerializeField] GameObject timerPrefab;

    public void SpawnTimer()
    {
        var timer = Instantiate(timerPrefab);
        timer.transform.position = transform.position;
        timer.transform.rotation = transform.rotation;
        this.gameObject.SetActive(false);
    }

    public void StartCalibration()
    {
        SceneLoader.Instance.LoadNewScene("Calibration");
        this.gameObject.SetActive(false);
    }

    public void ToggleTestARObjects()
    {
        SessionState.enableGenericVisualizations.Value = !SessionState.enableGenericVisualizations.Value;
    }
}
