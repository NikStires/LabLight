using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager instance;

    //audio manager

    public bool loadProcedure = true;

    private Transform workspaceTransform;

    public Transform WorkspaceTransform
    {
        get
        {
            return workspaceTransform;
        }
    }

    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Multiple SessionManager instances detected. Destroying duplicate (newest).");
            DestroyImmediate(gameObject);
        }

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



    /* used for charuco calibration, reimplement with hand tracking
    public void Update()
    {
        //NetworkTick();
    }

    void NetworkTick()
    {

    }*/

    //add switch scene code
    /*
    private void GoToScene(procedure data)
    */
}
