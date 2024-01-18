using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Behaviour that switches the specified object on if one of the fingertips is close enough
/// </summary>
public class LegalChessMoveController : VisibilityController
{
    public GameObject TargetPrefab;
    public Transform TargetContainer;

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);

        if (TrackedObjects != null && TrackedObjects.Count > 0 && TrackedObjects[0] is LegalChessMove)
        {
            LegalChessMove lcm = (LegalChessMove)TrackedObjects[0];

            foreach (var pos in lcm.targetPositions)
            {
                GameObject.Instantiate(TargetPrefab, pos, Quaternion.identity, TargetContainer);
            }
        }
    }
}
