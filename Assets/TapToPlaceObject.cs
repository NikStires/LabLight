using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

public class TapToPlaceObject : MonoBehaviour
{
    ARPlaneManager planeManager;

    XRHandSubsystem m_HandSubsystem;

    public GameObject jointPrefab;

    Dictionary<XRHandJointID, GameObject> jointsRight = new();
    Dictionary<XRHandJointID, GameObject> jointsLeft = new();

    [SerializeField] GameObject tapToPlacePrefab;

    private void Start()
    {
        //StartCoroutine(CalibrationAnimation());
        var handSubsystems = new List<XRHandSubsystem>();
        var planeManager = GetComponent<ARPlaneManager>();
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
    }

    void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        Debug.Log("this plane is a " + this.GetComponent<ARPlane>().classification);
        if(this.GetComponent<ARPlane>().classification == PlaneClassification.Table)
        {
            switch(updateType)
            {
                case XRHandSubsystem.UpdateType.BeforeRender:
                    //identify closest plane to hands
                    Debug.Log("this is being called");
                    for (var i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex();  i++)
                    {
                        Debug.Log("im in the 4 loop");
                        XRHandJointID jointID = XRHandJointIDUtility.FromIndex(i);

                        var trackingDataRight = m_HandSubsystem.rightHand.GetJoint(jointID);
                        var trackingDataLeft = m_HandSubsystem.leftHand.GetJoint(jointID);

                        if (trackingDataRight.TryGetPose(out Pose poseRight))
                        {
                            Debug.Log("Distance between center of plane and right hand" + Vector3.Distance(poseRight.position, this.GetComponent<ARPlane>().center));
                            if(Vector3.Distance(poseRight.position, this.GetComponent<ARPlane>().center) < 2)
                            {
                                Debug.Log("Your right hand is close to a plane");
                            }
                        }
                        if (trackingDataLeft.TryGetPose(out Pose poseLeft))
                        {
                            Debug.Log("Distance between center of plane and left hand" + (Vector3.Distance(poseLeft.position, this.GetComponent<ARPlane>().center)));
                            if(Vector3.Distance(poseLeft.position, this.GetComponent<ARPlane>().center) < 2)
                            {
                                Debug.Log("Your left hand is close to a plane");
                            }
                        }
                    }
                    break;
            }
        }
    }


    // Start is called before the first frame update
    public void OnInteractableEnter()
    {
        XRHandJoint indexFinger = m_HandSubsystem.rightHand.GetJoint(XRHandJointIDUtility.FromIndex((int)XRHandJointID.IndexTip));
        if(indexFinger.TryGetPose(out Pose pose))
        {
            /*
            Pose xrOrigin = new Pose(planeManager.Transform.position, planeManager.Transform.rotation);
            pose = pose.GetTransformedBy(xrOrigin);*/
            var anchorManager = GetComponent<ARAnchorManager>();
            ARAnchor anchor = anchorManager.AttachAnchor(this.GetComponent<ARPlane>(), pose);
            Debug.Log("placing well plate at " + pose.position);
            var instance = Instantiate(tapToPlacePrefab, anchor.transform.position, Quaternion.identity);
            instance.transform.parent = anchor.transform;
        }
    }
}
