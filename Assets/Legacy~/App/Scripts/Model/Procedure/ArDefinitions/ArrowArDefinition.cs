using Sirenix.OdinInspector;
using UnityEngine;

public class ArrowArDefinition : ArDefinition
{
    public Vector3 position;

    public Quaternion rotation;

    public float radius;

    public float angleInDegrees;

    public ArrowArDefinition()
    {
        arDefinitionType = ArDefinitionType.Arrow;
        angleInDegrees = 180;
    }

    public override string ListElementLabelName()
    {
        return "Arrow AR Definition";
    }
}
