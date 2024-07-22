using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Short procedure descriptor for listing available procedures
/// </summary>
[Serializable]
public class ProcedureDescriptor
{
    public string title;
    public string name;

    [Multiline(2)]
    public string description;
}