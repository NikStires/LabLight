using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;

public class ARPlaneViewController : MonoBehaviour
{
    public static ARPlaneViewController instance;
    private ARPlaneManager planeManager = null;

    private List<ARPlane> planes = new List<ARPlane>();

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("ARPlaneViewController already exists, destroying new instance");
            DestroyImmediate(gameObject);
        }
        planeManager = this.transform.parent.GetComponent<ARPlaneManager>();
        Debug.Log("ar plane view controller awake " + planeManager);
        if(planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
    }


    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach(var plane in args.added)
        {
            planes.Add(plane);
        }

        foreach(var plane in args.updated)      //planes should only be disabled once when added, otherwise up to other components disgression
        {
            if(!planes.Contains(plane))
            {
                planes.Add(plane);
            }
        }

        foreach(var plane in args.removed)
        {
            if(planes.Contains(plane))
            {
                planes.Remove(plane);
            }
        }
    }

    public List<ARPlane> GetPlanesByClassification(List<PlaneClassification> classifications)
    {
        List<ARPlane> returnList = new List<ARPlane>();
        foreach(var plane in planes)
        {
            if(classifications.Contains(plane.classification))
            {
                returnList.Add(plane);
            }
        }
        return returnList;
    }

    public List<ARPlane> GetPlanesWithinDistanceAndClassification(float distance, List<PlaneClassification> classifications = null) //user can pass in a list of classifications optionally that they would like to get
    {
        List<ARPlane> returnList = new List<ARPlane>();
        foreach(var plane in planes)
        {
            if(Vector3.Distance(plane.transform.position, Camera.main.transform.position) < distance)
            {
                if(classifications == null || classifications.Contains(plane.classification))
                {
                    returnList.Add(plane);
                }
            }
        }
        return returnList;
    }
}
