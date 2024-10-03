using UnityEngine;

/// <summary>
/// Bounding box based on the provided bound coordinates that is part of the tracked object 
/// 
/// Empty target, then apply to every tracked object
/// </summary>
public class BoundingBoxArDefinition : ArDefinition
{
    public BoundingBoxArDefinition()
    {
        arDefinitionType = ArDefinitionType.BoundingBox;
    }

    public override string ListElementLabelName()
    {
        return "BoundingBox AR Definition";
    }
}