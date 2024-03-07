using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Lighthouse.MessagePack;

public class SessionManager : MonoBehaviour
{
    public static SessionManager instance;

    public ARAnchorManager anchorManager;

    public ARPlaneManager planeManager;

    //audio manager

    public bool loadProcedure = true;

    private static Transform workspaceTransform;

    public Transform WorkspaceTransform
    {
        set
        {
            if(workspaceTransform != value)
            {
                workspaceTransform = value;
            }
        }
        get
        {
            return workspaceTransform;
        }
    }

    //for testing AM
    public List<TrackedObject> TrackedObjectsDebug = new List<TrackedObject>();

    [SerializeField]
    private static Transform charucoTransform;
    public Transform CharucoTransform
    {
        set
        {
            if(charucoTransform != value)
            {
                charucoTransform = value;
            }
        }
        get
        {
            return charucoTransform;
        }
    }

    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple SessionManager instances detected. Destroying duplicate (newest).");
            DestroyImmediate(gameObject);
        }

        anchorManager = this.transform.parent.GetComponent<ARAnchorManager>();
        planeManager = this.transform.parent.GetComponent<ARPlaneManager>();

        //anchorManager.enabled = false;
        //planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;

        var resourceFileDataProvider = new ResourceFileDataProvider();

        ServiceRegistry.RegisterService<IProcedureDataProvider>(resourceFileDataProvider);

        ServiceRegistry.RegisterService<IMediaProvider>(resourceFileDataProvider);


        //Set up default state
        SessionState.deviceId = SystemInfo.deviceName;
        SessionState.Connected = false;

        //for debug to remove AM
        charucoTransform = this.transform;
        workspaceTransform = this.transform;

        //var producer = new StubbedNetworkFrameProducer();
        //ServiceRegistry.RegisterService<ISharedStateController>(producer);
        //ServiceRegistry.RegisterService<INetworkFrameProducer>(producer);
        //http file provider ?

        //Setup logger
        /* Add service 
         * debug
         * voice controller
         * audio manager
         * well plate csv provider
         * file upload handler
         */

        // charucoTransform.position = new Vector3(0, 0, 0);
        // workspaceTransform.position = new Vector3(0, 0, 0);

        //add grid?
    }

    public void OnEnable()
    {
        if (loadProcedure)
        {
            LoadProcedure();
        }
    }

    public void LoadProcedure()
    {
        var procedureDataProvider = ServiceRegistry.GetService<IProcedureDataProvider>();
        if(procedureDataProvider != null)
        {
            procedureDataProvider.GetOrCreateProcedureDefinition("piplight_H551").Subscribe(procedure => 
            {
                Debug.Log(procedure.title + " loaded");
                ProtocolState.SetProcedureDefinition(procedure);
            }, (e) =>
            {
                Debug.Log("Error fetching procedure");
            });
        }
        else
        {
            Debug.Log("Procedure Data provider null");
        }

    }

    public void Update()
    {
        TrackedObjectsDebug = SessionState.TrackedObjects.ToList();
    }

    /* used for charuco calibration, reimplement with hand tracking
    public void Update()
    {
        //NetworkTick();
    }

    void NetworkTick()
    {

    }*/

    public void UpdateCalibration(Matrix4x4 pose)
    {
        if (CharucoTransform == null)
        {
            Debug.LogError("Missing CharucoTransform on SessionManager");
            return;
        }

        // Set the stage coordinate frame
        CharucoTransform.FromMatrix(pose);

        SessionState.onCalibrationUpdated.Invoke();

        // TODO: World lock the charuco transform

        //TODO: Show origin?
    }
}
