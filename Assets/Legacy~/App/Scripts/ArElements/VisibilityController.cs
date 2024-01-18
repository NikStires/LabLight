using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Behaviour that switches the specified object on if one of the fingertips is close enough
/// </summary>
public class VisibilityController : WorldPositionController
{
    [Tooltip("Gameobject to hide when index finger is within range")]
    public GameObject ObjectToHide;
    
    [Tooltip("Gameobject to show when index finger is within range")]
    public GameObject ObjectToShow;

    [Tooltip("Detection range")]
    public float TriggerDistance = .3f;

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);
    }

    private void OnDestroy()
    {
    }

    public override void Update()
    {
        base.Update();

        // Note HandJointInjector in the TrackedObjectServices GameObject must be enabled
        // It will add the HoloLens fingertips as trackedobjects

        if (TrackedObjects != null)
        {
            // Find fingers of interest
            var fingerTips = (from to in SessionState.TrackedObjects
                            where to.label == "LeftIndexTip" || to.label == "RightIndexTip"
                            select to); 

            // Determine if one of the fingers is close enough
            bool visible = false;
            if (fingerTips.Count() > 0)
            {
                foreach (var fingerTip in fingerTips)
                {
                    // Note, using XZ based distance
                    if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                        new Vector3(fingerTip.position.x, 0, fingerTip.position.z)) < TriggerDistance)
                    {
                        visible = true;
                    }
                }
            }

            if (ObjectToShow != null)
            {
                ObjectToShow.SetActive(visible);
            }

            if (ObjectToHide != null)
            {
                ObjectToHide.SetActive(!visible);
            }
        }
    }
}
