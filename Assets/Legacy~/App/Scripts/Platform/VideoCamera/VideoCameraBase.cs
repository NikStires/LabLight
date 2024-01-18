using UnityEngine;
using System;

public enum PixelFormat
{
    RGBA = 0,
    BGRA
};

public enum FlipCode
{
    None = 0,
    Vertical,
    Horizontal,
    VerticalHorizontal
}

public class CameraIntrinsics
{
    public Vector2Int resolution;
    public Vector2 focalLength;
    public Vector2 principalPoint;
    public Vector3 radialDistortion;
    public Vector2 tangentialDistortion;

    public static CameraIntrinsics from(float[] v)
    {
        return new CameraIntrinsics()
        {
            resolution = new Vector2Int((int)v[0], (int)v[1]),
            focalLength = new Vector2(v[2], v[3]),
            principalPoint = new Vector2(v[4], v[5]),
            radialDistortion = new Vector3(v[6], v[7], v[8]),
            tangentialDistortion = new Vector2(v[9], v[10])
        };
    }
}

public class VideoFrame
{
    public byte[] image;
    public Matrix4x4 camera2World;
    public CameraIntrinsics intrinsics;
    public int width;
    public int height;

    public float[] camera2WorldMatrix, projectionMatrix;
}