using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArrowElementViewController : WorldPositionController
{
    [SerializeField]
    private LineRenderer headLineRenderer;
    private LineRenderer lineRenderer;

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);

        // If not targeted (* or specific id) use target based positioning
        if (this.arDefinition.condition == null)
        {
            transform.localPosition = ((ArrowArDefinition)arDefinition).position;
            transform.localRotation = ((ArrowArDefinition)arDefinition).rotation;
        }

        lineRenderer = GetComponent<LineRenderer>();

        float radius = ((ArrowArDefinition)arDefinition).radius;
        if (radius < float.Epsilon)
        {
            // Straight
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, new Vector3(0, .1f, 0));
        }
        else
        {
            // Circle section
            float angleInRadians = ((ArrowArDefinition)arDefinition).angleInDegrees * Mathf.Deg2Rad;
            lineRenderer.positionCount = 16;
            float increment = angleInRadians / (lineRenderer.positionCount - 1);

            Vector3 lastPos = Vector3.zero;
            for (int i = 0; i < 16; i++)
            {
                lastPos = new Vector3(radius * Mathf.Cos(i * increment), 0f, radius * Mathf.Sin(i * increment));
                lineRenderer.SetPosition(i, lastPos);
            }

            headLineRenderer.SetPosition(0, lastPos + new Vector3(-0.25f * radius * Mathf.Cos(angleInRadians) + 0.25f * radius * Mathf.Sin(angleInRadians), 0, -0.25f * radius * Mathf.Sin(angleInRadians) - 0.25f * radius * Mathf.Cos(angleInRadians)));
            //headLineRenderer.SetPosition(0, lastPos + new Vector3(-0.25f * radius * Mathf.Cos(angleInRadians), 0, -0.25f * radius * Mathf.Sin(angleInRadians)));
            headLineRenderer.SetPosition(1, lastPos);
            headLineRenderer.SetPosition(2, lastPos + new Vector3(0.25f * radius * Mathf.Cos(angleInRadians) + 0.25f * radius * Mathf.Sin(angleInRadians), 0, 0.25f * radius * Mathf.Sin(angleInRadians) - 0.25f * radius * Mathf.Cos(angleInRadians)));
            //headLineRenderer.SetPosition(2, lastPos + new Vector3(0.25f * radius * Mathf.Cos(angleInRadians) , 0, 0.25f * radius * Mathf.Sin(angleInRadians) ));
        }
    }


    public override void Update()
    {
        // Use smooth positioning only when targeted (* or specific id)
        if (this.arDefinition.condition != null)
        {
            base.Update();
        }
    }
}
