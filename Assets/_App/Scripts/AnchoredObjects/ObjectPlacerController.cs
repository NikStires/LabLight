using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// AnchoredObjectController is a script that initializes the payload of this GameObject
/// Using the trackableId of the anchor a lookup is performed to find the corresponding LabLight data that is attached to this anchor
/// </summary>
public class ObjectPlacerController : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] HeadPlacementEventChannel headPlacementEventChannel;

    [SerializeField]
    private AnchoredObjectController AnchoredPrefab;

    private Action<AnchoredObjectController> placementDone;
   
    public void StartPlacementDelayed(Action<AnchoredObjectController> placementDone)
    {
        this.placementDone = placementDone;
        StartCoroutine(StartPlacement());
    }

    IEnumerator StartPlacement()
    {
        yield return new WaitForSeconds(1.0f);
        
        headPlacementEventChannel.OnSetHeadtrackedObject(gameObject);
        headPlacementEventChannel.PlanePlacementRequested.AddListener(obj => OnPlanePlacementRequested(obj));
    }

    private void OnPlanePlacementRequested(ARPlane plane)
    {
        headPlacementEventChannel.PlanePlacementRequested.RemoveListener(OnPlanePlacementRequested);

        // Instantiate at location of this temporary object
        var anchoredObject = Instantiate(AnchoredPrefab, this.transform.position, this.transform.rotation, this.transform.parent);
        placementDone?.Invoke(anchoredObject);
        Destroy(this);
    }
}
