using UnityEngine;
using UniRx;

/// <summary>
/// AnchorPayloadController is the abstract class that the AnchoredObjectController talks to
/// </summary>
public abstract class AnchorPayloadController : MonoBehaviour
{
    public bool debugMode = false;

    public GameObject[] editModeUI;
    public GameObject[] debugModeUI;

    protected AnchorPayload payload;

    private void Start()
    {
        SessionState.AnchoredObjectEditMode.Subscribe(val =>
        {
            UpdateView();
        }).AddTo(this);

        UpdateView();
    }

    public void Initialize(AnchorPayload payload)
    {
        this.payload = payload;
        UpdateView();
    }

    protected virtual void UpdateView()
    {
        // Enable UI for editMode, delete button, grab interaction etc.
        foreach (var obj in editModeUI)
        {
            obj.SetActive(SessionState.AnchoredObjectEditMode.Value);
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
}
