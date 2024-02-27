using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class CalibrationStatusBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject _offIndicator = null;
    
    [SerializeField]
    private GameObject _onIndicator = null;

    [SerializeField]
    private TextMeshProUGUI text;

    [SerializeField] 
    private CalibrationManagerScriptableObject calibrationManager;
    // Start is called before the first frame update
    private void Awake()
    {
        if (_offIndicator == null || _onIndicator == null)
        {
            throw new MissingReferenceException("OffIndicator and OnIndicator fields in CalibrationStatusBehavior class cannot be null.");
        }
    }

    private void OnEnable()
    {
        calibrationManager.OnCalibrationStatusUpdate.AddListener(UpdateTextDisplay);

        calibrationManager.OnCalibrationStarted.AddListener(UpdateIndicator);
    }

    private void OnDisable()
    {
        calibrationManager.OnCalibrationStatusUpdate.RemoveListener(UpdateTextDisplay);
        calibrationManager.OnCalibrationStarted.RemoveListener(UpdateIndicator);
    }

    private void UpdateTextDisplay(string status)
    {
        text.text = status;
    }

    private void UpdateIndicator(bool inCalibration)
    {
        _offIndicator.SetActive(inCalibration);
        _onIndicator.SetActive(!inCalibration);
    }
}
