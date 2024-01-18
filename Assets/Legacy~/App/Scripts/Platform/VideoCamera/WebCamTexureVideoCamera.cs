using System;
using System.Linq;
using System.Runtime.InteropServices;
using UniRx;
using UnityEngine;

/// <summary>
/// IVideoCamera implementation that uses the Unity webcam to provide image frames.
/// Intrinsics are hardcoded so the results are not useful, but this camera will help you to run through the flow of the program when on desktop.
/// </summary>
public class WebCamTexureVideoCamera : MonoBehaviour, IVideoCamera
{
    private WebCamTexture webcamTexture;
    private Color32[] data;
    private ReactiveProperty<bool> capturing = new ReactiveProperty<bool>();
    private Subject<VideoFrame> frameSubject = new Subject<VideoFrame>();
    private CameraIntrinsics intrinsics = new CameraIntrinsics();

    public WebCamTexureVideoCamera()
    {
        capturing.Value = false;
    }

    public IObservable<VideoFrame> GetFrames()
    {
        if (!capturing.Value)
        {
            capturing.Value = true;
            StartCapture();
        }

        return frameSubject;
    }

    void StartCapture()
    {
        if (WebCamTexture.devices.Count() == 0)
        {
            ServiceRegistry.Logger.LogError("No cameras found");
            return;
        }

        var cameraName = WebCamTexture.devices.First().name;
        ServiceRegistry.Logger.Log("Using camera " + cameraName);

        webcamTexture = new WebCamTexture(cameraName, 960, 540, 30);
        webcamTexture.Play();

        // Hardcoded intrinsics
        intrinsics.resolution = new Vector2Int(webcamTexture.width, webcamTexture.height);
        intrinsics.focalLength = new Vector2(931.3604125976563f, 931.108642578125f);
        intrinsics.principalPoint = new Vector2(624.2255859375f, 350.7365417480469f);
        intrinsics.radialDistortion = Vector2.zero;
        intrinsics.tangentialDistortion = Vector2.zero;
        intrinsics.resolution = new Vector2Int(webcamTexture.width, webcamTexture.height);
    }

    void StopCapture()
    {
        webcamTexture.Stop();
        capturing.Value = false;
        data = null;
    }

    public FlipCode Flip
    {
        get
        {
            return FlipCode.Vertical;
        }
    }

    public PixelFormat Format
    {
        get
        {
            return PixelFormat.RGBA;
        }
    }

    public IObservable<bool> Running
    {
        get
        {
            return capturing;
        }
    }

    void Update()
    {
        if (webcamTexture == null) 
            return;
        if (!webcamTexture.isPlaying)
            return;
        if (!webcamTexture.didUpdateThisFrame) 
            return;

        if (data == null)
        {
            data = new Color32[webcamTexture.width * webcamTexture.height];
        }

        if (!frameSubject.HasObservers)
        {
            StopCapture();
            return;
        }

        webcamTexture.GetPixels32(data);

        

        frameSubject.OnNext(new VideoFrame()
        {
            image = Conversion.ToByteArray(data),
            camera2World = Matrix4x4.identity,
            intrinsics = intrinsics,
            width = webcamTexture.width,
            height = webcamTexture.height
        });
    }
}
