using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckmateChessMoveController : WorldPositionController
{
    public GameObject TargetPrefab;
    public Transform TargetContainer;

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);
        /*
        if (trackedObjects != null && trackedObjects.Count > 0 && trackedObjects[0] is CheckmateChessMove)
        {
            CheckmateChessMove checkmateChessMoveObject = (CheckmateChessMove)TrackedObjects[0];
            if (checkmateChessMoveObject != null)
            {
                //GameObject.Instantiate(TargetPrefab, checkmateChessMoveObject.position, Quaternion.identity, TargetContainer);
            }
        }*/
    }
}
