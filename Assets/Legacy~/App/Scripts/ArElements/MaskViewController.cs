using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Updates the lineRenderer in this prefab
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class MaskViewController : ArElementViewController
{
    private LineRenderer _lineRenderer;

    private float _lineWidth = 1;
    private Material _lineMaterial;

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);

        _lineWidth = 0.001f * ((MaskArDefinition)arDefinition).lineWidthInMillimeters;
        _lineRenderer = GetComponent<LineRenderer>();
        _lineMaterial = new Material(Shader.Find("Mixed Reality Toolkit/Standard"));
        if (((MaskArDefinition)arDefinition).overrideColor)
        {
            _lineMaterial.SetColor("_Color", ((MaskArDefinition)arDefinition).color);
        }
        _lineRenderer.material = _lineMaterial;
    }

    public virtual void Update()
    {
        // TODO Make reactive so it only runs when something changed in trackedobject
        if (TrackedObjects != null && TrackedObjects.Count == 1 && TrackedObjects[0].mask != null)
        {
            _lineRenderer.positionCount = TrackedObjects[0].mask.Length;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            _lineRenderer.loop = true;

            if (((MaskArDefinition)arDefinition).projectToDeskPlane)
            {
                var projectedMask = new Vector3[TrackedObjects[0].mask.Length];
                TrackedObjects[0].mask.CopyTo(projectedMask, 0);
                for (int i = 0; i < projectedMask.Length; i++)
                {
                    projectedMask[i].y = 0;
                }
                _lineRenderer.SetPositions(projectedMask);
            }
            else
            {
                _lineRenderer.SetPositions(TrackedObjects[0].mask);
           //     _lineRenderer.positionCount = trackedObject.Value.bounds.Length;
           //     _lineRenderer.SetPositions(trackedObject.Value.bounds);
            }

            if (!((MaskArDefinition)arDefinition).overrideColor)
            {
                _lineMaterial.SetColor("_Color", TrackedObjects[0].color);
            }
        }
    }
}
