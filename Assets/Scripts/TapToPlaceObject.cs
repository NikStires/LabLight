using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

public class TapToPlaceObject : MonoBehaviour
{
    [SerializeField] MeshRenderer progressRing;
    [SerializeField] MeshRenderer thumbTip;
    [SerializeField] MeshRenderer pointerTip;
    [SerializeField] MeshRenderer middleTip;
    [SerializeField] MeshRenderer ringTip;
    [SerializeField] MeshRenderer pinkyTip;
    [SerializeField] GameObject origin;
    XRHandSubsystem m_HandSubsystem;

    public GameObject jointPrefab;

    Dictionary<XRHandJointID, GameObject> jointsRight = new();
    Dictionary<XRHandJointID, GameObject> jointsLeft = new();

    [SerializeField] GameObject tapToPlacePrefab;

    private bool inCalibration = false;

    private float progress = -0.4f;

    private float lerpDuration = 3f;

    private void Start()
    {
        //StartCoroutine(CalibrationAnimation());
        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);

        for (var i = 0; i < handSubsystems.Count; ++i)
        {
            var handSubsystem = handSubsystems[i];
            if (handSubsystem.running)
            {
                m_HandSubsystem = handSubsystem;
                break;
            }
        }


        if (m_HandSubsystem != null)
            m_HandSubsystem.updatedHands += OnUpdatedHands;

        var anchorManager = GetComponent<ARAnchorManager>();
    }

    void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        // Debug.Log("this plane is a " + this.GetComponent<ARPlane>().classification);
        // if(this.GetComponent<ARPlane>().classification == PlaneClassification.Table)
        // {
        //     switch(updateType)
        //     {
        //         case XRHandSubsystem.UpdateType.BeforeRender:
        //             //identify closest plane to hands
        //             Debug.Log("this is being called");
        //             for (var i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex();  i++)
        //             {
        //                 XRHandJointID jointID = XRHandJointIDUtility.FromIndex(i);

        //                 var trackingDataRight = m_HandSubsystem.rightHand.GetJoint(jointID);
        //                 var trackingDataLeft = m_HandSubsystem.leftHand.GetJoint(jointID);

        //                 if (trackingDataRight.TryGetPose(out Pose poseRight))
        //                 {
        //                     Debug.Log("Distance between center of plane and right hand" + Vector3.Distance(poseRight.position, this.GetComponent<ARPlane>().center));
        //                     if(Vector3.Distance(poseRight.position, this.GetComponent<ARPlane>().center) < 2)
        //                     {
        //                         Debug.Log("Your right hand is close to a plane");
        //                     }
        //                 }
        //                 if (trackingDataLeft.TryGetPose(out Pose poseLeft))
        //                 {
        //                     Debug.Log("Distance between center of plane and left hand" + (Vector3.Distance(poseLeft.position, this.GetComponent<ARPlane>().center)));
        //                     if(Vector3.Distance(poseLeft.position, this.GetComponent<ARPlane>().center) < 2)
        //                     {
        //                         Debug.Log("Your left hand is close to a plane");
        //                     }
        //                 }
        //             }
        //             break;
        //     }
        // }
    }


    // Start is called before the first frame update
    public void OnInteractableEnter()
    {
        // Debug.Log("interactable entered");
        // XRHandJoint indexFinger = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.IndexTip);
        // Debug.Log("got joint"); 
        // if(indexFinger.TryGetPose(out Pose pose))
        // {
        //     /*
        //     Pose xrOrigin = new Pose(planeManager.Transform.position, planeManager.Transform.rotation);
        //     pose = pose.GetTransformedBy(xrOrigin);*/
        //     //ARAnchor anchor = SessionManager.instance.anchorManager.AttachAnchor(this.GetComponent<ARPlane>(), pose);
        //     Debug.Log("placing cube at ");
        //     Debug.Log(pose.position);
        //     var instance = Instantiate(tapToPlacePrefab, pose.position, Quaternion.identity);
        //     //instance.transform.parent = anchor.transform;
        // }else
        // {
        //     Debug.Log("Getting pose failed");
        // }
    }

    public void SetupCalibration()
    {
        Debug.Log("Starting calibration");

    }

    // private IEnumerator startCalibration()
    // {
    //     inCalibration = true;

    //     StartCoroutine(LerpRingScale());
    // }

    private IEnumerator LerpRingScale()
    {
        float timeElapsed = 0;
        origin.SetActive(true);
        while (timeElapsed < lerpDuration && inCalibration)
        {
            //progressRing.transform.localScale = progressRing.transform.localScale * Mathf.Lerp(1f, 0f, timeElapsed / lerpDuration);
            if(progressRing.transform.localScale.x < 0.22f)
            {
                //deactivate finger points
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        progressRing.transform.localScale = new Vector3(0,0,0);
        yield return new WaitForSeconds(3f);
        origin.SetActive(false);
    }
}
