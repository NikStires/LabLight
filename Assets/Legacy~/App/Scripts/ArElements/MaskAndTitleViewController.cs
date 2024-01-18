using TMPro;
using UnityEngine;

/// <summary>
/// Mask renderer with an added label
/// </summary>
public class MaskAndTitleViewController : MaskViewController
{
    [SerializeField]
    private TextMeshPro _label;

    [SerializeField]
    private GameObject _positionMarker;

    public override void Update()
    {
        base.Update();

        if (TrackedObjects != null)
        {
            if (TrackedObjects.Count == 1)
            {
                _positionMarker.transform.localPosition = TrackedObjects[0].position;

                _label.text = TrackedObjects[0].label;
            }
        }
    }
}
