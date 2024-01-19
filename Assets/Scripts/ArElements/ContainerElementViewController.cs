using System;
using System.Collections.Generic;
using UnityEngine;


public class ContainerElementViewController : WorldPositionController
{
    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        Debug.Log("Creating new container for " + arDefinition.arDefinitionType + " with " + trackedObjects.Count + " tracked objects");
        base.Initialize(arDefinition, trackedObjects);
    }
}
