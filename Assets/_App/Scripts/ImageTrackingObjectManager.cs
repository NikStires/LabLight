using UnityEngine;
using UniRx;
using System;

/// <summary>
/// Handles objects that use image-based tracking. When successfully tracked, 
/// locks the object and signals the appropriate channels.
/// </summary>
public class ImageTrackingObjectManager : MonoBehaviour
{
    public ImageTrackingEventChannel imageTrackingEventChannel;

    private void OnEnable()
    {
        imageTrackingEventChannel.SetImageTrackedObject.AddListener(HandleImageTrackedObject);
    }

    private void OnDisable()
    {
        imageTrackingEventChannel.SetImageTrackedObject.RemoveListener(HandleImageTrackedObject);
    }

    public void HandleImageTrackedObject(GameObject obj)
    {
        var arObjectVC = obj.GetComponent<ArObjectViewController>();
        if (arObjectVC == null)
        {
            Debug.LogWarning($"HandleImageTrackedObject: No ArObjectViewController on {obj.name}.");
            return;
        }

        TrackImage(obj)
            .Subscribe(_ =>
            {
                arObjectVC.LockPosition();
                ProtocolState.Instance.LockingTriggered.Value = true;
                imageTrackingEventChannel.OnCurrentImageTracked();
                Debug.Log($"{obj.name} successfully image-tracked and locked.");
            });
    }

    // Example stub for an observable that finishes when image is recognized
    private IObservable<bool> TrackImage(GameObject obj)
    {
        // Replace this with actual image tracking code or service
        return Observable.Return(true).Delay(System.TimeSpan.FromSeconds(1));
    }
} 