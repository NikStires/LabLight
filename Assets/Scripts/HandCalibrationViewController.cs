using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Linq;
using System;
using UniRx;
using TMPro;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using Unity.PolySpatial;

public class HandCalibrationViewController : MonoBehaviour
{
    //[SerializeField] MeshRenderer progressRing;
    [SerializeField] GameObject originPrefab;

    [SerializeField] GameObject jointPrefab;

    [SerializeField] Material fillMaterial;

    [SerializeField] GameObject tapToPlacePrefab;

    [SerializeField] public CalibrationManagerScriptableObject calibrationManager;

    public static XRHandJointID[] calibrationJoints = new XRHandJointID[]
    {
        XRHandJointID.IndexTip,
        XRHandJointID.MiddleTip,
        XRHandJointID.RingTip,
        XRHandJointID.LittleTip,
        XRHandJointID.IndexProximal,
        XRHandJointID.MiddleProximal,
        XRHandJointID.RingProximal,
        XRHandJointID.LittleProximal
    };

    public static List<PlaneClassification> calibrationPlanesClassification = new List<PlaneClassification>()
    {
        PlaneClassification.Table,
        PlaneClassification.None
    }; 

    public Material planeMaterial;
    public Material removePlaneMaterial;

    public Dictionary<XRHandJointID, Pose> calibrationJointsPoseDict = new Dictionary<XRHandJointID, Pose>();

    List<ARPlane> calibrationPlanes;

    ARPlane planeSelected = null;

    XRHandSubsystem m_HandSubsystem;

    private Coroutine matrixCoroutine = null;

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
        yield return new WaitForSeconds(1f);
        SceneLoader.Instance.UnloadScene("Calibration");
    }

    public void RequestCalibration()
    {
        //planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
        //anchorManager.enabled = true;
        Debug.Log("calibration requested");

        //planeSelected.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { removePlaneMaterial });
        if(m_HandSubsystem != null)
        {
            m_HandSubsystem.updatedHands += OnUpdatedHands;
        }
        //start calibration
        calibrationManager.UpdateCalibrationStatus("Looking for planes");
        calibrationPlanes = ARPlaneViewController.instance.GetPlanesByClassification(calibrationPlanesClassification);
        //if calibration completed successfully, send calibration data to lighthouse and exit calibration mode
        //store started lighthouse origin and current plane in session manager
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
                                            planeSelected.GetComponent<MeshRenderer>().SetMaterials(new List<Material>(){ planeMaterial });
                                        }else if(planeSelected != hitPlane)
                                        {
                                            planeSelected.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { removePlaneMaterial });
                                            planeSelected = hitPlane;
                                        }
                                    }
                                }
                            }else //not above a plane and not in calibration
                            {
                                if(planeSelected != null && !inCalibration)
                                {
                                    planeSelected.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { removePlaneMaterial });
                                    planeSelected = null;
                                }
                            }
                        }
                        //only check if calibration joints are above plane
                        if(calibrationJoints.Contains(jointID))
                        {
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

        if(matrixCoroutine == null)
        {
            matrixCoroutine = StartCoroutine(getMatrixFromHandPosition());
        }else
        {
            Debug.Log("Calibration coroutine already running");
        }
    }

    private void DeactivateFingerPoints(List<GameObject> fingerPoints)
    {
        //progressRing.gameObject.SetActive(false);
        foreach(GameObject fingerPoint in fingerPoints)
        {
            Destroy(fingerPoint);
        }
        //fingerPoints.Clear();
    }

    private IEnumerator getMatrixFromHandPosition()
    {
        Debug.Log("getMatrixFromHandPosition: Starting calibration");
        calibrationManager.UpdateCalibrationStatus("Calibration in progress");

        yield return new WaitForSeconds(1.5f); // to remove, debug waiting 1 second to ensure optimal hand position
        //call send data to lighthouse

        Pose[] initialJointPositions = calibrationJointsPoseDict.Values.ToArray();

        List<GameObject> fingerPoints = new List<GameObject>();
        foreach (XRHandJointID joint in calibrationJoints)
        {
            if(HasMovedOutOfDistance(initialJointPositions, calibrationJointsPoseDict.Values.ToArray())) //wait 2 seconds for hand position to stabalize 
            {
                calibrationManager.UpdateCalibrationStatus("Calibration failed please place hands on plane");
                DeactivateFingerPoints(fingerPoints);
                matrixCoroutine = null;
                StartCalibrationOnPlane();
                yield break;
            }
            fingerPoints.Append(Instantiate(jointPrefab, calibrationJointsPoseDict[joint].position, calibrationJointsPoseDict[joint].rotation));
            calibrationManager.UpdateCalibrationStatus("getMatrixFromHandPosition: Instantiating joint " + joint);
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("getMatrixFromHandPosition: instantiated all joints");
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
        DeactivateFingerPoints(fingerPoints);
        yield return new WaitForSeconds(2f);
        CompleteCalibration();
        yield return null;
    }

    public void CompleteCalibration()
    {
        inCalibration = false;
        calibrationManager.UpdateCalibrationStatus("Calibration complete");
        calibrationManager.CalibrationStarted(false);
        //ARAnchor anchor = anchorManager.AttachAnchor(planeSelected, calibrationPose);
        var originInstance = Instantiate(originPrefab, SessionManager.instance.CharucoTransform.position, SessionManager.instance.CharucoTransform.rotation);

        Debug.Log("Lablight: origin position " + originInstance.transform.position);

        if(m_HandSubsystem != null)
        {
            m_HandSubsystem.updatedHands -= OnUpdatedHands;
        }

        calibrationPlanes = null;
        planeSelected.GetComponent<MeshRenderer>().SetMaterials(new List<Material> { removePlaneMaterial });
        planeSelected = null;
        StartCoroutine(UnloadCalibration());
    }

    public bool HasMovedOutOfDistance(Pose[] initialPositions, Pose[] currentPositions)
    {
        for (int i = 0; i < initialPositions.Length; i++)
        {
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
