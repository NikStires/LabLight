using UnityEngine;
using UniRx;
using TMPro;

public class ConnectionStatusBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject _offIndicator = null;
    
    [SerializeField]
    private GameObject _onIndicator = null;

    [SerializeField]
    private TextMeshProUGUI text;

    private void Awake()
    {
        if (_offIndicator == null || _onIndicator == null)
        {
            throw new MissingReferenceException("OffIndicator and OnIndicator fields in ConnectionBehavior class cannot be null.");
        }
    }

    private void OnEnable()
    {
        // Update UI by subscribing to changes
        SessionState.connectedStream.Subscribe(value =>
        {
            UpdateVisualState();
        }).AddTo(this);

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        _offIndicator.SetActive(!SessionState.Connected);
        _onIndicator.SetActive(SessionState.Connected);

        text.text = SessionState.Connected ? "Connected" : "Disconnected";
    }
}
