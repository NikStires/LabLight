using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ProtocolManager : MonoBehaviour
{
    private Dictionary<ArDefinition, ArElementViewController> specificArViews = new Dictionary<ArDefinition, ArElementViewController>();

    [SerializeField] GameObject timerPrefab;

    private CheckItemDefinition previousCheckItem;

    private void Awake()
    {
        ProtocolState.Instance.LockingTriggered.Value = false;
        ProtocolState.Instance.AlignmentTriggered.Value = false;
        ProtocolState.Instance.ChecklistStream.Subscribe(_ => OnCheckItemChange()).AddTo(this);
    }

    private void OnEnable()
    {
        ProtocolState.Instance.StartTime.Value = DateTime.Now;
    }

    private void OnDisable()
    {
        ProtocolState.Instance.AlignmentTriggered.Value = false;
    }

    private void OnCheckItemChange()
    {
        if (ProtocolState.Instance.HasCurrentChecklist())
        {
            var currentCheckItem = ProtocolState.Instance.CurrentCheckItemDefinition;
            if (currentCheckItem != null && currentCheckItem.activateTimer && currentCheckItem != previousCheckItem)
            {
                var timer = Instantiate(timerPrefab, transform);
                previousCheckItem = currentCheckItem;
            }
        }
    }
}
