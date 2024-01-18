using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

/// <summary>
/// Defines an outline volume color, borderColor and borderWidth
/// </summary>
public class OutlineArDefinition : ArDefinition
{
    public Color color;
    public Color borderColor;
    public float borderWidth;

    public OutlineArDefinition()
    {
        arDefinitionType = ArDefinitionType.Outline;
    }

    public override string ListElementLabelName()
    {
        return "Outline AR Definition";
    }
}