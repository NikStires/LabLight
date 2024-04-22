using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

public class TapToPlaceObject : MonoBehaviour
{
    PlaneInteractionManagerScriptableObject planeInteractionManager;

    [SerializeField] GameObject tapToPlacePrefab;


    // Start is called before the first frame update

    public void OnCollisionEntry(Collision collision)
    {
        if(collision.transform.parent.name == "HandJointPrefab")
        {
            Debug.Log("collision with hand joint");
            planeInteractionManager.FingerTipPlaneCollision.Invoke(collision.transform.position);
        }
    }

}
