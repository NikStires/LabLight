using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ActionCoverViewController : MonoBehaviour
{
    public GameObject nextStepCover;
    public GameObject previousStepCover;
    public GameObject checkItemCover;
    public GameObject uncheckItemCover;
    public GameObject signOffCover;
    public GameObject closeCover;

    private bool firstRender = true;

    // Start is called before the first frame update
    void Awake()
    {
        ProtocolState.checklistStream.Subscribe(_ => OnCheckItemChanged()).AddTo(this);

        ProtocolState.stepStream.Subscribe(_ => OnStepChanged()).AddTo(this);

        ProtocolState.LockingTriggered.Subscribe(val => OnLockingTriggered(val)).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        if(ProtocolState.procedureDef.Value != null && ProtocolState.Steps[ProtocolState.Step].Checklist != null && ProtocolState.Steps[ProtocolState.Step].SignedOff && ProtocolState.CheckItem == ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1)
        {
            signOffCover.SetActive(false);
            if(ProtocolState.Steps.Count - 1 == ProtocolState.Step)
            {
                closeCover.SetActive(true);
            }else
            {
                nextStepCover.SetActive(true);
            }
        }

        if(firstRender && ProtocolState.procedureDef.Value != null)
        {
            ClearCovers();
            if(ProtocolState.Steps[ProtocolState.Step].Checklist == null)
            {
                nextStepCover.SetActive(true);
            }else
            {
                checkItemCover.SetActive(true);
            }
            firstRender = false;
        }
    }

    private void OnCheckItemChanged()
    {
        if(!ProtocolState.LockingTriggered.Value)
        {
            if(ProtocolState.CheckItem == ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1 && ProtocolState.Steps[ProtocolState.Step].Checklist[ProtocolState.CheckItem].IsChecked.Value && !ProtocolState.Steps[ProtocolState.Step].SignedOff)
            {
                checkItemCover.SetActive(false);//disable check cover
                signOffCover.SetActive(true);//enable sign off cover
            }else if(ProtocolState.CheckItem <= ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1 && !ProtocolState.Steps[ProtocolState.Step].Checklist[ProtocolState.CheckItem].IsChecked.Value)
            {
                signOffCover.SetActive(false);
                checkItemCover.SetActive(true);
                uncheckItemCover.SetActive(true);
            }
            else if(ProtocolState.CheckItem == 1)
            {
                previousStepCover.SetActive(false);//disable previous step cover
                checkItemCover.SetActive(true);//enable check cover
                uncheckItemCover.SetActive(true);//enable uncheck cover
            }else if(ProtocolState.CheckItem == 0)
            {
                ClearCovers(); //disable any previous covers
                checkItemCover.SetActive(true);
                previousStepCover.SetActive(true);
            }else
            {
                checkItemCover.SetActive(true);
                uncheckItemCover.SetActive(true);
            }
        }
    }

    private void OnStepChanged()
    {
        ClearCovers();
        if(ProtocolState.Steps[ProtocolState.Step].Checklist == null)
        {
            previousStepCover.SetActive(true);
            nextStepCover.SetActive(true);
        }else if(ProtocolState.CheckItem == 0)
        {
            checkItemCover.SetActive(true);
            previousStepCover.SetActive(true);
        }else if(ProtocolState.CheckItem == ProtocolState.Steps[ProtocolState.Step].Checklist.Count() - 1)
        {
            uncheckItemCover.SetActive(true);
            if(ProtocolState.Steps[ProtocolState.Step].SignedOff)
            {
                nextStepCover.SetActive(true);
            }else
            {
                signOffCover.SetActive(true);
            }
        }
    }

    private void ClearCovers()
    {
        nextStepCover.SetActive(false);
        previousStepCover.SetActive(false);
        signOffCover.SetActive(false);
        checkItemCover.SetActive(false);
        uncheckItemCover.SetActive(false);
        closeCover.SetActive(false);
    }

    private void OnLockingTriggered(bool val) //enable or disable locking covers
    {
        if(val)
        {
            ClearCovers();
        }else
        {
            uncheckItemCover.SetActive(true);
            checkItemCover.SetActive(true);
        }
    }

}
