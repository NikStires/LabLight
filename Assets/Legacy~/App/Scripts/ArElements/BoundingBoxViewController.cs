using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Updates the lineRenderer in this prefab
/// </summary>
public class BoundingBoxViewController : ArElementViewController
{
    private DynamicCubeMesh dynamicCubeMesh;
    private Vector3[] corners = new Vector3[8];

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);
        dynamicCubeMesh = GetComponent<DynamicCubeMesh>();
    }

    public virtual void Update()
    {
        // TODO Make reactive so it only runs when something changed in trackedobject
        if (TrackedObjects != null && TrackedObjects.Count > 0 && TrackedObjects[0].mask != null)
        {
            TrackedObjects[0].bounds.CopyTo(corners, 0);
            dynamicCubeMesh.UpdateCorners(corners);
        }
    }
}
