using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniRx;

public class DetectionsStatusBehavior : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    private void OnEnable()
    {
        _text.text = SessionState.TrackedObjects.Count.ToString();
        SessionState.TrackedObjects.ObserveCountChanged().Subscribe(value =>  _text.text = value.ToString()).AddTo(this);
    }
}
