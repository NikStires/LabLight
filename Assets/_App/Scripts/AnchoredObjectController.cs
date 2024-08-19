using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// AnchoredObjectController is a script that initializes the payload of this GameObject
/// Using the trackableId of the anchor a lookup is performed to find the corresponding LabLight data that is attached to this anchor
/// </summary>
[RequireComponent(typeof(ARAnchor))]
public class AnchoredObjectController : MonoBehaviour
{
    public bool editMode = false;
    public bool debugMode = false;

    public GameObject[] editModeUI;
    public GameObject[] debugModeUI;

    public void OnEnable()
    {
        var anchor = this.GetComponent<ARAnchor>();

        // Find the LabLight data that corresponds to this anchor
        //ServiceRegistry.GetService<>()

        // User to lookup data anchor.trackableId and determine what part to activate/prefab to instantiate

        UpdateView();
    }

    private void UpdateView()
    {
        // Enable UI for editMode, delete button, grab interaction etc.
        foreach (var obj in editModeUI)
        {
            obj.SetActive(editMode);
        }

        // Enable UI for debugMode, show trackableId, show anchor position, show anchor rotation etc.
        foreach (var obj in debugModeUI)
        {
            obj.SetActive(debugMode);
        }
    }

    public void OnValidate()
    {
        UpdateView();
    }

    public void RemoveAnchoredObject()
    {
        Debug.Log("RemoveAnchoredObject");
    }
}
