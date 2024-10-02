using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class ProtocolManager : MonoBehaviour
{
    private Dictionary<ArDefinition, ArElementViewController> specificArViews = new Dictionary<ArDefinition, ArElementViewController>();

    [SerializeField] GameObject timerPrefab;

    private CheckItemDefinition previousCheckItem;

    private void Awake()
    {
        ProtocolState.LockingTriggered.Value = false;
        ProtocolState.AlignmentTriggered.Value = false;
        //ProtocolState.checklistStream.Subscribe(_ => OnCheckItemChange()).AddTo(this);
    }

    private void OnEnable()
    {
        ProtocolState.SetStartTime(DateTime.Now);
    }

    private void OnDisable()
    {
        ProtocolState.AlignmentTriggered.Value = false;
    }

    private void OnCheckItemChange()
    {
        if(ProtocolState.Steps[ProtocolState.Step].Checklist != null)
        {
            var currentCheckItem = ProtocolState.procedureDef.steps[ProtocolState.Step].checklist[ProtocolState.CheckItem];
            if(currentCheckItem.activateTimer && currentCheckItem != previousCheckItem)
            {
                var timer = Instantiate(timerPrefab, transform);
                previousCheckItem = currentCheckItem;
            }
        }
    }
}
