using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

public class FootPedalController : MonoBehaviour
{
    public CheckListViewController checklist;
    public StepAppBar stepBar;
    public LockingDisplayController lockingDisplay;

    private float footPedalInterval = 0;
    private float lastFootPedalActionTime = 0;

    private static float PEDALDELAY;

    void Start()
    {
        if(Application.isEditor)
        {
            PEDALDELAY = 0f;
        }
        else
        {
            PEDALDELAY = 1.5f;
        }
    }

    // Airturn Duo 500 acts as a keyboard
    // https://manuals.plus/airturn/duo-500-bluetooth-pedal-controller-manual#ixzz87lTHS9j6
    //          Switch 1	    Switch 2	    Switch 3	    Switch 4	    Switch 5
    // Mode 1	AirDirect
    // Mode 2	Up Arrow        Left Arrow      Down Arrow      Right Arrow     Enter
    // Mode 3	Page Up         Left Arrow      Page Down       Right Arrow     Enter
    // Mode 4	Volume Up       Previous Track  Volume Down     Next Track      Play/Pause
    // Mode 5	Space           Left Click      Enter           Right Click     Home
    // Mode 6	3	            p               m               Shift +R        Space
    // Mode 7	Damper          Portamento      Sostenuto       Soft Pedal      Legato
    void Update()
    {
        footPedalInterval = Time.time;
        // mode 2 or 3
        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.PageUp)))
        {
            if(lastFootPedalActionTime == 0 || footPedalInterval - lastFootPedalActionTime > PEDALDELAY)
            {
                lastFootPedalActionTime = Time.time;
                PreviousProcedureAction();
            }
        }
        else if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.PageDown)))
        {
            if(lastFootPedalActionTime == 0 || footPedalInterval - lastFootPedalActionTime > PEDALDELAY)
            {
                lastFootPedalActionTime = Time.time;
                NextProcedureAction();
            }
        }
    }

    public void NextProcedureAction()
    {
        if(ProtocolState.LockingTriggered.Value)
        {
            lockingDisplay.NextAction();
        }
        else if(ProtocolState.AlignmentTriggered.Value)
        {
            ProtocolState.AlignmentTriggered.Value = false;
            AlignmentController.ResetModel(lockingDisplay.GetSpecificArViews().Where(arview => arview.Key.arDefinitionType == ArDefinitionType.Model).Select(x => x.Value).ToList());
        }
        else if(ProtocolState.Steps[ProtocolState.Step].Checklist != null)
        {
            if(ProtocolState.Steps[ProtocolState.Step].SignedOff)
            {
                stepBar.nextStep();
            }else if(ProtocolState.CheckItem == ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1 && ProtocolState.Steps[ProtocolState.Step].Checklist[ProtocolState.CheckItem].IsChecked.Value && !ProtocolState.Steps[ProtocolState.Step].SignedOff)
            {
                checklist.SignOff();
            }else
            {
                checklist.CheckItem();
            }
        }else 
        {
            stepBar.nextStep();
        }
    }

    public void PreviousProcedureAction()
    {
        if(!ProtocolState.LockingTriggered.Value)
        {
            if(ProtocolState.AlignmentTriggered.Value)
            {
                ProtocolState.AlignmentTriggered.Value = false;
                AlignmentController.ResetModel(lockingDisplay.GetSpecificArViews().Where(arview => arview.Key.arDefinitionType == ArDefinitionType.Model).Select(x => x.Value).ToList());
            }else
            {
                if(ProtocolState.CheckItem == 0 || ProtocolState.Steps[ProtocolState.Step].SignedOff)
                {
                    stepBar.previousStep();
                }else
                {
                    checklist.UnCheckItem();
                }
            }
        }else
        {
            lockingDisplay.PreviousAction();
        }
    }
}
