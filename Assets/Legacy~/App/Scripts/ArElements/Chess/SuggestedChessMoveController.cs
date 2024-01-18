using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Source location is the trackedobject it is attached to, only targetlocation needs to be added
/// </summary>
public class SuggestedChessMoveController : WorldPositionController
{
    public GameObject TargetPrefab;
    public Transform TargetContainer;

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);
        
        if (TrackedObjects != null && TrackedObjects.Count > 0 && TrackedObjects[0] is SuggestedChessMove)
        {
            SuggestedChessMove suggestedMove = (SuggestedChessMove)TrackedObjects[0];
            if(suggestedMove != null)
            {
                GameObject.Instantiate(TargetPrefab, suggestedMove.targetPosition, Quaternion.identity, TargetContainer);
            }
        }
    }
}
