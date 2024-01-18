using System;
using UniRx;
using UnityEngine;

/// <summary>
/// Video camera that loads a texture and returns that as it's videoframe
/// </summary>
public class PreloadedImageVideoCamera : IVideoCamera
{
    string preloadedImagePath = "scene";
    private ReactiveProperty<bool> running = new ReactiveProperty<bool>();

    // Positions the scene for use in the editor (when this camera is used)
    private Vector3 cameraOffset = new Vector3(-0.1f, -0.4f, 0.2f);

    // Videoframe delay in milliseconds
    private int delayInMilliseconds = 200;

    public PreloadedImageVideoCamera(string path)
    {
        this.preloadedImagePath = path;
        running.Value = true;
    }

    public FlipCode Flip
    {
        get { return FlipCode.Vertical; }
    }

    public PixelFormat Format
    {
        get { return PixelFormat.RGBA; }
    }

    public IObservable<bool> Running
    {
        get { return running; }
    }

    public IObservable<VideoFrame> GetFrames()
    {
        var tex = Resources.Load<Texture2D>(preloadedImagePath);

        if (tex == null)
        {
            Debug.LogError("Could not load image from path: " + preloadedImagePath);
            return null;
        }

        var data = tex.GetPixels32();

        var bytes = Conversion.ToByteArray(data);

        var intrinsics = new CameraIntrinsics();

        // Hardcoded intrinsics
        intrinsics.resolution = new Vector2Int(1280, 720);
        intrinsics.focalLength = new Vector2(931.3604125976563f, 931.108642578125f);
        intrinsics.principalPoint = new Vector2(624.2255859375f, 350.7365417480469f);
        intrinsics.radialDistortion = Vector2.zero;
        intrinsics.tangentialDistortion = Vector2.zero;

        var sub = new Subject<VideoFrame>();

        Action send = () =>
        {
            sub.OnNext(new VideoFrame()
            {
                image = bytes,
                camera2World = Matrix4x4.TRS(cameraOffset, Quaternion.identity, Vector3.one),
                intrinsics = intrinsics
            });
        };

        Debug.Log("Seeing in 2 seconds");
        Observable.Timer(TimeSpan.FromMilliseconds(delayInMilliseconds)).Subscribe(_ => send());
        Observable.Timer(TimeSpan.FromMilliseconds(delayInMilliseconds)).Subscribe(_ => send());
        Observable.Timer(TimeSpan.FromMilliseconds(delayInMilliseconds)).Subscribe(_ => send());

        // Shutting down this fake camera
        Observable.Timer(TimeSpan.FromMilliseconds(delayInMilliseconds)).Subscribe(_ => running.Value = false);

        return sub;
    }
}