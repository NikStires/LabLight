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

public class HandCalibrationViewController : MonoBehaviour
{
    [SerializeField] MeshRenderer progressRing;
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

    ARPlane planeSelected = null;

    XRHandSubsystem m_HandSubsystem;

    ARPlaneManager planeManager = null;
    ARAnchorManager anchorManager = null;

    private Coroutine calibrationCoroutine = null;

    public bool calibrationCountdownStarted = false;
    
    public float distanceThreshold = 0.02f;

    private float progress = -0.4f;
    private float lerpDuration = 3f;

    private bool abovePlane = false;

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

        planeManager = SessionManager.instance.planeManager;
    }

    private void OnEnable() 
    {
        RequestCalibration();
    }

    public void RequestCalibration()
    {
        //start plane tracking
        //start hand tracking
        //planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
        //planeManager.enabled = true;
        //anchorManager.enabled = true;

        Debug.Log("calibration requested");
        if(m_HandSubsystem != null)
        {
            m_HandSubsystem.updatedHands += OnUpdatedHands;
        }   

        if(planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }

        if(planeSelected != null)
        {
            planeSelected.transform.Find("Cube").gameObject.SetActive(false); //disable any previous cubes
        }
        //start calibration
        calibrationManager.UpdateCalibrationStatus("Looking for planes");

        //if calibration completed successfully, send calibration data to lighthouse and exit calibration mode
        //store started lighthouse origin and current plane in session manager
    }

    public void CompleteCalibration(Vector3 originPosition)
    {
        inCalibration = false;
        calibrationManager.UpdateCalibrationStatus("Calibration complete");
        calibrationManager.CalibrationStarted(false);
        planeSelected.transform.Find("Cube").GetComponent<Renderer>().material.color = Color.green;
        //ARAnchor anchor = anchorManager.AttachAnchor(planeSelected, calibrationPose);
        var originInstance = Instantiate(originPrefab, originPosition, Quaternion.identity);
        calibrationCoroutine = null;

        Debug.Log("Lablight: Calibration complete");
        Debug.Log("Lablight: origin position " + originPosition);

        if(m_HandSubsystem != null)
        {
            m_HandSubsystem.updatedHands -= OnUpdatedHands;
        }

        if(planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (var plane in args.added)
        {
            if(plane.classification != PlaneClassification.Table)
            {
                plane.gameObject.SetActive(false);
            }
        }

        foreach(var plane in args.updated)
        {
            if(plane.classification != PlaneClassification.Table)
            {
                plane.gameObject.SetActive(false);
            }
        }
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
                                if(hit.collider.GetComponent<ARPlane>().classification == PlaneClassification.Table)
                                {
                                    //Debug.Log("Lablight: hand distance above plane " + hit.distance);
                                    if(planeSelected == null)
                                    {
                                        //Debug.Log("plane selected");
                                        abovePlane = true;
                                        //activate cube
                                        planeSelected = hit.collider.GetComponent<ARPlane>();
                                        planeSelected.GetComponent<ARPlane>().transform.Find("Cube").gameObject.SetActive(true);
                                    }else if(planeSelected != hit.collider.GetComponent<ARPlane>()) //very rare case
                                    {
                                        abovePlane = true;
                                        //decative old cube
                                        planeSelected.GetComponent<ARPlane>().transform.Find("Cube").gameObject.SetActive(false);
                                        //Debug.Log("new plane selected");
                                        planeSelected = hit.collider.GetComponent<ARPlane>();
                                    }else
                                    {
                                        //do nothing
                                        //Debug.Log("Lablight: plane selected " + planeSelected.trackableId);
                                    }
                                }else if(hit.collider.GetComponent<ARPlane>().classification != PlaneClassification.Table)
                                {
                                    if(planeSelected != null) //if not over any plane, disable current plane cube 
                                    {
                                        abovePlane = false;
                                        planeSelected.GetComponent<ARPlane>().transform.Find("Cube").gameObject.SetActive(false);
                                        planeSelected = null;
                                    }
                                }
                            }
                        }
                        //only check if calibration joints are above plane
                        if(calibrationJoints.Contains(jointID) && abovePlane)
                        {
                            //Debug.Log("Lablight: storing " + jointID + " pose");
                            calibrationJointsPoseDict[jointID] = poseRight;
                        }
                    }
                    // if (trackingDataLeft.TryGetPose(out Pose poseLeft)) left hand calibration todo
                    // {
                    // }
                }
                if(planeSelected != null && calibrationJointsPoseDict.Count == calibrationJoints.Length && !inCalibration)
                {
                    inCalibration = true;
                    if(planeManager != null)
                    {
                        planeManager.planesChanged -= OnPlanesChanged;
                    }
                    StartCalibrationOnPlane();
                }else if(planeSelected == null && inCalibration)
                {
                    if(planeManager != null)
                    {
                        planeManager.planesChanged += OnPlanesChanged;
                    }
                    inCalibration = false;
                    calibrationManager.CalibrationStarted(inCalibration);
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
            fingerPoints.Remove(fingerPoint);
            fingerPoint.SetActive(false);
            Destroy(fingerPoint);
        }
    }

    private IEnumerator startCalibration()
    {

        calibrationManager.UpdateCalibrationStatus("Calibration in progress");

        yield return new WaitForSeconds(1.5f); // to remove, debug waiting 1 second to ensure optimal hand position
        //call send data to lighthouse

        Pose[] initialJointPositions = calibrationJointsPoseDict.Values.ToArray();

        for (int i = 0; i < initialJointPositions.Length; i++)
        {
            initialJointPositions[i].position = new Vector3(initialJointPositions[i].position.x, planeSelected.center.y, initialJointPositions[i].position.z);
        }
        foreach (XRHandJointID joint in calibrationJoints)
        {

            if(HasMovedOutOfDistance(initialJointPositions, calibrationJointsPoseDict.Values.ToArray())) //wait 2 seconds for hand position to stabalize 
            {
                calibrationManager.UpdateCalibrationStatus("Calibration failed please place hands on plane");
                DeactivateFingerPoints();
                yield break;
            }
            Pose jointPose = calibrationJointsPoseDict[joint];
            GameObject jointInstance = Instantiate(jointPrefab, jointPose.position, jointPose.rotation);
            Debug.Log("Instantiating joint at " + jointPose.position);
            fingerPoints.Append(jointInstance);
            jointInstance.transform.localScale = new Vector3(1f, 1f, 1f);
            Debug.Log("Lablight: middle tip position: " + calibrationJointsPoseDict[XRHandJointID.MiddleTip].position.y + " plane selected y position: " +  planeSelected.center.y);

            yield return new WaitForSeconds(0.5f);
        }
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
        Vector3 originPosition = new Vector3(calibrationMatrix.m03, calibrationMatrix.m13, calibrationMatrix.m23);
        SessionManager.UpdateCalibration(calibrationMatrix);
        CompleteCalibration(originPosition);
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

            if (Vector3.Distance(initialPositions[i].position, currentPositions[i].position) > 0.05f)
            {
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
