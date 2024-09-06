using System;
using System.Collections.Generic;
using UnityEngine;

public enum PayloadType
{
    Note = 1,
    HazardZone = 2
}

public abstract class AnchorPayload
{
    public PayloadType payloadType;
}

/// <summary>
/// AR Anchor that will be restored by ARFoundation
/// Flexible payload
/// </summary>
[Serializable]
public class Anchor
{
    public string id;
    public AnchorPayload payload;
}

/// <summary>
/// Anchored text note
/// </summary>
[Serializable]
public class SpatialNotePayload : AnchorPayload
{
    public string text;

    public SpatialNotePayload()
    {
        payloadType = PayloadType.Note;
    }
}

/// <summary>
/// Anchored text note
/// </summary>
[Serializable]
public class HazardZonePayload : AnchorPayload
{
    // TODO hazardzone data to save

    public HazardZonePayload()
    {
        payloadType = PayloadType.HazardZone;
    }
}

/// <summary>
/// 
/// </summary>
[Serializable]
public class AnchorData
{
    /// Version to help identify differences in file formats
    public int version;

    public List<Anchor> anchors = new List<Anchor>();
}