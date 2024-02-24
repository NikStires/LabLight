using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Lighthouse.MessagePack;

public enum Mode { Master, Isolated, Observer }

/// <summary>
/// Central state containing navigation stack, observable values, ProcedureDefinition
/// </summary>
public class SessionState : MonoBehaviour
{
    public static SessionState Instance;

    public ProcedureDefinition activeProtocol;
    public static string deviceId;
    public static WorkspaceFrame workspace;
    public static float lastFrameTime;
    public Vector3 mainPanelPosition;
    public Vector3 mainPanelRotation;

    private static bool _connected = false;
    private static bool _recording;
    private static Mode mode;

    
    public static ReactiveProperty<bool> enableGenericVisualizations = new ReactiveProperty<bool>();

    //app settings
    //public static ReactiveProperty<bool> Timer = new ReactiveProperty<bool>();
    //public static ReactiveProperty<bool> ShowGrid = new ReactiveProperty<bool>();
    //public static ReactiveProperty<bool> TextToSpeech = new ReactiveProperty<bool>();
    //public static ReactiveProperty<bool> ConfirmationPanelVisible = new ReactiveProperty<bool>();

    public static ReactiveProperty<bool> ShowWorkspaceOrigin = new ReactiveProperty<bool>();

    // Data streams typed bus where required
    public static Subject<bool> connectedStream = new Subject<bool>();
    public static Subject<bool> recordingStream = new Subject<bool>();
    public static ReactiveProperty<ArucoSettings> ArucoSettings = new ReactiveProperty<ArucoSettings>();
    public static ReactiveCollection<TrackedObject> TrackedObjects = new ReactiveCollection<TrackedObject>();
    
    /// <summary>
    /// Keep track of the last settings that where last used for detection so we can detect if Lighthouse changed settings in the meantime 
    /// </summary>
    public static ArucoSettings LastUsedArucoSettings;

    /// Flag that indicates if lighthouse was calibrated with different Charuco settings than the last Charuco settings used on HoloLens
    public static ReactiveProperty<bool> CalibrationDirty = new ReactiveProperty<bool>();

    /// CSV file that is marked as available for download 
    public static ReactiveProperty<string> CsvFileDownloadable = new ReactiveProperty<string>();

    //wellplate settings
    public static ReactiveProperty<bool> ShowRowColIndicators = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowRowColIndicatorHighlight = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowRowColHighlights = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowInformationPanel = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowMarker = new ReactiveProperty<bool>();
    //reactive properties for tFTubes
    public static ReactiveProperty<bool> ShowSourceContents = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowSourceTransform = new ReactiveProperty<bool>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple SessionState instances detected. Destroying duplicate (newest).");
            DestroyImmediate(gameObject);
        }
    }

    //setters
    public static bool Connected
    {
        set
        {
            if (_connected != value)
            {
                _connected = value;
                if(_connected)
                {
                    Debug.Log("requesting aruco settings");
                    ServiceRegistry.GetService<ILighthouseControl>()?.RequestArucoSettings();
                }
                connectedStream.OnNext(value);
            }
        }
        get
        {
            return _connected;
        }
    }

    public static bool Recording
    {
        set
        {
            if (_recording != value)
            {
                _recording = value;
                recordingStream.OnNext(value);
            }
        }
        get
        {
            return _recording;
        }
    }
}
