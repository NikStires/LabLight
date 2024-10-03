using System;
using System.Collections.Generic;
using UnityEngine;

public enum MediaDescriptorType
{
    Image = 1,
    Video = 2,
    Sound = 3,
    Prefab = 4
}

/// <summary>
/// Short media descriptor for listing available media items
/// </summary>
[Serializable]
public class MediaDescriptor
{
    public MediaDescriptorType type;
    public string path;
}