using System;
using System.IO;
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
        //InitCSV();

    }

    private void OnDisable()
    {
        ProtocolState.AlignmentTriggered.Value = false;
    }

    /*
     * init csv to create csv file for current protocol session
     */
}
