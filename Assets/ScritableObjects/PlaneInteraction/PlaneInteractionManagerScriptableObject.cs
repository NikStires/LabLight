using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

[CreateAssetMenu(fileName = "PlaneInteractionManager", menuName = "ScriptableObjects/PlaneInteractionManager", order = 2)]
public class PlaneInteractionManagerScriptableObject : ScriptableObject
{
    
    [SerializeField]
    public UnityEvent<Vector3> FingerTipPlaneCollision = new UnityEvent<Vector3>();
    [SerializeField]
    public UnityEvent EnableTapToPlace = new UnityEvent();
    [SerializeField]
    public UnityEvent DisableTapToPlace = new UnityEvent();
    [SerializeField]
    public UnityEvent EnableHeadPlacement = new UnityEvent();
    [SerializeField]
    public UnityEvent DisableHeadPlacement = new UnityEvent();
    [SerializeField]
    public UnityEvent<List<GameObject>> RequestObjectPlacement = new UnityEvent<List<GameObject>>();

    [SerializeField]
    public UnityEvent<ARPlane> PlanePlacementRequested = new UnityEvent<ARPlane>();
    // Start is called before the first frame update
    private void Awake()
    {
        if(FingerTipPlaneCollision == null)
        {
            FingerTipPlaneCollision = new UnityEvent<Vector3>();
        }
        if(EnableTapToPlace == null)
        {
            EnableTapToPlace = new UnityEvent();
        }
        if(DisableTapToPlace == null)
        {
            DisableTapToPlace = new UnityEvent();
        }
        if(EnableHeadPlacement == null)
        {
            EnableHeadPlacement = new UnityEvent();
        }
        if(DisableHeadPlacement == null)
        {
            DisableHeadPlacement = new UnityEvent();
        }
        if(RequestObjectPlacement == null)
        {
            RequestObjectPlacement = new UnityEvent<List<GameObject>>();
        }
        if(PlanePlacementRequested == null)
        {
            PlanePlacementRequested = new UnityEvent<ARPlane>();
        }
    }

    // Update is called once per frame
    public void OnFingerTipPlaneCollision(Vector3 position)
    {
        FingerTipPlaneCollision.Invoke(position);
    }

    public void OnEnableTapToPlace()
    {
        EnableTapToPlace.Invoke();
    }

    public void OnDisableTapToPlace()
    {
        DisableTapToPlace.Invoke();
    }

    public void OnEnableHeadPlacement()
    {
        EnableHeadPlacement.Invoke();
    }

    public void OnDisableHeadPlacement()
    {
        DisableHeadPlacement.Invoke();
    }

    public void OnRequestObjectPlacement(List<GameObject> objects)
    {
        RequestObjectPlacement.Invoke(objects);
    }
    
    public void OnPlanePlacementRequested(ARPlane plane)
    {
        PlanePlacementRequested.Invoke(plane);
    }
}
