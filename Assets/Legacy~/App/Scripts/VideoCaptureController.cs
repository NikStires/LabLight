using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Windows.WebCam;

/// <summary>
/// Voice controlled video capture
/// </summary>
public class VideoCaptureController : MonoBehaviour
{
    Action disposeVoice;
    VideoCapture videoCapture;

    private IAudio audioPlayer;

    private void Awake()
    {
        audioPlayer = ServiceRegistry.GetService<IAudio>();
    }

    private void OnEnable()
    {
        SetupVoiceCommands();
    }

    private void OnDisable()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;

        StopRecording();
    }

    private void SetupVoiceCommands()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;

        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(new Dictionary<string, Action>()
        {
            {"record", () =>
                {
                    StartStopRecording();
                }
            },
        });
    }

    void StartStopRecording()
    {
        if (videoCapture == null)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    void StopRecording()
    {
        if (videoCapture == null) return;

        audioPlayer.Play(AudioEventEnum.StopRecording);
        videoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
    }

    void StartRecording()
    {
        if (videoCapture != null) return;

        audioPlayer.Play(AudioEventEnum.StartRecording);

        Resolution cameraResolution = UnityEngine.Windows.WebCam.VideoCapture.SupportedResolutions
          .OrderByDescending((res) => res.width * res.height)
          .First();

        float cameraFramerate = UnityEngine.Windows.WebCam.VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();

        UnityEngine.Windows.WebCam.VideoCapture.CreateAsync(false, delegate (UnityEngine.Windows.WebCam.VideoCapture vc)
        {
            if (vc != null)
            {
                videoCapture = vc;

                UnityEngine.Windows.WebCam.CameraParameters cameraParameters = new UnityEngine.Windows.WebCam.CameraParameters();
                cameraParameters.hologramOpacity = 1.0f;
                cameraParameters.frameRate = cameraFramerate;
                cameraParameters.cameraResolutionWidth = cameraResolution.width;
                cameraParameters.cameraResolutionHeight = cameraResolution.height;
                cameraParameters.pixelFormat = UnityEngine.Windows.WebCam.CapturePixelFormat.BGRA32;

                videoCapture.StartVideoModeAsync(cameraParameters,
                                                       UnityEngine.Windows.WebCam.VideoCapture.AudioState.ApplicationAndMicAudio,
                                                       OnStartedVideoCaptureMode);
            }
            else
            {
                ServiceRegistry.Logger.LogError("Failed to create VideoCapture Instance!");
            }
        });
    }

    void OnStartedVideoCaptureMode(UnityEngine.Windows.WebCam.VideoCapture.VideoCaptureResult result)
    {
        if (result.success)
        {
            ServiceRegistry.Logger.Log("Started Video Capture Mode!");
            var timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
            var filename = string.Format("Protocol_{0}.mp4", timeStamp);
            var filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            filepath = filepath.Replace("/", @"\");
            videoCapture.StartRecordingAsync(filepath, OnStartedRecordingVideo);

        }
        else
        {
            ServiceRegistry.Logger.LogError("Failed to start Video Capture Mode");
        }          
    }

    void OnStoppedVideoCaptureMode(UnityEngine.Windows.WebCam.VideoCapture.VideoCaptureResult result)
    {
        if (result.success)
        {
            ServiceRegistry.Logger.Log("Stopped Video Capture Mode!");
            SessionState.Recording.Value = false;
        }
        else
        {
            ServiceRegistry.Logger.LogError("Failed to stop Video Capture Mode");
        }
    }

    void OnStartedRecordingVideo(UnityEngine.Windows.WebCam.VideoCapture.VideoCaptureResult result)
    {
        if (result.success)
        {
            ServiceRegistry.Logger.Log("Started Recording Video!");
            SessionState.Recording.Value = true;
        }
        else
        {
            ServiceRegistry.Logger.LogError("Failed to start Recording Video!");
        }        
    }

    void OnStoppedRecordingVideo(UnityEngine.Windows.WebCam.VideoCapture.VideoCaptureResult result)
    {
        if (result.success)
        {
            ServiceRegistry.Logger.Log("Stopped Recording Video!");
            videoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
        }
        else
        {
            ServiceRegistry.Logger.LogError("Failed to stop Recording Video.");
        }
    }
}