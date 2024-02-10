using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Linq;
using System;

public class HandCalibrationViewController : MonoBehaviour
{
    [SerializeField] MeshRenderer progressRing;
    [SerializeField] MeshRenderer thumbTip;
    [SerializeField] MeshRenderer pointerTip;
    [SerializeField] MeshRenderer middleTip;
    [SerializeField] MeshRenderer ringTip;
    [SerializeField] MeshRenderer pinkyTip;
    [SerializeField] GameObject origin;

    [SerializeField] GameObject jointPrefab;

    [SerializeField] Material fillMaterial;

    [SerializeField] GameObject tapToPlacePrefab;

    public static XRHandJointID[] calibrationJoints = new XRHandJointID[]
    {
        XRHandJointID.ThumbTip,
        XRHandJointID.IndexTip,
        XRHandJointID.MiddleTip,
        XRHandJointID.RingTip,
        XRHandJointID.LittleTip
    };

    public Dictionary<XRHandJointID, Pose> calibrationJointsPoseDict = new Dictionary<XRHandJointID, Pose>();

    ARPlane planeSelected = null;

    XRHandSubsystem m_HandSubsystem;

    Dictionary<XRHandJointID, GameObject> jointsRight = new();
    Dictionary<XRHandJointID, GameObject> jointsLeft = new();

    private bool inCalibration = false;

    ARPlaneManager planeManager = null;

    private float progress = -0.4f;
    private float lerpDuration = 3f;

    private void Start()
    {
        fillMaterial.SetFloat("_FillRate", -0.4f);
        //StartCoroutine(CalibrationAnimation());
        var handSubsystems = new List<XRHandSubsystem>();
        planeManager = SessionManager.instance.planeManager;
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

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(IsInBoundsOfPlane(new Vector3(0, 0, 0)));
        }
    }
    void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        switch(updateType)
        {
            case XRHandSubsystem.UpdateType.BeforeRender:
                //if hand is within bounds disable all other planes
                for (var i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex();  i++)
                {
                    XRHandJointID jointID = XRHandJointIDUtility.FromIndex(i);

                    var trackingDataRight = m_HandSubsystem.rightHand.GetJoint(jointID);
                    //var trackingDataLeft = m_HandSubsystem.leftHand.GetJoint(jointID);

                    if (trackingDataRight.TryGetPose(out Pose poseRight))
                    {
                        planeSelected = IsInBoundsOfPlane(poseRight.position);
                        if(planeSelected != null)
                        {
                            Debug.Log("Lablight: plane selected " + planeSelected.trackableId);
                            planeSelected.transform.Find("Cube").gameObject.SetActive(true);
                            //can only calibrate with right hand for now

                            //store relevant joints to check if they are within 0.05 y of plane
                            if(calibrationJoints.Contains(jointID))
                            {
                                Debug.Log("Lablight: storing " + jointID + " pose");
                                calibrationJointsPoseDict[jointID] = poseRight;
                            }
                        }else
                        {
                            Debug.Log("Lablight: no plane selected");
                            foreach(ARPlane plane in planeManager.trackables)
                            {
                                plane.transform.Find("Cube").gameObject.SetActive(false);
                            }
                        }
                    }
                    // if (trackingDataLeft.TryGetPose(out Pose poseLeft))
                    // {
                    //     // Debug.Log("Distance between center of plane and left hand" + (Vector3.Distance(poseLeft.position, this.GetComponent<ARPlane>().center)));
                    //     // if(Vector3.Distance(poseLeft.position, this.GetComponent<ARPlane>().center) < 2)
                    //     // {
                    //     //     Debug.Log("Your left hand is close to a plane");
                    //     // }
                    // }
                }
                //if hand is within the bounds of a plane, disable all other planes.
                //else if hand is not within the bounds of the selected plane enable all planes if disabled
                if(planeSelected != null) 
                {
                    //disable planes
                    Debug.Log("Lablight: disabling all other planes");
                    foreach(ARPlane plane in planeManager.trackables)
                    {
                        if(plane != planeSelected)
                        {
                            //plane.gameObject.SetActive(false);
                            plane.transform.Find("Cube").gameObject.SetActive(false);
                        }
                    }

                    //if calibration joints are within 0.05 y of plane, start calibration
                    if(!inCalibration)
                    {
                        //check if all calibration joints are within 0.05 y of plane
                        Debug.Log("Lablight: Checking if all calibration joints are within 0.05 y of plane");
                        Debug.Log("Lablight: calibrationJointsPoseDict.Count " + calibrationJointsPoseDict.Count);
                        if(calibrationJointsPoseDict.Count == 5)
                        {
                            foreach(KeyValuePair<XRHandJointID, Pose> joint in calibrationJointsPoseDict)
                            {
                                if(Mathf.Abs(joint.Value.position.y - planeSelected.center.y) > 0.06f)
                                {
                                    Debug.Log("Lablight: " + joint.Key + " y position: " + joint.Value.position.y + " plane y position: " + planeSelected.center.y);
                                    Debug.Log("Lablight: " + joint.Key + " is not within 0.06 y of plane");
                                    return;
                                }else
                                {
                                    Debug.Log("Lablight: " + joint.Key + " y position: " + joint.Value.position.y + " plane y position: " + planeSelected.center.y);
                                    Debug.Log("Lablight: " + joint.Key + " is within 0.06 y of plane");
                                }
                            }
                            Debug.Log("Lablight: Starting calibration");
                            StartCoroutine(startCalibration());
                        }
                    }
                }
                else
                {
                    // //enable planes
                    // foreach(ARPlane plane in planeManager.trackables)
                    // {
                    //     plane.gameObject.SetActive(true);
                    // }
                }
            break;
        }
    }


    public ARPlane IsInBoundsOfPlane(Vector3 jointPosition)
    {
        if(jointPosition != null)
        {
            foreach (ARPlane plane in planeManager.trackables)
            {
                //ARPlane plane = planeManager.GetPlane(planeTrackable.trackableId);
                Vector3 centerOffset = new Vector3(plane.center.x - plane.centerInPlaneSpace.x, plane.center.y - plane.centerInPlaneSpace.y, 0f);
                Vector2[] adjustedBoundary = plane.boundary.Select(point => new Vector2(point.x + centerOffset.x, point.y + centerOffset.y)).ToArray();

                if (IsPointInPolygon(adjustedBoundary, new Vector2(jointPosition.x, jointPosition.z)) && plane.classification == PlaneClassification.Table)
                {
                    return plane;
                }
            }
        }
        return null;
    }
            

    public bool IsPointInPolygon(Vector2[] polygon, Vector2 point)
    {
        bool isInside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }
        return isInside;
    }


    void OnInteractableEnter()
    {
        //Pose pointerFingerPose = new Pose(origin.position, origin.rotation);
        XRHandJoint trackingData = m_HandSubsystem.rightHand.GetJoint(XRHandJointIDUtility.FromIndex((int)XRHandJointID.IndexTip));
        if(trackingData.TryGetPose(out Pose pose))
        {
            /*
            Pose xrOrigin = new Pose(planeManager.Transform.position, planeManager.Transform.rotation);
            pose = pose.GetTransformedBy(xrOrigin);*/
            var anchorManager = GetComponent<ARAnchorManager>();
            //ARAnchor anchor = anchorManager.AttachAnchor(plane, pose);
            //var instance = Instantiate(tapToPlacePrefab, pose.position, Quaternion.identity);
            //instance.transform.parent = anchor.transform;
        }
    }

    private IEnumerator CalibrationAnimation()
    {
        thumbTip.gameObject.SetActive(true);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        pointerTip.gameObject.SetActive(true);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        middleTip.gameObject.SetActive(true);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        ringTip.gameObject.SetActive(true);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        pinkyTip.gameObject.SetActive(true);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        StartCoroutine(LerpRingScale());
    }

    private IEnumerator LerpRingScale()
    {
        float timeElapsed = 0;
        origin.SetActive(true);
        while (timeElapsed < lerpDuration)
        {
            progressRing.transform.localScale = progressRing.transform.localScale * Mathf.Lerp(1f, 0f, timeElapsed / lerpDuration);
            if(progressRing.transform.localScale.x < 0.22f)
            {
                StartCoroutine(DeactivateFingerPoints());
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        progressRing.transform.localScale = new Vector3(0,0,0);
        yield return new WaitForSeconds(3f);
        origin.SetActive(false);
    }

    private IEnumerator DeactivateFingerPoints()
    {
        yield return new WaitForSeconds(0.05f);
        progressRing.gameObject.SetActive(false);
        thumbTip.gameObject.SetActive(false);
        pointerTip.gameObject.SetActive(false);
        middleTip.gameObject.SetActive(false);
        ringTip.gameObject.SetActive(false);
        pinkyTip.gameObject.SetActive(false);
    }

    private IEnumerator startCalibration()
    {
        inCalibration = true;
        //StartCoroutine(LerpRingScale());

        // Start a countdown timer
        float countdown = 5f;
        Pose[] initialJointPositions = calibrationJointsPoseDict.Values.ToArray();
        while (countdown > 0)
        {
            Debug.Log("Lablight: countdown " + countdown);
            // Highlight each finger tip as each second passes
            HighlightFingerTip((int)Math.Ceiling(5 - countdown));
            countdown -= Time.deltaTime;

            // Check if the hand moved out of a given distance
            if (HasMovedOutOfDistance(initialJointPositions, calibrationJointsPoseDict.Values.ToArray()))
            {
                // Stop the calibration process
                //StopCalibration();
                inCalibration = false;
                // Show a text window saying calibration failed
                ShowCalibrationFailedMessage();
                DeactivateFingerPoints();
                yield break; // Exit the coroutine
            }

            yield return null;
        }

        // Calibration succeeded
        //DeactivateFingerPoints();
        ShowCalibrationSucceededMessage();
        planeSelected.transform.Find("Cube").GetComponent<Renderer>().material.color = Color.green;
    }

    private void HighlightFingerTip(int index)
    {
        Debug.Log("Lablight: highlighting finger tip " + index);
        switch (index)
        {
            case 1:
                thumbTip.transform.position = calibrationJointsPoseDict[XRHandJointID.ThumbTip].position;
                thumbTip.gameObject.SetActive(true);
                break;
            case 2:
                pointerTip.transform.position = calibrationJointsPoseDict[XRHandJointID.IndexTip].position;
                pointerTip.gameObject.SetActive(true);
                break;
            case 3:
                middleTip.transform.position = calibrationJointsPoseDict[XRHandJointID.MiddleTip].position;
                middleTip.gameObject.SetActive(true);
                break;
            case 4:
                ringTip.transform.position = calibrationJointsPoseDict[XRHandJointID.RingTip].position;
                ringTip.gameObject.SetActive(true);
                break;
            case 5:
                pinkyTip.transform.position = calibrationJointsPoseDict[XRHandJointID.LittleTip].position;
                pinkyTip.gameObject.SetActive(true);
                break;
        }
    }

    private void StopCalibration()
    {
        inCalibration = false;
        StopCoroutine(LerpRingScale());
        DeactivateFingerPoints();
    }

    private void ShowCalibrationFailedMessage()
    {
        Debug.Log("Lablight: Calibration failed");
    }

    private void ShowCalibrationSucceededMessage()
    {
        Debug.Log("Lablight: Calibration succeeded");
    }

    public bool HasMovedOutOfDistance(Pose[] initialJointPositions, Pose[] currentJointPositions)
    {
        for (int i = 0; i < initialJointPositions.Length; i++)
        {
            if (Vector3.Distance(initialJointPositions[i].position, currentJointPositions[i].position) > 0.05f)
            {
                return true;
            }
        }
        return false;
    }

}
