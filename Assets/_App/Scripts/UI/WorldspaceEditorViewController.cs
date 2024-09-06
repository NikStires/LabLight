using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class WorldspaceEditorViewController : LLBasePanel
{
    public AnchoredObjectController anchoredObjectPrefab;
    public Transform achoredObjectParent;

    private void Start()
    {
        achoredObjectParent = FindObjectOfType<XROrigin>().transform;
    }

    public void OnEnable()
    {
        SessionState.SpatialNoteEditMode.Value = true;
    }

    public void OnDisable()
    {
        SessionState.SpatialNoteEditMode.Value = false;
    }

    public void PlaceHazardZone()
    {

    }

    public void PlaceSpatialNote()
    {
        var anchoredObject = Instantiate(anchoredObjectPrefab, achoredObjectParent);
        anchoredObject.Initialize(new SpatialNotePayload(), true);
        anchoredObject.StartPlacementDelayed();
    }

    public void CloseSpatialNotesMenu()
    {
        SceneLoader.Instance.LoadSceneClean("ProtocolMenu");
    }
}
