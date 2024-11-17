using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ArDefinitionType
{
    Line = 0,
    Outline = 1,
    Overlay = 2,
    Model = 3,              // Spatially positioned model
    Container = 4,           // Container with one or more content items
    Mask = 5,
    Arrow = 6,
    BoundingBox = 7
}

[Serializable]
public class ArObject
{
    // [JsonIgnore] 
    // public Condition placementCondition; AM TODO -> rework condition to get condition from prefab (view)

    [JsonProperty("specificObjectName")]
    public string specificObjectName { get; set; }


    [JsonProperty("arObjectID")]
    public string arObjectID { get; set; }

    [JsonProperty("rootPrefabName")]
    public string rootPrefabName { get; set; }
}   