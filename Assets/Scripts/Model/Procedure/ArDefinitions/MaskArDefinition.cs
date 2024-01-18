using UnityEngine;

/// <summary>
/// Render line mask that is part of the tracked object 
/// 
/// Empty target, then apply to every tracked object
/// </summary>
public class MaskArDefinition : ArDefinition
{
    // Project lines to charuco place
    public bool projectToDeskPlane = false;
    
    // Use specified color instead of the color in the TrackedObject
    public bool overrideColor = false;

    // Color to use as overrideColor
    public Color color = Color.white;

    // Line thickness in millimeters
    public float lineWidthInMillimeters = 4;

    public MaskArDefinition()
    {
        arDefinitionType = ArDefinitionType.Mask;
    }

    public override string ListElementLabelName()
    {
        return "Mask AR Definition";
    }
}