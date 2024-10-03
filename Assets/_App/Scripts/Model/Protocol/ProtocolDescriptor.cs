using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Short protocol descriptor for listing available protocols
/// </summary>
[Serializable]
public class ProtocolDescriptor
{
    public string title;
    public string name;

    [Multiline(2)]
    public string description;
}