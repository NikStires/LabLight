using System;
using UniRx;
using UnityEngine;

/// <summary>
/// Based on ProjectionExample in HoloLensCameraStream
/// </summary>
public class VideoProjector : MonoBehaviour
{
    // Charuco conversion matrices
    private static Matrix4x4 invY = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
    private static Matrix4x4 invZ = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
    private static Matrix4x4 xRot = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));

    private int reservedId;
    private IDisposable videoFrameSubscription;

    private Texture2D _pictureTexture;
    private Renderer _pictureRenderer;

    // Use this for initialization
    void Start()
    {
        Debug.Log("VideoProjector started");

        _pictureRenderer = GetComponent<Renderer>() as Renderer;
        _pictureRenderer.material = new Material(Shader.Find("AR/LocatableCamera"));

        var camera = ServiceRegistry.GetService<IVideoCamera>();

        // Note: Callback can be on a background thread
        videoFrameSubscription = camera.GetFrames().Subscribe(frame =>
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() => UpdateTexture(frame), false);
        });

        camera.Running.Subscribe(running =>
        {
            if (!running && videoFrameSubscription != null)
            {
                Debug.Log("Shutdown camera");

                videoFrameSubscription.Dispose();
                videoFrameSubscription = null;
            }
        });
    }

    private void UpdateTexture(VideoFrame frame)
    {
        if (_pictureTexture == null)
        {
            _pictureTexture = new Texture2D(frame.width, frame.height, TextureFormat.BGRA32, false);
        }

        // Upload bytes to texture
        _pictureTexture.LoadRawTextureData(frame.image);
        _pictureTexture.wrapMode = TextureWrapMode.Clamp;
        _pictureTexture.Apply();

        // Set material parameters
        _pictureRenderer.sharedMaterial.SetTexture("_MainTex", _pictureTexture);

        if (frame.camera2WorldMatrix != null)
        { 
            Matrix4x4 camera2WorldMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(frame.camera2WorldMatrix);
            _pictureRenderer.sharedMaterial.SetMatrix("_WorldToCameraMatrix", camera2WorldMatrix.inverse);
        }

        if (frame.projectionMatrix != null)
        {
            Matrix4x4 projectionMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(frame.projectionMatrix);
            _pictureRenderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);
        }

        // RS
        // OpenXR LocatableCamera example
        // PhotoCaptureFrame has a different API
        //photoCaptureFrame.TryGetCameraToWorldMatrix(out Matrix4x4 cameraToWorldMatrix);

        //Vector3 position = cameraToWorldMatrix.GetColumn(3) - cameraToWorldMatrix.GetColumn(2);
        //Quaternion rotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));

        //photoCaptureFrame.TryGetProjectionMatrix(Camera.main.nearClipPlane, Camera.main.farClipPlane, out Matrix4x4 projectionMatrix);

        //quadRenderer.sharedMaterial.SetMatrix("_WorldToCameraMatrix", cameraToWorldMatrix.inverse);
        //quadRenderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);

        // quad.transform.position = position;
        // quad.transform.rotation = rotation;
    }
}


