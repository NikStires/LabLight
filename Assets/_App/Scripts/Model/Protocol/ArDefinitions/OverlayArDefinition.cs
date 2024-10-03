using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class OverlayArDefinition : ArDefinition
{
    public string image;
    public Color color;

    public OverlayArDefinition()
    {
        arDefinitionType = ArDefinitionType.Overlay;
//        frame = Frame.Charuco;
    }

    public override string ListElementLabelName()
    {
        return "Overlay AR Definition";
    }
}