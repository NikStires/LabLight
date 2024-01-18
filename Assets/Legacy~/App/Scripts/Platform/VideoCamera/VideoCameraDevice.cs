using HoloLensCameraStream;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

/// <summary>
/// Uses the HoloLensCameraStream plugin to retrieve frames from the color camera
/// </summary>
public class VideoCameraDevice : IVideoCamera
{
    private HoloLensCameraStream.Resolution _resolution;
    private HoloLensCameraStream.VideoCapture _videoCapture;
    private IntPtr _spatialCoordinateSystemPtr;
    private byte[] _latestImageBytes;

    ReactiveProperty<bool> capturing = new ReactiveProperty<bool>();
    Subject<VideoFrame> frameSubject = new Subject<VideoFrame>();

    public FlipCode Flip
    {
        get
        {
            return FlipCode.None;
        }
    }

    public PixelFormat Format
    {
        get
        {
            return PixelFormat.BGRA;
        }
    }

    public IObservable<bool> Running
    {
        get
        {
            return capturing;
        }
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
        ServiceRegistry.Logger.Log("Starting capture");

        // Hololens 1
        // _spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

        // Hololens 2
        if (Microsoft.MixedReality.Toolkit.WindowsMixedReality.WindowsMixedRealityUtilities.UtilitiesProvider == null)
        {
            return;
        }

        _spatialCoordinateSystemPtr = Microsoft.MixedReality.Toolkit.WindowsMixedReality.WindowsMixedRealityUtilities.UtilitiesProvider.ISpatialCoordinateSystemPtr;

        VideoCapture.CreateAync(OnVideoCaptureCreated);
    }

    void StopCapture()
    {
        ServiceRegistry.Logger.Log("Stopping capture");

        if (!capturing.Value)
        {
            return;
        }

        capturing.Value = false;

        if (_videoCapture == null)
        {
            return;
        }

        _videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
        _videoCapture.StopVideoModeAsync(onVideoModeStopped);
    }

    void OnVideoCaptureCreated(HoloLensCameraStream.VideoCapture v)
    {
        if (v == null)
        {
            ServiceRegistry.Logger.LogError("Could not start video");
            return;
        }

        _videoCapture = v;

        // Request the spatial coordinate ptr if you want fetch the camera and set it if you need to 
        _videoCapture.WorldOriginPtr = _spatialCoordinateSystemPtr;

        var rez = _videoCapture.GetSupportedResolutions();
        rez.ToList().ForEach(r =>
        {
            ServiceRegistry.Logger.Log("Resolution: " + r.width + " / " + r.height);

            var rez2 = _videoCapture.GetSupportedFrameRatesForResolution(r);
            rez2.ToList().ForEach(z =>
            {
                ServiceRegistry.Logger.Log("FPS: " + z);
            });
        });

        // Get lowest resolution		
        _resolution = _videoCapture.GetSupportedResolutions().OrderBy((r) => r.width * r.height).FirstOrDefault();

        // Fastest framerates
        float frameRate = _videoCapture.GetSupportedFrameRatesForResolution(_resolution).OrderByDescending(r => r).FirstOrDefault();

        _videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;

        HoloLensCameraStream.CameraParameters cameraParams = new HoloLensCameraStream.CameraParameters();
        cameraParams.cameraResolutionWidth = _resolution.width;
        cameraParams.cameraResolutionHeight = _resolution.height;
        cameraParams.frameRate = Mathf.RoundToInt(frameRate);
        cameraParams.pixelFormat = HoloLensCameraStream.CapturePixelFormat.BGRA32;

        _videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
    }

    private void OnVideoModeStarted(VideoCaptureResult result)
    {
        if (!result.success)
        {
            ServiceRegistry.Logger.LogError("Could not start camera");
            return;
        }

        ServiceRegistry.Logger.Log("Camera started");
    }

    public static Matrix4x4 ConvertFloatArrayToMatrix4x4(float[] matrixAsArray)
    {
        Matrix4x4 m = new Matrix4x4();
        m.m00 = matrixAsArray[0];
        m.m01 = matrixAsArray[1];
        m.m02 = matrixAsArray[2];
        m.m03 = matrixAsArray[3];
        m.m10 = matrixAsArray[4];
        m.m11 = matrixAsArray[5];
        m.m12 = matrixAsArray[6];
        m.m13 = matrixAsArray[7];
        m.m20 = matrixAsArray[8];
        m.m21 = matrixAsArray[9];
        m.m22 = matrixAsArray[10];
        m.m23 = matrixAsArray[11];
        m.m30 = matrixAsArray[12];
        m.m31 = matrixAsArray[13];
        m.m32 = matrixAsArray[14];
        m.m33 = matrixAsArray[15];
        return m;
    }

    private void OnFrameSampleAcquired(VideoCaptureSample frame)
    {
        try
        {
            if (!frameSubject.HasObservers)
            {
                ServiceRegistry.Logger.Log("No more VideoCameraDevice frame observers");

                StopCapture();
                return;
            }

            if (_latestImageBytes == null || _latestImageBytes.Length < frame.dataLength) _latestImageBytes = new byte[frame.dataLength];

            float[] camera2WorldMatrixF;
            if (!frame.TryGetCameraToWorldMatrix(out camera2WorldMatrixF))
            {
                ServiceRegistry.Logger.LogError("Failed to get matrix");
                return;
            }

            Matrix4x4 camera2WorldMatrix = ConvertFloatArrayToMatrix4x4(camera2WorldMatrixF);
            camera2WorldMatrix = Matrix4x4.Transpose(camera2WorldMatrix);
            frame.CopyRawImageDataIntoBuffer(_latestImageBytes);

            var newFrame = new VideoFrame()
            {
                image = _latestImageBytes,
                camera2World = camera2WorldMatrix,
                intrinsics = CameraIntrinsics.from(frame.GetCameraIntrinsics()),
                width = _resolution.width,
                height = _resolution.height
            };

            // Get the cameraToWorldMatrix and projectionMatrix
            if (!frame.TryGetCameraToWorldMatrix(out newFrame.camera2WorldMatrix) || !frame.TryGetProjectionMatrix(out newFrame.projectionMatrix))
                return;

            frameSubject.OnNext(newFrame);

        }
        catch (Exception e)
        {
            ServiceRegistry.Logger.LogError("THROW: " + e.Message);
            ServiceRegistry.Logger.LogError(e.StackTrace);
            throw e;
        }
        finally
        {
            frame.Dispose();
        }
    }

    private void onVideoModeStopped(VideoCaptureResult result)
    {
        ServiceRegistry.Logger.Log("Close result " + result.hResult + " " + result.success);
        _videoCapture.Dispose();
    }
}
