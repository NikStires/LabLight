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

    private Transform workspaceTransform;

    public Transform WorkspaceTransform
    {
        set
        {
            workspaceTransform = value;
        }
        get
        {
            return workspaceTransform;
        }
    }

    [SerializeField]
    private static Transform charucoTransform;
    public static Transform CharucoTransform
    {
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

        var resourceFileDataProvider = new ResourceFileDataProvider();

        ServiceRegistry.RegisterService<IProcedureDataProvider>(resourceFileDataProvider);

        ServiceRegistry.RegisterService<IMediaProvider>(resourceFileDataProvider);

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

        //Set up default state
        SessionState.deviceId = SystemInfo.deviceName;
        SessionState.Connected = false;

        //add grid?
    }



    /* used for charuco calibration, reimplement with hand tracking
    public void Update()
    {
        //NetworkTick();
    }

    void NetworkTick()
    {

    }*/

    public static void UpdateCalibration(Matrix4x4 pose)
    {
        if (CharucoTransform == null)
        {
            Debug.LogError("Missing CharucoTransform on SessionManager");
            return;
        }

        // Set the stage coordinate frame
        CharucoTransform.FromMatrix(pose);

        // TODO: World lock the charuco transform

        //TODO: Show origin?
    }
}
