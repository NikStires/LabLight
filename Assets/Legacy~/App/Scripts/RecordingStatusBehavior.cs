using UnityEngine;
using UniRx;
using TMPro;

public class RecordingStatusBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject _onIndicator = null;
    [SerializeField]
    private TextMeshProUGUI _text;

    private void Awake()
    {
        if (_onIndicator == null)
        {
            throw new MissingReferenceException("OnIndicator fields in RecordingStatusBehavior class cannot be null.");
        }
    }

    private void OnEnable()
    {
        // Update UI by subscribing to changes
        SessionState.Recording.Subscribe(value =>
        {
            UpdateVisualState();
        }).AddTo(this);

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        _onIndicator.SetActive(SessionState.Recording.Value);

        _text.color = SessionState.Recording.Value ? Color.white : new Color(0.04705882f, 0.2156863f, 0.2980392f);
    }
}
