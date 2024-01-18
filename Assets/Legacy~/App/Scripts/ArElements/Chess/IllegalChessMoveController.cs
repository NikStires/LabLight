using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The source location is already visualized, the target needs to be generated
/// </summary>
public class IllegalChessMoveController : WorldPositionController
{
    public GameObject TargetPrefab;
    public Transform TargetContainer;

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);
        
        if(trackedObjects != null && trackedObjects.Count > 0 && trackedObjects[0] is IllegalChessMove)
        {
            IllegalChessMove illegalChessMoveObject = (IllegalChessMove)TrackedObjects[0];
            if(illegalChessMoveObject != null)
            {
                GameObject.Instantiate(TargetPrefab, illegalChessMoveObject.targetPosition, Quaternion.identity, TargetContainer);
            }
        }
    }
}
