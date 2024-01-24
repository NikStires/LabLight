using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager instance;

    public bool UseLocalFileSystem = true;

    //audio manager

    //email ?
    /*
     * private string mailUserName;
     * private string mailPassword;
     */
    private Transform workspaceTransform;

    public Transform WorkspaceTransform
    {
        get
        {
            return workspaceTransform;
        }
    }

    public bool loadProcedure = true;

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

        if (UseLocalFileSystem)
        {
            var resourceFileDataProvider = new ResourceFileDataProvider();

            //var aggregateFileDataProvider = new AggregateFileDataProvider(new List<IProcedureDataProvider> { resourceFileDataProvider, new LocalFileDataProvider() });
            ServiceRegistry.RegisterService<IProcedureDataProvider>(resourceFileDataProvider);

            ServiceRegistry.RegisterService<IMediaProvider>(resourceFileDataProvider);
            //ServiceRegistry.RegisterService<IWorkspaceProvider>(new StubbedWorkspaceProvider());

            var producer = new StubbedNetworkFrameProducer();
            //ServiceRegistry.RegisterService<ISharedStateController>(producer);
            //ServiceRegistry.RegisterService<INetworkFrameProducer>(producer);
        }//http file provider ?

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

        //SessionState.ShowWorkspaceOrigin(val =>
        //{
        //    Axes.SetValue(val);
        //}).AddTo(this);
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
