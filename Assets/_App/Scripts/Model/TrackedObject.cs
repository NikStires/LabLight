using System;
using UnityEngine;

/// <summary>
/// A tracked object coming from ACAM, previously known as ObjectFrame
/// </summary>
[System.Serializable]
public class TrackedObject
{
    public int id;
    public string label;
    public float angle;
    public Vector3 scale;
    public Vector3 position;
    public Quaternion rotation;
    public static int IdCount = 1;

    // RS Added based on what is provided by ACAM2
    public int classId;
    public Vector3[] mask;
    public DateTime lastUpdate;
    public Color color;
    public Vector3[] bounds;
}

