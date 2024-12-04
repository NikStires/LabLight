using System.Collections.Generic;
using UniRx;
using UnityEngine;

/// <summary>
/// Base class for all AR visualizations
/// 
/// An ArView is responsible for translating the ArObject and optional TrackedObjects to a runtime visualization
/// </summary>
public class ArObjectViewController : MonoBehaviour
{
    public ArObject arObject;

    public virtual void Initialize(ArObject arObject, List<TrackedObject> trackedObjects)
    {
        this.arObject = arObject;

        foreach (var trackedObject in trackedObjects)
        {
            TrackedObjects.Add(trackedObject);
        }
    }

    public ReactiveCollection<TrackedObject> TrackedObjects = new ReactiveCollection<TrackedObject>();
}
