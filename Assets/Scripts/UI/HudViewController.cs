using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;

public class HudViewController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _hudText;
    [SerializeField] private GameObject _hudTextBG;
    [SerializeField] private MMFeedbacks _hudFeedbacks;
    [SerializeField] private HudEventSO _hudEventSO;

    void Start()
    {
        _hudEventSO.DisplayMessage.AddListener(DisplayHudMessage);
        _hudEventSO.DisplayWarning.AddListener(DisplayHudWarning);
        _hudEventSO.DisplayError.AddListener(DisplayHudError);
    }

    void DisplayHudMessage(string message)
    {
        if(_hudFeedbacks.HasFeedbackStillPlaying())
        {
            _hudFeedbacks.StopFeedbacks();
        }
        _hudText.color = Color.white;
        _hudText.text = message;
        _hudFeedbacks.PlayFeedbacks();
    }

    void DisplayHudWarning(string message)
    {
        if(_hudFeedbacks.HasFeedbackStillPlaying())
        {
            _hudFeedbacks.StopFeedbacks();
        }
        _hudText.color = Color.yellow;
        _hudText.text = message;
        _hudFeedbacks.PlayFeedbacks();
    }

    void DisplayHudError(string message)
    {
        if(_hudFeedbacks.HasFeedbackStillPlaying())
        {
            _hudFeedbacks.StopFeedbacks();
        }
        _hudText.color = Color.red;
        _hudText.text = message;
        _hudFeedbacks.PlayFeedbacks();
    }
}
