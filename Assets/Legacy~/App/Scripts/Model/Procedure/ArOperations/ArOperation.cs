using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public enum ArOperationType
{
    Highlight = 0,
    Animation = 1,
    Show = 2,
    Anchor = 3
}

/// <summary>
/// Abstract operation that describes an operation that needs to be performed an arView
/// </summary>
public abstract class ArOperation
{
    [HideInInspector]
    public ArOperationType arOperationType;

    [JsonProperty(IsReference = true)]
    [HideLabel]
    //[DisableInNonPrefabs]
    public ArDefinition arDefinition;

    public virtual string ListElementLabelName()
    {
        return "Operation";
    }

    public abstract void Apply(ArElementViewController arView);
}

