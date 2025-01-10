using UnityEngine;
using UniRx;

/// <summary>
/// Handles objects that use image-based tracking. When successfully tracked, 
/// locks the object and signals the appropriate channels.
/// </summary>
public class ImageTrackingObjectManager : MonoBehaviour
{
    public void HandleImageTrackedObject(GameObject obj)
    {
        var arObjectVC = obj.GetComponent<ArObjectViewController>();
        if (arObjectVC == null)
        {
            Debug.LogWarning($"HandleImageTrackedObject: No ArObjectViewController on {obj.name}.");
            return;
        }

        // Simulate or hook up actual image-tracking logic here:
        // For example, listen to some "OnImageTracked" event, then lock:
        TrackImage(obj)
            .Subscribe(_ =>
            {
                // Once image is confirmed tracked:
                arObjectVC.LockPosition();
                // Update the global state or notify others as needed:
                ProtocolState.Instance.LockingTriggered.Value = true;
                ServiceRegistry.GetService<HeadPlacementEventChannel>()
                    ?.CurrentPrefabLocked?.Invoke();
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