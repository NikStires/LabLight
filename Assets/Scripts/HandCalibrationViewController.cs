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
    //[SerializeField] GameObject origin;
    [SerializeField] GameObject originPrefab;

    [SerializeField] GameObject jointPrefab;

    [SerializeField] Material fillMaterial;

    [SerializeField] GameObject tapToPlacePrefab;

    public static XRHandJointID[] calibrationJoints = new XRHandJointID[]
    {
        //XRHandJointID.ThumbTip,
        XRHandJointID.IndexTip,
        XRHandJointID.MiddleTip,
        XRHandJointID.RingTip,
        XRHandJointID.LittleTip,
        XRHandJointID.IndexProximal,
        XRHandJointID.MiddleProximal,
        XRHandJointID.RingProximal,
        XRHandJointID.LittleProximal
    };

    public Dictionary<XRHandJointID, Pose> calibrationJointsPoseDict = new Dictionary<XRHandJointID, Pose>();

    ARPlane planeSelected = null;

    List<ARPlane> tablePlanes = new List<ARPlane>();

    XRHandSubsystem m_HandSubsystem;

    Dictionary<XRHandJointID, GameObject> jointsRight = new();
    Dictionary<XRHandJointID, GameObject> jointsLeft = new();

    private bool inCalibration = false;

    ARPlaneManager planeManager = null;
    ARAnchorManager anchorManager = null;
    
    public float distanceThreshold = 0.08f;

    private float progress = -0.4f;
    private float lerpDuration = 3f;

    private void Start()
    {
        fillMaterial.SetFloat("_FillRate", -0.4f);
        //StartCoroutine(CalibrationAnimation());
        var handSubsystems = new List<XRHandSubsystem>();
        planeManager = SessionManager.instance.planeManager;
        anchorManager = SessionManager.instance.anchorManager;
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

    public void OnPlanesChanged(ARPlanesChangedEventArgs changes)
    {
        foreach(var plane in changes.added)
        {
            //disable plane if not a table
            if(plane.classification != PlaneClassification.Table)
            {
                plane.gameObject.SetActive(false);
            }else
            {
                tablePlanes.Add(plane);
            }
        }

        foreach(var plane in changes.updated)
        {
            if(tablePlanes.Contains(plane))
            {
                tablePlanes[tablePlanes.IndexOf(plane)] = changes.updated[changes.updated.IndexOf(plane)];
            }
        }

        foreach(var plane in changes.removed)
        {
            if(tablePlanes.Contains(plane))
            {
                tablePlanes.Remove(plane);
            }
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
                    // Debug.Log("Lablight: disabling all other planes");
                    foreach(ARPlane plane in tablePlanes)
                    {
                        if(plane != planeSelected)
                        {
                            //plane.gameObject.SetActive(false);
                            plane.transform.Find("Cube").gameObject.SetActive(false);
                        }
                    }

                    //if calibration joints are within distance threshold y of plane, start calibration
                    if(!inCalibration)
                    {
                        //check if all calibration joints are within distance threshold y of plane
                        Debug.Log("Lablight: Checking if all calibration joints are within " + distanceThreshold + " y of plane");
                        Debug.Log("Lablight: calibrationJointsPoseDict.Count " + calibrationJointsPoseDict.Count);
                        if(calibrationJointsPoseDict.Count == calibrationJoints.Length)
                        {
                            foreach(KeyValuePair<XRHandJointID, Pose> joint in calibrationJointsPoseDict)
                            {
                                if(Mathf.Abs(joint.Value.position.y - planeSelected.center.y) > distanceThreshold)
                                {
                                    Debug.Log("Lablight: " + joint.Key + " y position: " + joint.Value.position.y + " plane y position: " + planeSelected.center.y);
                                    Debug.Log("Lablight: " + joint.Key + " is not within " + distanceThreshold + " y of plane");
                                    return;
                                }else
                                {
                                    Debug.Log("Lablight: " + joint.Key + " y position: " + joint.Value.position.y + " plane y position: " + planeSelected.center.y);
                                    Debug.Log("Lablight: " + joint.Key + " is within " + distanceThreshold + " y of plane");
                                }
                            }
                            Debug.Log("Lablight: Starting calibration");
                            StartCoroutine(startCalibration());
                        }
                    }
                }
            break;
        }
    }


    public ARPlane IsInBoundsOfPlane(Vector3 jointPosition)
    {
        if(jointPosition != null)
        {
            foreach (ARPlane plane in tablePlanes)
            {
                Vector3 centerOffset = new Vector3(plane.center.x - plane.centerInPlaneSpace.x, plane.center.y - plane.centerInPlaneSpace.y, 0f);
                Vector2[] adjustedBoundary = plane.boundary.Select(point => new Vector2(point.x + centerOffset.x, point.y + centerOffset.y)).ToArray();

                if (IsPointInPolygon(adjustedBoundary, new Vector2(jointPosition.x, jointPosition.z)))
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

    private IEnumerator CalibrationAnimation()
    {
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        StartCoroutine(LerpRingScale());
    }

    private IEnumerator LerpRingScale()
    {
        float timeElapsed = 0;
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
    }

    private IEnumerator DeactivateFingerPoints()
    {
        yield return new WaitForSeconds(0.05f);
        progressRing.gameObject.SetActive(false);
    }

    private IEnumerator startCalibration()
    {
        inCalibration = true;
        //StartCoroutine(LerpRingScale());

        // Start a countdown timer
        float countdown = 5f;
        Pose[] initialJointPositions = calibrationJointsPoseDict.Values.ToArray();
        //send joint positions to lighthouse
        StartCoroutine(HighlightCalibrationJoints(initialJointPositions));
        while (countdown > 0)
        {
            Debug.Log("Lablight: countdown " + countdown);
            // Highlight each finger tip as each second passes
            countdown -= Time.deltaTime;

            // Check if the hand moved out of a given distance
            if (HasMovedOutOfDistance(initialJointPositions, calibrationJointsPoseDict.Values.ToArray()))
            {
                // Stop the calibration process
                inCalibration = false;
                StopCoroutine(LerpRingScale());
                //StopCoroutine(CalibrationAnimation());
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
        Pose calibrationPose = calibrationJointsPoseDict[XRHandJointID.MiddleTip];
        calibrationPose.position.y = planeSelected.transform.position.y;
        ARAnchor anchor = anchorManager.AttachAnchor(planeSelected, calibrationPose);
        var originInstance = Instantiate(originPrefab, calibrationPose.position, Quaternion.identity);
        originInstance.transform.parent = anchor.transform;
    }

    private IEnumerator HighlightCalibrationJoints(Pose[] jointPositions)
    {
        GameObject[] jointObjects = new GameObject[jointPositions.Length];
        foreach (Pose jointPose in jointPositions)
        {
            if(HasMovedOutOfDistance(jointPositions, calibrationJointsPoseDict.Values.ToArray()))
            {
                foreach(GameObject jointGameObject in jointObjects)
                {
                    Destroy(jointGameObject);
                }
                yield break;
            }
            GameObject joint = Instantiate(jointPrefab, jointPose.position, jointPose.rotation);
            Debug.Log("Instantiating joint at " + jointPose.position);
            jointObjects.Append(joint);
            joint.transform.localScale = new Vector3(1f, 1f, 1f);
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
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
