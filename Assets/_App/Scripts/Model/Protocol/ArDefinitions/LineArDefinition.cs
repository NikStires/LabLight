using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class LineArDefinition : ArDefinition
{
    public string from;
    public string to;
    public Color color;

    public LineArDefinition()
    {
        arDefinitionType = ArDefinitionType.Line;
    }

    public override string ListElementLabelName()
    {
        return "Line AR Definition";
    }
}
