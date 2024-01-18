using UnityEngine;

/// <summary>
/// Box outline with configurable inner and outline color
/// </summary>
public class OutlineElementViewController : ArElementViewController
{
    public GameObject box;

    float edgeWidth;
    Material material;

    public void SetColor(Color color)
    {
        var rend = box.GetComponent<Renderer>();
        rend.material.color = color;
    }

    public void SetEdgeColor(Color color)
    {
        var rend = box.GetComponent<Renderer>();
        rend.material.SetColor("_HoverColorOverride", color);
        // rend.material.SetFloat("_BorderLightOpaqueAlpha", color.a);
    }

    public void SetEdgeWidth(float value)
    {
        edgeWidth = value;
    }

    void Update()
    {
        // Update view based on source model

        // Position, scale and rotate outlines (currently a box wireframe) based on the source ObjectFrames that they are attached to.
        if (TrackedObjects != null)
        {
            var trackedObject = TrackedObjects[0];

            transform.localPosition = trackedObject.position;
            transform.localScale = trackedObject.scale;
            transform.localRotation = trackedObject.rotation;
        }

        if (!material)
        {
            material = box.GetComponent<Renderer>().material;
        }

        // Determine smallest face
        float minLength = Mathf.Min(transform.localScale.x, transform.localScale.y, Mathf.Abs(transform.localScale.z));
        float relValue = Mathf.Min(1f, edgeWidth / minLength);

        // Border width is relative to the smallest face
        material.SetFloat("_BorderWidth", relValue);
    }
}