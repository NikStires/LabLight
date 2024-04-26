using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

public class TapToPlaceObject : MonoBehaviour
{
    [SerializeField]
    public PlaneInteractionManagerScriptableObject planeInteractionManager;

    [SerializeField] GameObject tapToPlacePrefab;


    // Start is called before the first frame update

    public void OnTriggerEnter(Collider collider)
    {
        if(collider.transform.parent != null && collider.transform.parent.name == "HandJointPrefab(Clone)")
        {
            Debug.Log("PlaneInteractionMananger: collision with hand joint");
            planeInteractionManager.OnFingerTipPlaneCollision(collider.transform.position);
        }
    }

    public void PlaneSelected()
    {
        planeInteractionManager.OnPlanePlacementRequested(this.GetComponent<ARPlane>());
        Debug.Log("Plane placement requested");
    }

}
