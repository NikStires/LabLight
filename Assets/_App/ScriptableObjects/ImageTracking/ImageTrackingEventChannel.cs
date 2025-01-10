using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ImageTrackingEventChannel", menuName = "ScriptableObjects/ImageTrackingEventChannel", order = 2)]
public class ImageTrackingEventChannel : ScriptableObject
{
    [SerializeField]
    public UnityEvent<GameObject> SetImageTrackedObject = new UnityEvent<GameObject>();

    [SerializeField]
    public UnityEvent CurrentImageTracked = new UnityEvent();

    [SerializeField]
    public UnityEvent RequestDisableImageTrackingManager = new UnityEvent();

    private void Awake()
    {
        if(SetImageTrackedObject == null)
        {
            SetImageTrackedObject = new UnityEvent<GameObject>();
        }
        if(CurrentImageTracked == null)
        {
            CurrentImageTracked = new UnityEvent();
        }
        if(RequestDisableImageTrackingManager == null)
        {
            RequestDisableImageTrackingManager = new UnityEvent();
        }
    }

    public void OnSetImageTrackedObject(GameObject obj)
    {
        SetImageTrackedObject.Invoke(obj);
    }

    public void OnCurrentImageTracked()
    {
        CurrentImageTracked.Invoke();
    }

    public void OnRequestDisableImageTrackingManager()
    {
        RequestDisableImageTrackingManager.Invoke();
    }
} 