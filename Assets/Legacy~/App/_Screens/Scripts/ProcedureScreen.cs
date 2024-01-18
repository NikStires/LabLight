using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class ProcedureScreen : ScreenViewController
{
    private bool inTempScreen = false;
    [Tooltip("Indicates the special purpose use of editing procedures in editor")]
    [SerializeField]
    private bool ProcedureEditorMode = false;
    IAudio audioPlayer;
    Action disposeVoice;

    /// <summary>
    /// Dictionary that maps the ArDefinition with the corresponding view instance
    /// </summary>
    private Dictionary<ArDefinition, ArElementViewController> specificArViews = new Dictionary<ArDefinition, ArElementViewController>();

    private void Awake()
    {
        ProtocolState.LockingTriggered.Value = false;
        ProtocolState.AlignmentTriggered.Value = false;
        SetupVoiceCommands();
    }

    private void OnEnable()
    {
        ProtocolState.SetStartTime(DateTime.Now);
        InitCSV();

        if (ProcedureEditorMode)
        {
            var fileDataProvider = new ResourceFileDataProvider();
            ServiceRegistry.RegisterService<IProcedureDataProvider>(fileDataProvider);
            ServiceRegistry.RegisterService<IMediaProvider>(fileDataProvider);
        }
        else
        {
            inTempScreen = false;
            modeChanged(SessionState.RunningMode);

            SessionState.modeStream.Subscribe(modeChanged).AddTo(this);
        }
    }

    private void OnDisable()
    {
        if(!inTempScreen)
        {
            ProtocolState.AlignmentTriggered.Value = false;
        }
        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    private void modeChanged(Mode mode)
    {
        var noProcedure = String.IsNullOrEmpty(ProtocolState.ProcedureTitle);

        if (mode != Mode.Observer && noProcedure)
        {
            //ServiceRegistry.Logger.Log("On running screen with no procedure as non-slave. Moving to procedure selection");
            if (SessionManager.Instance != null)
            {
                //  SessionManager.Instance.GoBack();
            }
        }
    }

    private void SetupVoiceCommands()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;

        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(new Dictionary<string, Action>()
        {
            { "start documenting", () =>
                {
                    audioPlayer.Play(AudioEventEnum.StartRecording);
                    ServiceRegistry.GetService<ILighthouseControl>()?.StartRecordingVideo();
                }
            },
            { "start documentation", () =>
                {
                    audioPlayer.Play(AudioEventEnum.StartRecording);
                    ServiceRegistry.GetService<ILighthouseControl>()?.StartRecordingVideo();
                }
            },
            { "document this", () =>
                {
                    audioPlayer.Play(AudioEventEnum.StartRecording);
                    ServiceRegistry.GetService<ILighthouseControl>()?.StartRecordingVideo();
                }
            },
            { "stop documentation", () =>
                {
                    audioPlayer.Play(AudioEventEnum.StopRecording);
                    ServiceRegistry.GetService<ILighthouseControl>()?.StopRecordingVideo();
                }
            },
            { "stop documenting", () =>
                {
                    audioPlayer.Play(AudioEventEnum.StopRecording);
                    ServiceRegistry.GetService<ILighthouseControl>()?.StopRecordingVideo();
                }
            },
            { "replay", () =>
                {
                    audioPlayer.Play(AudioEventEnum.StartReplay);
                    ServiceRegistry.GetService<ILighthouseControl>()?.StartPlayingVideo();
                }
            },
            { "stop replay", () =>
                {
                    audioPlayer.Play(AudioEventEnum.StopReplay);
                    ServiceRegistry.GetService<ILighthouseControl>()?.StopPlayingVideo();
                }
            },
            { "instant replay", () =>
                {
                    audioPlayer.Play(AudioEventEnum.StartReplay);
                    ServiceRegistry.GetService<ILighthouseControl>()?.StartPlayingVideo();
                }
            },
            { "replay 30 seconds", () =>
                {
                    audioPlayer.Play(AudioEventEnum.StartReplay);
                    ServiceRegistry.GetService<ILighthouseControl>()?.StartPlayingVideo();
                }
            },
        });
    }

    public void GoBack() 
    {
        if (!ProcedureEditorMode)
        {
            ProtocolState.AlignmentTriggered.Value = false;

            // Find image files recorded during this procedure and upload them
            ServiceRegistry.GetService<IFileUploadHandler>().UploadMediaFiles(ProtocolState.ProcedureTitle, ProtocolState.StartTime, DateTime.Now);

            if (!string.IsNullOrEmpty(ProtocolState.CsvPath))
            {
                Debug.Log("uploading csv file");
                ServiceRegistry.GetService<IFileUploadHandler>().UploadFile(ProtocolState.CsvPath);
            }
            ServiceRegistry.GetService<ILighthouseControl>()?.ResetTrackedObjects();
            ServiceRegistry.GetService<ILighthouseControl>()?.ResetProtcolStatus();
            SessionManager.Instance.GoBack();
        }
    }

    /// <summary>
    /// Creates CSV file for checklist data
    /// </summary>
    /// <param name="filename"></param>
    void InitCSV()
    {
        ProtocolState.SetCsvPath(Application.dataPath + "/" + ProtocolState.ProcedureTitle + "_" + ProtocolState.StartTime.ToString("yyyyMMddHHmmss") + ".csv");
        Debug.Log("CSV initalized at Path: " + ProtocolState.CsvPath);
        var tw = new StreamWriter(ProtocolState.CsvPath, false);
        tw.WriteLine("Step, Status, Time");
        tw.Close();
    }
}
