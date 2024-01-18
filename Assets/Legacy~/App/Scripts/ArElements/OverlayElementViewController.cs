using UnityEngine;

/// <summary>
/// Sets the color of the material to given color
/// </summary>
public class OverlayElementViewController : ArElementViewController
{
    public GameObject overlay;

    public void SetColor(Color color)
    {
        overlay.GetComponent<Renderer>().material.color = color;
    }

    private void Update()
    {
        // Update overlays (currently a billboarded sprite) based on the source ObjectFrames that they are attached to.

        // Position in the middle of the object (z is negative)
        if (TrackedObjects != null)
        {
            if (TrackedObjects.Count == 1)
            {
                transform.localPosition = TrackedObjects[0].position + new Vector3(0, TrackedObjects[0].scale.y / 2.0f, 0);
            }
            else
            {
                // TODO handle multiple objects
            }
        }
    }
}