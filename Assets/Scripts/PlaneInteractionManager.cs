using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Hands;

public class PlaneInteractionManager : MonoBehaviour
{
    public static PlaneInteractionManager instance;
    public PlaneInteractionManagerScriptableObject planeInteractionManager;

    [SerializeField] public GameObject defaultReticlePrefab;

    private GameObject reticle;

    private GameObject currentPrefab;

    [SerializeField] public GameObject jointTipPrefab; //joint prefabs which can be enabled with function primarily used to detect collision with plane

    [SerializeField] public Dictionary<XRHandJointID, GameObject> rightTips = new Dictionary<XRHandJointID, GameObject>();

    [SerializeField] public Dictionary<XRHandJointID, GameObject> leftTips = new Dictionary<XRHandJointID, GameObject>();

    [SerializeField] public float scaledThreshold = 0.1f; //scaled threshold for pinch detection

    [SerializeField] public Queue<GameObject> objectsQueue = new Queue<GameObject>(); //queue of objects to be placed on plane

    public static XRHandJointID[] tipIDs = new XRHandJointID [] //for now only account for 3 finger tips, no little or thumb
    {
        XRHandJointID.IndexTip,
        XRHandJointID.MiddleTip,
        XRHandJointID.RingTip
    };


    [SerializeField] Material planeMaterial;

    [SerializeField] Material invisiblePlaneMaterial;
    List<ARPlane> availablePlanes;

    ARPlane currentPlane; //used for head reticle placement

    public static List<PlaneClassification> planeClassifications = new List<PlaneClassification>
    {
        PlaneClassification.Wall,
        PlaneClassification.Table,
        PlaneClassification.Seat,
        PlaneClassification.None
    };

    XRHandSubsystem m_HandSubsystem;

    private bool headPlacementEnabled = false;

    private bool enableTapToPlace = false;

    // Start is called before the first frame update
    //tips for each finger so that we can detect position of 

    // when enabled default reticle will point to collisions with planes based on the forward direction of the camera

    // a prefab can be passed in which can replace the reticle with a custom object that allows for visualization of any object on plane

    // uses hand interaction scripted object events to detect pinch and release events

    // when pinching the object will be placed on the plane
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("PlaneInteractionManager already exists, destroying new instance");
            DestroyImmediate(gameObject);
        }
        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);

        for(int i = 0; i < handSubsystems.Count; i++)
        {
            if(handSubsystems[i].running)
            {
                m_HandSubsystem = handSubsystems[i];
                break;
            }
        }
    }

    private void OnEnable()
    {   
        if(m_HandSubsystem != null)
        {
            m_HandSubsystem.updatedHands += OnUpdatedHands;
        }

        availablePlanes = ARPlaneViewController.instance.GetPlanesByClassification(planeClassifications);

        AddSubscriptions();
    }

    private void OnDisable()
    {
        if(m_HandSubsystem != null)
        {
            m_HandSubsystem.updatedHands -= OnUpdatedHands;
        }
        RemoveSubscriptions();
    }

    private void AddSubscriptions()
    {
        planeInteractionManager.RequestObjectPlacement.AddListener(obj => SetPrefabs(obj));
        planeInteractionManager.EnableTapToPlace.AddListener(EnableTapToPlace);
        planeInteractionManager.DisableTapToPlace.AddListener(DisableTapToPlace);
        planeInteractionManager.FingerTipPlaneCollision.AddListener(obj => OnCollisionEntry(obj));
        planeInteractionManager.EnableHeadPlacement.AddListener(EnableHeadPlacement);
        planeInteractionManager.DisableHeadPlacement.AddListener(DisableHeadPlacement);
    }

    private void RemoveSubscriptions()
    {
        planeInteractionManager.RequestObjectPlacement.RemoveListener(SetPrefabs);
        planeInteractionManager.EnableTapToPlace.RemoveListener(EnableTapToPlace);
        planeInteractionManager.DisableTapToPlace.RemoveListener(DisableTapToPlace);
        planeInteractionManager.FingerTipPlaneCollision.RemoveListener(OnCollisionEntry);
        planeInteractionManager.EnableHeadPlacement.RemoveListener(EnableHeadPlacement);
        planeInteractionManager.DisableHeadPlacement.RemoveListener(DisableHeadPlacement);
    }
    
    void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        switch(updateType)
        {
            case XRHandSubsystem.UpdateType.Dynamic:
            if(enableTapToPlace)
            {
                //check current plane by raycasting from camera
                foreach(XRHandJointID jointID in tipIDs)
                {
                    var trackingDataRight = m_HandSubsystem.rightHand.GetJoint(jointID);
                    var trackingDataLeft = m_HandSubsystem.leftHand.GetJoint(jointID);

                    if(trackingDataRight.TryGetPose(out Pose poseRight))
                    {
                        if(!rightTips.ContainsKey(jointID))
                        {
                            rightTips[jointID] = Instantiate(jointTipPrefab, poseRight.position, poseRight.rotation);
                            rightTips[jointID].transform.localScale = new Vector3(2f, 2f, 2f);
                        }
                        else
                        {
                            rightTips[jointID].transform.SetPositionAndRotation(poseRight.position, poseRight.rotation);
                        }
                    }

                    if(trackingDataLeft.TryGetPose(out Pose poseLeft))
                    {
                        if(!leftTips.ContainsKey(jointID))
                        {
                            leftTips[jointID] = Instantiate(jointTipPrefab, poseLeft.position, poseLeft.rotation);
                            leftTips[jointID].transform.localScale = new Vector3(2f, 2f, 2f);
                        }
                        else
                        {
                            leftTips[jointID].transform.SetPositionAndRotation(poseLeft.position, poseLeft.rotation);
                        }
                    }
                }
            }

            //attempting to detect middle finger pinch

            // assign joint values
            // XRHandJoint rightMiddleTipJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.IndexTip);
            // XRHandJoint rightThumbTipJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.ThumbTip);

            XRHandJoint leftMiddleTip = m_HandSubsystem.leftHand.GetJoint(XRHandJointID.IndexTip);
            XRHandJoint leftThumbTip = m_HandSubsystem.leftHand.GetJoint(XRHandJointID.ThumbTip);
            // if((rightMiddleTipJoint.TryGetPose(out Pose rightMiddlePose) && rightThumbTipJoint.TryGetPose(out Pose rightThumbPose)))
            // {
            //     if(DetectPinch(rightMiddleTipJoint, rightThumbTipJoint) && currentPrefab != null)
            //     {
            //         Debug.Log("Pinch detected");
            //         LockObjectAndProgressQueue();
            //     }
            // }

            if(leftMiddleTip.TryGetPose(out Pose leftMiddlePose) && leftThumbTip.TryGetPose(out Pose leftThumbPose))
            {
                if(DetectPinch(leftMiddleTip, leftThumbTip))
                {
                    if(currentPrefab != null)
                    {
                        Debug.Log("Pinch detected on left hand, placing object");
                        LockObjectAndProgressQueue();
                    }else
                    {
                        Debug.Log("No object to place");
                    }
                }
            }
            break;
        }
    }

    private void Update()
    {
        if(objectsQueue.Count > 0 || currentPrefab != null || reticle != null)
        {
            RaycastHit [] hits;
            hits = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, 2f);
            ARPlane plane = hits.Where(hit => hit.transform.TryGetComponent(out ARPlane plane)).Select(hit => hit.transform.GetComponent<ARPlane>()).FirstOrDefault();
            if(plane != null && availablePlanes.Contains(plane))
            {
                RaycastHit hit = hits.Where(hit => hit.transform.TryGetComponent(out ARPlane plane)).FirstOrDefault();
                if(currentPlane != null && currentPlane != plane)
                {
                    currentPlane.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() {invisiblePlaneMaterial});
                }

                currentPlane = plane;
                currentPlane.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() {planeMaterial});
                if(headPlacementEnabled)
                {
                    if(currentPrefab != null)
                    {
                        Debug.Log("Updating current prefab position to " + hit.point);
                        currentPrefab.transform.SetPositionAndRotation(new Vector3(hit.point.x, currentPlane.center.y, hit.point.z), Quaternion.FromToRotation(currentPrefab.transform.up, currentPlane.normal));
                    }else if(reticle != null)
                    {
                        Debug.Log("Updating reticle position to " + hit.point);
                        reticle.transform.SetPositionAndRotation(new Vector3(hit.point.x, currentPlane.center.y, hit.point.z), Quaternion.FromToRotation(reticle.transform.up, currentPlane.normal));
                    }
                }
            }else
            {
                Debug.Log("No planes hit!");
            }
            
            // // Check if the raycast hits any AR planes in the availablePlanes list, sends ray forward from camera 6 feet or 2 meter
            // if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 2f))
            // {

            //     if(hit.transform.TryGetComponent(out ARPlane plane) && availablePlanes.Contains(plane)) //if plane is one we wish to place on, set reticle to hit point
            //     {
            //         if(currentPlane != null && currentPlane != plane)
            //         {
            //             currentPlane.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() {invisiblePlaneMaterial});
            //         }

            //         currentPlane = plane;
            //         currentPlane.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() {planeMaterial});
            //         if(headPlacementEnabled)
            //         {
            //             if(currentPrefab != null)
            //             {
            //                 Debug.Log("Updating current prefab position to " + hit.point);
            //                 currentPrefab.transform.SetPositionAndRotation(new Vector3(hit.point.x, currentPlane.center.y, hit.point.z), Quaternion.FromToRotation(currentPrefab.transform.up, currentPlane.normal));
            //             }else if(reticle != null)
            //             {
            //                 Debug.Log("Updating reticle position to " + hit.point);
            //                 reticle.transform.SetPositionAndRotation(new Vector3(hit.point.x, currentPlane.center.y, hit.point.z), Quaternion.FromToRotation(reticle.transform.up, currentPlane.normal));
            //             }
            //         }
            //     }else
            //     {
            //         Debug.Log("No plane hit!");
            //         Debug.Log("Hit this instead: " + hit.transform.gameObject.name);
            //     }
            // }
        }
    }

    public void SetPrefabs(List<GameObject> prefabs)
    {
        Debug.Log(prefabs.Count + " prefabs added to queue");
        if(reticle != null)
        {
            Destroy(reticle); //destroy default reticle if enabled
            reticle = null;
        }
        if(objectsQueue.Count == 0) //this only happens if the default reticle is enabled, ie head placement is enabled before objects are added,
                                                             //in this case we will set the current prefab to the first object in the list
        {
            //currentPrefab = Instantiate(prefabs[0], Vector3.zero, Quaternion.identity);
            currentPrefab = prefabs[0];
            currentPrefab.SetActive(true);
            if(prefabs.Count > 1)
            {
                for(int i = 1; i < prefabs.Count; i++)
                {
                    objectsQueue.Enqueue(prefabs[i]);
                }
            }
        }else
        {
            foreach(GameObject prefab in prefabs)
            {
                objectsQueue.Enqueue(prefab);
            }
        }
    }

    public void EnableHeadPlacement() //spawns default reticle
    {
        if(!headPlacementEnabled)
        {
            availablePlanes = ARPlaneViewController.instance.GetPlanesByClassification(planeClassifications);
            Debug.Log("Enabling head placement");
            headPlacementEnabled = true;
            if(currentPrefab == null && reticle == null)
            {
                Debug.Log("current prefab is null, spawning default reticle"); 
                reticle = Instantiate(defaultReticlePrefab, Vector3.zero, Quaternion.identity);
            }
            if(enableTapToPlace)
            {
                Debug.Log("Disabling tap to place, can only enable one at a time");
                DisableTapToPlace();
            }
        }else
        {
            Debug.Log("Head placement already enabled");
        }
    }

    public void DisableHeadPlacement()
    {
        headPlacementEnabled = false;
    }

    public void EnableTapToPlace() //enables finger placement and destroys current tips
    {
        if(!enableTapToPlace)
        {
            availablePlanes = ARPlaneViewController.instance.GetPlanesByClassification(planeClassifications);
            Debug.Log("Enabling tap to place");
            enableTapToPlace = true;
            if(currentPrefab == null && reticle == null)
            {
                Debug.Log("current prefab is null, spawning default reticle");
                reticle = Instantiate(defaultReticlePrefab, Vector3.zero, Quaternion.identity);
            }
            if(headPlacementEnabled)
            {
                Debug.Log("Disabling head placement, can only enable one at a time");
                DisableHeadPlacement();
            }
        }else
        {
            Debug.Log("Tap to place already enabled");
        }
    }

    public void DisableTapToPlace() //disables finger placement and destroys current tips
    {
        enableTapToPlace = false;
        if(leftTips.Values.Count > 0)
        {
            foreach(var jointTip in leftTips.Values)
            {
                Destroy(jointTip);
            }
            leftTips.Clear();
        }
        if(rightTips.Values.Count > 0)
        {
            foreach(var jointTip in rightTips.Values)
            {
                Destroy(jointTip);
            }
            rightTips.Clear();
        }
    }

    public void OnCollisionEntry(Vector3 position)
    {
        if(enableTapToPlace && currentPlane != null)
        {
            Debug.Log("Collision entry");
            if(currentPrefab != null)
            {
                currentPrefab.transform.SetPositionAndRotation(position, Quaternion.FromToRotation(currentPrefab.transform.up, currentPlane.normal));
            }else if(reticle != null)
            {
                reticle.transform.SetPositionAndRotation(position, Quaternion.FromToRotation(reticle.transform.up, currentPlane.normal));
            }
        }
    }

    private void LockObjectAndProgressQueue()
    {
        if(objectsQueue.Count > 0)
        {
            Debug.Log("Locked object, getting next object in queue");
            currentPrefab = Instantiate(objectsQueue.Dequeue(), currentPrefab.transform.position, currentPrefab.transform.rotation);
        }else
        {
            Debug.Log("No more objects to place, disabling tap to place and head placement");
            currentPrefab = null;
            DisableTapToPlace();
            DisableHeadPlacement();
            if(currentPlane != null)
            {
                currentPlane.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() {invisiblePlaneMaterial});
            }
        }
    }

    //if object is placed we want to either cycle to next object or if there are no objects left disable tap to place and head placement

    
    private bool DetectPinch(XRHandJoint middle, XRHandJoint thumb)
    {

        if (middle.trackingState != XRHandJointTrackingState.None &&
            thumb.trackingState != XRHandJointTrackingState.None)
        {
            Vector3 indexPOS = Vector3.zero;
            Vector3 thumbPOS = Vector3.zero;

            if (middle.TryGetPose(out Pose indexPose))
            {
                // adjust transform relative to the PolySpatial Camera transform
                indexPOS = Camera.main.transform.InverseTransformPoint(indexPose.position);
            }

            if (thumb.TryGetPose(out Pose thumbPose))
            {
                // adjust transform relative to the PolySpatial Camera adjustments
                thumbPOS = Camera.main.transform.InverseTransformPoint(thumbPose.position);
            }

            var pinchDistance = Vector3.Distance(indexPOS, thumbPOS);

            if (pinchDistance <= scaledThreshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }else
        {
            return false;
        }
    }

    // private void TestLockObjectAndProgressQueue()
    // {
    //     if(objectsQueue.Count > 0)
    //     {
    //         //currentPrefab = Instantiate(objectsQueue.Dequeue(), currentPrefab.transform.position, currentPrefab.transform.rotation);
    //         currentPrefab = objectsQueue.Dequeue();
    //         currentPrefab.SetActive(true);
    //     }else
    //     {
    //         Debug.Log("No more objects to place, disabling tap to place and head placement");
    //         currentPrefab = null;
    //         DisableTapToPlace();
    //         DisableHeadPlacement();
    //         if(currentPlane != null)
    //         {
    //             currentPlane.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() {invisiblePlaneMaterial});
    //         }
    //     }
    // }

    // private IEnumerator SimulatePinchAfterDelay(float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //     TestLockObjectAndProgressQueue();
    // }

    public void OnEnableHeadPlacement()
    {
        EnableHeadPlacement();
    }

    public void OnEnableTapToPlace()
    {
        EnableTapToPlace();
    }
    
}
