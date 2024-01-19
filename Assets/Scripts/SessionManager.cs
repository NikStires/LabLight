using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class SessionManager : Singleton<SessionManager>
{
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

    public void Awake()
    {
        if (UseLocalFileSystem)
        {
            var resourceFileDataProvider = new ResourceFileDataProvider();

            var aggregateFileDataProvider = new AggregateFileDataProvider(new List<IProcedureDataProvider> { resourceFileDataProvider, new LocalFileDataProvider() });
            ServiceRegistry.RegisterService<IProcedureDataProvider>(aggregateFileDataProvider);

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
        ProtocolState.SetProcedureDefinition(null);
        ProtocolState.ProcedureTitle = "";
        ProtocolState.Step = 0;

        //add grid?

        //SessionState.ShowWorkspaceOrigin(val =>
        //{
        //    Axes.SetValue(val);
        //}).AddTo(this);
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
