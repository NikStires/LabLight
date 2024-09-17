using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class WorldspaceEditorViewController : LLBasePanel
{
    public ObjectPlacerController objectPlacerController;
    public Transform achoredObjectParent;

    private void Start()
    {
        achoredObjectParent = FindObjectOfType<XROrigin>().transform;
    }

    public void OnEnable()
    {
        SessionState.AnchoredObjectEditMode.Value = true;
    }

    public void OnDisable()
    {
        SessionState.AnchoredObjectEditMode.Value = false;
    }

    public void PlaceHazardZone()
    {

    }

    public void PlaceSpatialNote()
    {
        // Temporary placement object/reticule that wil be replaced by anchored object after placement
        var placementObject = Instantiate(objectPlacerController, achoredObjectParent);

        // Payload is injected after placement
        placementObject.StartPlacementDelayed((anchoredObject) =>
            {
                Debug.Log("Initializing anchoredObject");
                anchoredObject.Initialize(new SpatialNotePayload(), true);
            });
    }

    public void RemoveAllAnchoredObjects()
    {
        var anchoredObjects = FindObjectsByType<AnchoredObjectController>(FindObjectsSortMode.None);
        foreach (var anchoredObject in anchoredObjects)
        {
            anchoredObject.RemoveAnchoredObject();
        }
    }

    public void CloseSpatialNotesMenu()
    {
        SceneLoader.Instance.LoadSceneClean("ProtocolMenu");
    }
}
