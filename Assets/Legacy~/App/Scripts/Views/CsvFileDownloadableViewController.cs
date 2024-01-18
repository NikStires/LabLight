using UniRx;
using UnityEngine;

public class CsvFileDownloadableViewController : MonoBehaviour
{
    public GameObject csvFileDownloadableObject;

    private void OnEnable()
    {
        SessionState.CsvFileDownloadable.Subscribe(val =>
        {
            csvFileDownloadableObject.SetActive(!string.IsNullOrEmpty(val));
        }).AddTo(this);
    }
}