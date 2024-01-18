using ACAM2.MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public enum Mode { Master, Isolated, Observer }

/// <summary>
/// Central state containing navigation stack, observable values, ProcedureDefinition
/// </summary>
public class SessionState
{
    public static string deviceId;
    public static WorkspaceFrame workspace;
    public static float lastFrameTime;
    private static bool _connected;
    private static Mode mode;

    
    public static ReactiveProperty<bool> enableGenericVisualizations = new ReactiveProperty<bool>();

    //app settings
    public static ReactiveProperty<bool> Recording = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> Timer = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowIntro = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> EnableHandMenu = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> MultiplePinLocking = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> LockStepControls = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowGrid = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowHandRays = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowCalibrationAxes = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> TextToSpeech = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ConfirmationPanelVisible = new ReactiveProperty<bool>();

    // Data streams typed bus where required
    public static Subject<Mode> modeStream = new Subject<Mode>();
    public static Subject<bool> connectedStream = new Subject<bool>();
    public static ReactiveProperty<ArucoSettings> ArucoSettings = new ReactiveProperty<ArucoSettings>();
    public static ReactiveCollection<TrackedObject> TrackedObjects = new ReactiveCollection<TrackedObject>();

    /// <summary>
    /// Keep track of the last settings that where last used for detection so we can detect if Lighthouse changed settings in the meantime 
    /// </summary>
    public static ArucoSettings LastUsedArucoSettings;
    
    /// <summary>
    /// Flag that indicates if lighthouse was calibrated with different Charuco settings than the last Charuco settings used on HoloLens
    /// </summary>
    public static ReactiveProperty<bool> CalibrationDirty = new ReactiveProperty<bool>();

    /// <summary>
    /// CSV file that is marked as available for download 
    /// </summary>
    public static ReactiveProperty<string> CsvFileDownloadable = new ReactiveProperty<string>();

    //wellplate settings
    public static ReactiveProperty<bool> ShowRowColIndicators = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowRowColIndicatorHighlight = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowRowColHighlights = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowInformationPanel = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowBB = new ReactiveProperty<bool>();
    //reactive properties for tFTubes
    public static ReactiveProperty<bool> ShowTubeContents = new ReactiveProperty<bool>();
    public static ReactiveProperty<bool> ShowTubeCaps = new ReactiveProperty<bool>();

    //setters
    public static bool Connected
    {
        set
        {
            if (_connected != value)
            {
                _connected = value;
                connectedStream.OnNext(value);
            }
        }
        get
        {
            return _connected;
        }
    }

    public static Mode RunningMode
    {
        set
        {
            if (mode != value)
            {
                mode = value;
                modeStream.OnNext(value);
            }
        }
        get
        {
            return mode;
        }
    }

}
