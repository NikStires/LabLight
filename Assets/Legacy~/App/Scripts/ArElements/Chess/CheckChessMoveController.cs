using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckChessMoveController : WorldPositionController
{
    public GameObject TargetPrefab;
    public Transform TargetContainer;

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);
        if (trackedObjects != null && trackedObjects.Count > 0 && trackedObjects[0] is CheckChessMove)
        {
            CheckChessMove checkChessMoveObject = (CheckChessMove)TrackedObjects[0];
            if (checkChessMoveObject != null)
            {
                GameObject.Instantiate(TargetPrefab, checkChessMoveObject.position, Quaternion.identity, TargetContainer);
            }
        }
    }
}
