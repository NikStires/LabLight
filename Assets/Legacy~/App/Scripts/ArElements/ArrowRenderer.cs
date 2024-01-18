using System.Collections.Generic;
using UnityEngine;

public class ArrowRenderer : ArElementViewController
{
    public float stopDistance = 0.01f;
    public float LineWidth = .025f;
    public Vector3 p0;
    public Vector3 p3;
     
    public GameObject arrowHead;
    public Renderer arrow;
    LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);
    }

    public void SetColor(Color color)
    {
        arrow.material.color = GetComponent<Renderer>().material.color = color;
    }

    void Update()
    {
        var delta = p3 - p0;
        if (delta.magnitude == 0) return;

        var distanceToPercent = 1.0f / delta.magnitude;
        var end = Vector3.Lerp(p3, p0, stopDistance * distanceToPercent);

        lineRenderer.positionCount = 2;
        lineRenderer.widthCurve = new AnimationCurve(
          new Keyframe(0, LineWidth),
          new Keyframe(1, LineWidth)
        );

        lineRenderer.SetPositions(new Vector3[] { p0, end });

        arrowHead.transform.localPosition = p3;
        arrowHead.transform.localRotation = Quaternion.LookRotation(delta);
    }
}
