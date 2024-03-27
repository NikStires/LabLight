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
using UniRx;
using TMPro;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

public class HandCalibrationViewController : MonoBehaviour
{
    //[SerializeField] MeshRenderer progressRing;
    //[SerializeField] GameObject origin;
    [SerializeField] GameObject originPrefab;

    [SerializeField] GameObject jointPrefab;

    [SerializeField] Material fillMaterial;

    [SerializeField] GameObject tapToPlacePrefab;

    [SerializeField] List<GameObject> fingerPoints = new List<GameObject>();

    [SerializeField] public CalibrationManagerScriptableObject calibrationManager;

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

    List<ARPlane> calibrationPlanes = new List<ARPlane>();

    ARPlane planeSelected = null;

    XRHandSubsystem m_HandSubsystem;
    ARAnchorManager anchorManager = null;

    private Coroutine calibrationCoroutine = null;

    public bool calibrationCountdownStarted = false;
    
    public float distanceThreshold = 0.02f;

    public float calibrationDistanceThreshold = 0.03f;

    private float progress = -0.4f;
    private float lerpDuration = 3f;

    private bool inCalibration = false;

    private void Awake()
    {
        fillMaterial.SetFloat("_FillRate", -0.4f);
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
    }

    private void OnEnable() 
    {
        RequestCalibration();
#if UNITY_EDITOR
        StartCoroutine(UnloadCalibration());
#endif
    }

    private IEnumerator UnloadCalibration()
    {
        yield return new WaitForSeconds(5f);
        SceneLoader.Instance.UnloadScene("Calibration");
    }

    public void RequestCalibration()
    {
        //planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
        //anchorManager.enabled = true;

        Debug.Log("calibration requested");
        if(m_HandSubsystem != null)
        {
            m_HandSubsystem.updatedHands += OnUpdatedHands;
        }
        //start calibration
        calibrationManager.UpdateCalibrationStatus("Looking for planes");
        //calibrationPlanes = SessionManager.instance.planeViewManager.GetPlanesByClassification(new List<PlaneClassification> { PlaneClassification.Table, PlaneClassification.None });
        calibrationPlanes = ARPlaneViewController.instance.GetPlanesByClassification(new List<PlaneClassification> { PlaneClassification.Table, PlaneClassification.None });
        foreach(var plane in calibrationPlanes)
        {
            plane.gameObject.SetActive(true);
        }
        
        //if calibration completed successfully, send calibration data to lighthouse and exit calibration mode
        //store started lighthouse origin and current plane in session manager
    }

    public void CompleteCalibration()
    {
        inCalibration = false;
        calibrationManager.UpdateCalibrationStatus("Calibration complete");
        calibrationManager.CalibrationStarted(false);
        planeSelected.gameObject.SetActive(false);
        //ARAnchor anchor = anchorManager.AttachAnchor(planeSelected, calibrationPose);
        var originInstance = Instantiate(originPrefab, SessionManager.instance.CharucoTransform.position, SessionManager.instance.CharucoTransform.rotation);

        calibrationCoroutine = null;

        Debug.Log("Lablight: origin position " + originInstance.transform.position);

        if(m_HandSubsystem != null)
        {
            m_HandSubsystem.updatedHands -= OnUpdatedHands;
        }
        planeSelected.transform.Find("Cube").gameObject.SetActive(false);
        planeSelected = null;

        //SessionManager.instance.planeViewManager.disableAllPlanes();
        ARPlaneViewController.instance.disableAllPlanes();
 
        SceneLoader.Instance.UnloadScene("Calibration");
    }

    void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        switch(updateType)
        {
            case XRHandSubsystem.UpdateType.Dynamic:
                for (var i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex();  i++)
                {
                    XRHandJointID jointID = XRHandJointIDUtility.FromIndex(i);

                    var trackingDataRight = m_HandSubsystem.rightHand.GetJoint(jointID);
                    //var trackingDataLeft = m_HandSubsystem.leftHand.GetJoint(jointID);

                    if (trackingDataRight.TryGetPose(out Pose poseRight))
                    {
                        if(jointID == XRHandJointID.MiddleProximal)
                        {
                            if(Physics.Raycast(poseRight.position, Vector3.down, out RaycastHit hit, 1f))
                            {
                                if(hit.collider.TryGetComponent<ARPlane>(out ARPlane hitPlane))
                                {
                                    if(calibrationPlanes.Contains(hitPlane))
                                    {
                                        if(planeSelected == null)
                                        {
                                            planeSelected = hitPlane;
                                            planeSelected.transform.Find("Cube").gameObject.SetActive(true);
                                        }else if(planeSelected != hitPlane)
                                        {
                                            planeSelected.transform.Find("Cube").gameObject.SetActive(false);
                                            //Debug.Log("new plane selected");
                                            planeSelected = hitPlane;
                                        }
                                    }
                                    // if(hitPlane != null)
                                    // {
                                    //     if(hitPlane.classification == PlaneClassification.Table)
                                    //     {
                                    //         //Debug.Log("Lablight: hand distance above plane " + hit.distance);
                                    //         if(planeSelected == null)
                                    //         {
                                    //             planeSelected = hitPlane;
                                    //             planeSelected.transform.Find("Cube").gameObject.SetActive(true);
                                    //         }else if(planeSelected != hitPlane) //very rare case
                                    //         {
                                    //             planeSelected.transform.Find("Cube").gameObject.SetActive(false);
                                    //             //Debug.Log("new plane selected");
                                    //             planeSelected = hitPlane;
                                    //         }
                                    //     }else if(hitPlane.classification != PlaneClassification.Table)
                                    //     {
                                    //         if(planeSelected != null && !inCalibration) //if we aren't in calibration and we aren't over a table we unselect the plane. bool is there to correct cases in which raycast hits plane below the current plane
                                    //         {
                                    //             planeSelected.transform.Find("Cube").gameObject.SetActive(false);
                                    //             planeSelected = null;
                                    //         }
                                    //     }
                                    // }
                                }
                            }else //not above a plane and not in calibration
                            {
                                if(planeSelected != null && !inCalibration)
                                {
                                    planeSelected.transform.Find("Cube").gameObject.SetActive(false);
                                    planeSelected = null;
                                }
                            }
                        }
                        //only check if calibration joints are above plane
                        if(calibrationJoints.Contains(jointID))
                        {
                            //Debug.Log("Lablight: storing " + jointID + " pose");
                            calibrationJointsPoseDict[jointID] = poseRight;
                        }
                    }
                }
                if(planeSelected != null && calibrationJointsPoseDict.Count == calibrationJoints.Length && !inCalibration)
                {
                    inCalibration = true;
                    StartCalibrationOnPlane();
                }
            break;
        }
    }

    private void StartCalibrationOnPlane()
    {
        calibrationManager.UpdateCalibrationStatus("Plane selected, awaiting hand placement");
        calibrationManager.CalibrationStarted(inCalibration);

        //if calibration joints are within distance threshold y of plane, start calibration
        //check if all calibration joints are within distance threshold y of plane
        foreach(KeyValuePair<XRHandJointID, Pose> joint in calibrationJointsPoseDict)
        {
            if(Mathf.Abs(joint.Value.position.y - planeSelected.center.y) > distanceThreshold)
            {
                inCalibration = false;
                return;
            }   
        }
        calibrationManager.UpdateCalibrationStatus("Starting calibration");

        if(calibrationCoroutine == null)
        {
            calibrationCoroutine = StartCoroutine(startCalibration());
        }else
        {
            Debug.Log("Calibration coroutine already running");
        }
    }

    private void DeactivateFingerPoints()
    {
        //progressRing.gameObject.SetActive(false);
        foreach(GameObject fingerPoint in fingerPoints)
        {
            fingerPoint.SetActive(false);
            Destroy(fingerPoint);
        }
        fingerPoints.Clear();
    }

    private IEnumerator startCalibration()
    {
        Debug.Log("startCalibration: Starting calibration");
        calibrationManager.UpdateCalibrationStatus("Calibration in progress");

        yield return new WaitForSeconds(1.5f); // to remove, debug waiting 1 second to ensure optimal hand position
        //call send data to lighthouse

        Pose[] initialJointPositions = calibrationJointsPoseDict.Values.ToArray();
        foreach (XRHandJointID joint in calibrationJoints)
        {
            if(HasMovedOutOfDistance(initialJointPositions, calibrationJointsPoseDict.Values.ToArray())) //wait 2 seconds for hand position to stabalize 
            {
                calibrationManager.UpdateCalibrationStatus("Calibration failed please place hands on plane");
                inCalibration = false;
                DeactivateFingerPoints();
                yield break;
            }
            fingerPoints.Append(Instantiate(jointPrefab, calibrationJointsPoseDict[joint].position, calibrationJointsPoseDict[joint].rotation));
            calibrationManager.UpdateCalibrationStatus("startCalibration: Instantiating joint at " + calibrationJointsPoseDict[joint].position);
            //jointInstance.transform.localScale = new Vector3(1f, 1f, 1f);
            //Debug.Log("Lablight: middle tip position: " + calibrationJointsPoseDict[XRHandJointID.MiddleTip].position.y + " plane selected y position: " +  planeSelected.transform.position.y);
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("startCalibration: instantiated all joints");
        Matrix4x4 calibrationMatrix = CalibrationFromMatrix.Calculate_Hand_Coordinate_System_Transform(true, 
            calibrationJointsPoseDict[XRHandJointID.IndexProximal].position.x, planeSelected.transform.position.y, calibrationJointsPoseDict[XRHandJointID.IndexProximal].position.z,
            calibrationJointsPoseDict[XRHandJointID.MiddleProximal].position.x, planeSelected.transform.position.y, calibrationJointsPoseDict[XRHandJointID.MiddleProximal].position.z,
            calibrationJointsPoseDict[XRHandJointID.RingProximal].position.x, planeSelected.transform.position.y, calibrationJointsPoseDict[XRHandJointID.RingProximal].position.z,
            calibrationJointsPoseDict[XRHandJointID.LittleProximal].position.x, planeSelected.transform.position.y, calibrationJointsPoseDict[XRHandJointID.LittleProximal].position.z,
            calibrationJointsPoseDict[XRHandJointID.IndexTip].position.x, planeSelected.transform.position.y, calibrationJointsPoseDict[XRHandJointID.IndexTip].position.z,
            calibrationJointsPoseDict[XRHandJointID.MiddleTip].position.x, planeSelected.transform.position.y, calibrationJointsPoseDict[XRHandJointID.MiddleTip].position.z,
            calibrationJointsPoseDict[XRHandJointID.RingTip].position.x, planeSelected.transform.position.y, calibrationJointsPoseDict[XRHandJointID.RingTip].position.z,
            calibrationJointsPoseDict[XRHandJointID.LittleTip].position.x, planeSelected.transform.position.y, calibrationJointsPoseDict[XRHandJointID.LittleTip].position.z
        );
        SessionManager.instance.UpdateCalibration(calibrationMatrix);
        CompleteCalibration();
        yield return null;
    }

    public bool HasMovedOutOfDistance(Pose[] initialPositions, Pose[] currentPositions)
    {
        for (int i = 0; i < initialPositions.Length; i++)
        {
            // if(Physics.Raycast(initialPositions[i].position, Vector3.down, out RaycastHit hit, 1f))
            // {
            //     if(hit.collider.GetComponent<ARPlane>() == planeSelected)
            //     {
            //         Debug.Log("Lablight: distance from plane " + hit.distance);
            //     }
            // }
            if (Vector3.Distance(initialPositions[i].position, currentPositions[i].position) > calibrationDistanceThreshold)
            {
                Debug.Log("HasMovedOutOfDistance: Hand detected out of distance by " + Vector3.Distance(initialPositions[i].position, currentPositions[i].position) + " units");
                return true;
            }
        }
        return false;
    }

    
}

    // private IEnumerator CalibrationAnimation()
    // {
    //     progress += 0.14f;
    //     fillMaterial.SetFloat("_FillRate", progress);
    //     yield return new WaitForSeconds(1f);
    //     progress += 0.14f;
    //     fillMaterial.SetFloat("_FillRate", progress);
    //     yield return new WaitForSeconds(1f);
    //     progress += 0.14f;
    //     fillMaterial.SetFloat("_FillRate", progress);
    //     yield return new WaitForSeconds(1f);
    //     progress += 0.14f;
    //     fillMaterial.SetFloat("_FillRate", progress);
    //     yield return new WaitForSeconds(1f);
    //     progress += 0.14f;
    //     fillMaterial.SetFloat("_FillRate", progress);
    //     StartCoroutine(LerpRingScale());
    // }

    // private IEnumerator LerpRingScale()
    // {
    //     float timeElapsed = 0;
    //     while (timeElapsed < lerpDuration)
    //     {
    //         progressRing.transform.localScale = progressRing.transform.localScale * Mathf.Lerp(1f, 0f, timeElapsed / lerpDuration);
    //         if(progressRing.transform.localScale.x < 0.22f)
    //         {
    //             StartCoroutine(DeactivateFingerPoints());
    //         }
    //         timeElapsed += Time.deltaTime;
    //         yield return null;
    //     }
    //     progressRing.transform.localScale = new Vector3(0,0,0);
    //     yield return new WaitForSeconds(3f);
    // }
