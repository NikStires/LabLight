using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CalibrationManager", menuName = "ScriptableObjects/CalibrationManager", order = 1)]
public class CalibrationManagerScriptableObject : ScriptableObject
{
    
    [SerializeField]
    public UnityEvent OnCalibrationRequested = new UnityEvent();
    [SerializeField]
    public UnityEvent<string> OnCalibrationStatusUpdate = new UnityEvent<string>();
    [SerializeField]
    public UnityEvent<bool> OnCalibrationStarted = new UnityEvent<bool>();
    // Start is called before the first frame update
    private void Awake()
    {
        if(OnCalibrationRequested == null)
        {
            OnCalibrationRequested = new UnityEvent();
        }
        if(OnCalibrationStatusUpdate == null)
        {
            OnCalibrationStatusUpdate = new UnityEvent<string>();
        }
        if(OnCalibrationStarted == null)
        {
            OnCalibrationStarted = new UnityEvent<bool>();
        }
    }

    // Update is called once per frame
    public void OnRequestCalibration()
    {
        OnCalibrationRequested.Invoke();
    }

    public void UpdateCalibrationStatus(string status)
    {
        OnCalibrationStatusUpdate.Invoke(status);
    }

    public void CalibrationStarted(bool started)
    {
        OnCalibrationStarted.Invoke(started);
    }
}
