using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the boundaries of the workspace
/// </summary>
public class WorkspaceFrame
{
    // All coordinates are in charuco frame in meters
    public List<Vector2> border;
    public Vector3 cameraPosition;  // RS Seems to be unused
}

