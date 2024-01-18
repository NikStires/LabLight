using UniRx;
using UnityEngine;

public class DirtyCalibrationViewController : MonoBehaviour
{
    public GameObject dirtyCalibrationObject;


    private void OnEnable()
    {
        SessionState.CalibrationDirty.Subscribe(val =>
        {
            dirtyCalibrationObject.SetActive(val);
        }).AddTo(this);
    }
}