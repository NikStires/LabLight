using OpenCVForUnity.UnityUtils;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using ACAM2.MessagePack;

/// <summary>
/// Sets up the main services
/// </summary>
public class SessionManager : Singleton<SessionManager>
{
    [Tooltip("Webcam camera to use in editor. Will fallback to PreloadedImageVideoCamera when null")]
    public WebCamTexureVideoCamera webCamVideoCamera;

    [SerializeField]
    [Tooltip("Load procedure and media from local filesystem")]
    private bool UseLocalFileSystem = true;

    [SerializeField]
    [Tooltip("Audio manager that maintains the mapping for audio events to their corresponding audioclip")]
    private AudioManager audioManager;

    [SerializeField]
    [Tooltip("Username for the mail service")]
    private string mailUserName;

    [SerializeField]
    [Tooltip("Password for the Mail service")]
    private string mailPassword;

    [SerializeField]
    [Tooltip("Screen to run as main screen")]
    private ScreenType MainScreen = ScreenType.Menu;

    [SerializeField]
    private Transform charucoTransform;
    public Transform CharucoTransform
    {
        get
        {
            return charucoTransform;
        }
    }

    public Transform stageTransform;
    public Workspace workspace;

    //DEBUG
    //public List<TrackedObject> TrackedObjects = new List<TrackedObject>();

    [SerializeField]
    private GameObject Axes;

    [SerializeField]
    [Tooltip("Screens that will be instantiated once they are needed")]
    private List<ScreenViewController> ScreenPrefabs;

    [HideInInspector]
    public ScreenViewController ActiveScreen;
    private ScreenViewController ReturnScreen;

    private void Awake()
    {
#if UNITY_EDITOR
        if (webCamVideoCamera)
        {
            ServiceRegistry.RegisterService<IVideoCamera>(webCamVideoCamera);
        }
        else
        {
            ServiceRegistry.RegisterService<IVideoCamera>(new PreloadedImageVideoCamera("scene"));
        }
#else
        ServiceRegistry.RegisterService<IVideoCamera>(new VideoCameraDevice());
        Debug.Log("*** Starting VideoCameraDevice");
#endif

        if (UseLocalFileSystem)
        {
            var resourceFileDataProvider = new ResourceFileDataProvider();

            var aggregateFileDataProvider = new AggregateFileDataProvider(new List<IProcedureDataProvider> { resourceFileDataProvider, new LocalFileDataProvider() });
            ServiceRegistry.RegisterService<IProcedureDataProvider>(aggregateFileDataProvider);

            ServiceRegistry.RegisterService<IMediaProvider>(resourceFileDataProvider);
            ServiceRegistry.RegisterService<IWorkspaceProvider>(new StubbedWorkspaceProvider());

            var producer = new StubbedNetworkFrameProducer();
            ServiceRegistry.RegisterService<ISharedStateController>(producer);
            ServiceRegistry.RegisterService<INetworkFrameProducer>(producer);
        }
        else
        {
            var http = new HttpImpl();
            var httpData = new HttpDataProvider(http);
            ServiceRegistry.RegisterService<IMediaProvider>(http);

            ServiceRegistry.RegisterService<IProcedureDataProvider>(httpData);
            ServiceRegistry.RegisterService<IWorkspaceProvider>(httpData);
            ServiceRegistry.RegisterService<INetworkFrameProducer>(new IasUdpClient(http));
        }

        // Setup logging
        var debugLogger = new DebugLogger();
#if !UNITY_EDITOR
			var fileLogger = new FileLogger(System.IO.Path.Combine(Application.persistentDataPath, "log.txt"));
			debugLogger.SetNext(fileLogger);
#endif
        ServiceRegistry.RegisterService<LoggerImpl>(debugLogger);
        ServiceRegistry.RegisterService<IVoiceController>(new VoiceController());
        ServiceRegistry.RegisterService<IAudio>(audioManager);
        ServiceRegistry.RegisterService<IMailService>(new SmtpMailService(new System.Net.NetworkCredential(mailUserName, mailPassword)));
        ServiceRegistry.RegisterService<IWellPlateCsvProvider>(new WellPlateCsvProvider());
        ServiceRegistry.RegisterService<IFileUploadHandler>(new FileUploadHandler());

        // Default state
        SessionState.deviceId = SystemInfo.deviceName;
        SessionState.RunningMode = Mode.Isolated;
        SessionState.Connected = false;
        ProtocolState.SetProcedureDefinition(null);
        ProtocolState.ProcedureTitle = "";
        ProtocolState.Step = 0;

        // RS Manually create a workspaceframe without going through a workspacescreen and fetching it from the stubbed
        var ws = new WorkspaceFrame();
        ws.border = new List<Vector2>() { new Vector2(-.79f, .255f), new Vector2(.79f, .255f), new Vector2(.79f, -.745f), new Vector2(-.79f, -.745f), };
        ws.cameraPosition = new Vector3(0, 1, 0);
        SessionState.workspace = ws;

        GotoScreen(ScreenType.Calibration);

        SessionState.ShowCalibrationAxes.Subscribe(value =>
        {
            Axes.SetActive(value);
        }).AddTo(this);
    }

    Mode modeFromMaster(string master, string deviceId)
    {
        if (string.IsNullOrEmpty(master)) return Mode.Isolated;
        if (master.Equals(deviceId)) return Mode.Master;
        return Mode.Observer;
    }

    private void Update()
    {
        //TrackedObjects = SessionState.TrackedObjects.ToList();      //DEBUG remove
        NetworkTick();
    }

    /// RS TODO bring this into a seperate service, running it should not be a responsibility of the SessionManager
    void NetworkTick()
    {
        var frames = ServiceRegistry.GetService<INetworkFrameProducer>().GetAndClearFrames();
        if (frames.Count > 0)
        {
            // Grab most recent frame
            var frame = frames[frames.Count - 1];

            // Position stage
            stageTransform.localPosition = frame.screen.position;
            if (frame.screen.lookForward != Vector3.zero)
            {
                stageTransform.localRotation = Quaternion.LookRotation(frame.screen.lookForward, Vector3.up);
            }

            // Determine operating mode from master string
            var priorMode = SessionState.RunningMode;
            Mode mode = modeFromMaster(frame.master, SessionState.deviceId);

            // Mode change
            if (priorMode != mode)
            {
                ServiceRegistry.Logger.Log("Mode changed: " + mode.ToString());

                SessionState.RunningMode = mode;

                if (mode == Mode.Master)
                {
                    // Sync values to server when becoming master
                    ServiceRegistry.Logger.Log("Became master - syncing values to server");

                    ProtocolState.SetProcedureTitle(ProtocolState.ProcedureTitle);
                    ProtocolState.SetStep(ProtocolState.Step);
                }
            }

            // Update procedure and step when slaved
            if (mode == Mode.Observer)
            {
                ProtocolState.ProcedureTitle = frame.procedure;
                ProtocolState.Step = frame.step;
            }
        }

        if (frames.Count > 0) SessionState.lastFrameTime = Time.time;
        var haveFrameInLastSecond = SessionState.lastFrameTime >= Time.time - 1;
        if (haveFrameInLastSecond != SessionState.Connected)
        {
            // jashley 11/15 hack to get quick session state working
            // will consult with Roland in morning
            //SessionState.Connected = haveFrameInLastSecond;
        }
    }

    private bool introShown = false;

    public void GoBack()
    {
        if (ReturnScreen != null)
        {
            ReturnToScreen();
        }
        else
        {
            // RS Todo Restore stack behaviour
            if (SessionState.ShowIntro.Value && !introShown)
            {
                introShown = true;

                GotoScreen(ScreenType.Intro);
            }
            else
            {
                ProtocolState.SetProcedureDefinition(null);
                GotoScreen(MainScreen);
            }
        }
    }

    private void ReturnToScreen()
    {
        Debug.Log("Return");

        if (ActiveScreen != null)
        {
            // RS Deactivate the screen to invoke onDisable before onEnable of new screen that is instantiated
            ActiveScreen.gameObject.SetActive(false);
            Destroy(ActiveScreen.gameObject);
        }

        ActiveScreen = ReturnScreen;
        ReturnScreen = null;
        ActiveScreen.gameObject.SetActive(true);
    }

    // RS Screens now instantiated prefabs and destroyed when no longer needed
    public void GotoScreen(ScreenType screenType, bool returnToScreen = false)
    {
        if (ActiveScreen != null && ActiveScreen.ScreenData.Type == screenType)
        {
            return;
        }

        var screenPrefab = ScreenPrefabs.FirstOrDefault(c => c.ScreenData.Type == screenType);

        if (screenPrefab == null)
        {
            Debug.LogError("No screen registered in SessionManager with screenType: " + screenType);
        }

        if (ActiveScreen != null)
        {
            // RS Deactivate the screen to invoke onDisable before onEnable of new screen that is instantiated
            ActiveScreen.gameObject.SetActive(false);

            if (returnToScreen)
            {
                ReturnScreen = ActiveScreen;
            }
            else
            { 
                Destroy(ActiveScreen.gameObject);
                ReturnScreen = null;
            }
        }

        Transform screenParent = null;
        switch (screenPrefab.ScreenData.Parent)
        {
            case ParentType.Root:
                screenParent = null;
                break;
            case ParentType.Stage:
                screenParent = stageTransform;
                break;
            case ParentType.Charuco:
                screenParent = CharucoTransform;
                break;
            default:
                Debug.LogError("Unknown ScreenPrefab ParentType");
                break;
        }

        ActiveScreen = Instantiate(screenPrefab, screenParent);

        if (workspace != null)
        {
            workspace.gameObject.SetActive(ActiveScreen.ScreenData.ShowGrid);
        }
        else
        {
            Debug.LogWarning("Assign the Workspace object to the SessionManager.");
        }

        audioManager?.Play(AudioEventEnum.EnterNewScreen);
    }

    public void UpdateCalibration(Matrix4x4 pose)
    {
        if (CharucoTransform == null)
        {
            Debug.LogError("Missing CharucoTransform on SessionManager");
            return;
        }

        // Set the stage coordinate frame
        ARUtils.SetTransformFromMatrix(CharucoTransform, ref pose);

        // Update the primary WLT spacepin to lock the world
        var spacePin = CharucoTransform.gameObject.GetComponent<SimpleSpacePinHandler>();
        if (spacePin)
        {
            spacePin.UpdateSpacePin();
        }
        else
        {
            Debug.LogError("Missing SimpleSpacePinHandler component on CharucoTransform");
        }

        // Trigger OnEnable so it can autohide
        Axes.SetActive(false);
        Axes.SetActive(SessionState.ShowCalibrationAxes.Value);
    }
}
