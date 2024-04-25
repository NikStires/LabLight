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

    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected");
        Debug.Log(collision.transform.name);
        if(collision.transform.parent.name == "HandJointPrefab(Clone)")
        {
            Debug.Log("PlaneInteractionMananger: collision with hand joint");
            planeInteractionManager.OnFingerTipPlaneCollision(collision.transform.position);
        }
    }

}
