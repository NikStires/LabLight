using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

public class TitleBarViewController : MonoBehaviour
{
    public Transform HostBackground;
    public Renderer highlightRenderer;
    public Material HighlightMaterial;

    private ObjectManipulator objectManipulator;
    private Material DefaultMaterial;
    private float threshold = 0.03f;
    private bool grabbed;

    // Start is called before the first frame update
    void Start()
    {
        //resize title bar and its box collider to fit panel size
        transform.localScale = new Vector3 (HostBackground.localScale.x * 0.75f, transform.localScale.y, transform.localScale.z);
        GetComponent<BoxCollider>().size = new Vector3(transform.GetChild(0).localScale.x, GetComponent<BoxCollider>().size.y, GetComponent<BoxCollider>().size.z);

        DefaultMaterial = highlightRenderer.material;

        objectManipulator = GetComponent<ObjectManipulator>();
        objectManipulator.OnManipulationStarted.AddListener(_ => { grabbed = true; highlightRenderer.material = HighlightMaterial;});
        objectManipulator.OnManipulationEnded.AddListener(_ => grabbed = false);
    }

    // Update is called once per frame
    void Update()
    {
        if(!grabbed)
        {
            // Check if right hand is within the collider
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out MixedRealityPose RightTip))
            {
                CheckIfHandIsWithinCollider(RightTip.Position);
            }
            // Check if left hand is within the collider
            else if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Left, out MixedRealityPose LeftTip))
            {
                CheckIfHandIsWithinCollider(LeftTip.Position);
            }
        }
    }

    void CheckIfHandIsWithinCollider(Vector3 handPosition)
    {
        // Get the closest point on the collider to the hand
        Vector3 closestPoint = GetComponent<BoxCollider>().ClosestPoint(handPosition);

        // Check if the closest point is the same as the hand position
        if (Vector3.Distance(closestPoint, handPosition) < threshold)
        {
            highlightRenderer.material = HighlightMaterial;
        }
        else
        {
            highlightRenderer.material = DefaultMaterial;
        }
    }
}
