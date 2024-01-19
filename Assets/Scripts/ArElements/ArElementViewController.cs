using System.Collections.Generic;
using UniRx;
using UnityEngine;

/// <summary>
/// Base class for all AR visualizations
/// 
/// An ArView is responsible for translating the ArDefinition and optional TrackedObjects to a runtime visualization
/// </summary>
public class ArElementViewController : MonoBehaviour
{
    protected ArDefinition arDefinition;

    public virtual void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        this.arDefinition = arDefinition;

        foreach (var trackedObject in trackedObjects)
        {
            TrackedObjects.Add(trackedObject);
        }
    }

    public ReactiveCollection<TrackedObject> TrackedObjects = new ReactiveCollection<TrackedObject>();
}
