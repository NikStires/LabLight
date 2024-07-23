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

    private void Awake()
    {
        ProtocolState.LockingTriggered.Value = false;
        ProtocolState.AlignmentTriggered.Value = false;
    }

    private void OnEnable()
    {
        ProtocolState.SetStartTime(DateTime.Now);
    }

    private void OnDisable()
    {
        ProtocolState.AlignmentTriggered.Value = false;
    }
}
