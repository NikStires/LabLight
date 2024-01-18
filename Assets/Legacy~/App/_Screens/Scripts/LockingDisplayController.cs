using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using TMPro;
using System.IO;
using UniRx;
using UnityEngine.UI;

public enum LockingAction
{
    None = 0, //locking is finished or not started
    Anchoring = 1, //if object is currently in anchoring set
    Aligning = 2, //anchoring step has been completed now aligning
    Awaiting = 3 //if anchoring and aligning are complete and we are awaiting another model to unlock
}

public class LockingDisplayController : MonoBehaviour
{
    public GameObject lockingPanel;
    public TextMeshProUGUI lockingText;
    public RectTransform lockingContentGrid;
    public GameObject modelStatusButtonPrefab;
    public GameObject expectedObjectsText;

    //buttons
    public Interactable FinishLockingButton;
    public Interactable LockButton;
    public Interactable UnlockButton;
    public Interactable AlignButton;
    public Interactable RealignButton;

    //TODO
    //enable locking when Tracked object detected for unlocked model
    //after 15-20 seconds if no tracked object is detected then ask user if they would like to manually place the model
    //initiate manual model placement
    private List<ArDefinition> allAnchorDefinitions = new List<ArDefinition>();
    private int currentAnchorDefinition = 0;
    private List<ArDefinition> currentAnchorDefinitions = new List<ArDefinition>();
    private Dictionary<ArDefinition, ArElementViewController> anchorViews = new Dictionary<ArDefinition, ArElementViewController>();
    private List<ArDefinition> defsInstantiated = new List<ArDefinition>();

    //current state
    private ArElementViewController currentView;
    private ArDefinition currentDef;
    private LockingAction currentLockingAction = LockingAction.None;
    private ModelStatusButtonViewController currentButton;

    private IAudio audioPlayer;
    Action disposeVoice;

    void OnEnable()
    {
        audioPlayer = ServiceRegistry.GetService<IAudio>();
        SetupVoiceCommands();
        SetupButtonEvents();
        //ServiceRegistry.GetService<ILighthouseControl>()?.Request_Deep_Models(); NS causes error and is unused so commenting out for now FIX LATER
        ProtocolState.LockingTriggered.Value = false;
    }

    void OnDisable()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
        ProtocolState.LockingTriggered.Value = false;
        ServiceRegistry.GetService<ILighthouseControl>()?.DetectorMode(4, 0, 10f);
    }

    private void InitializeLockingSession(List<ArDefinition> selectedDefinitions)
    {
        if(audioPlayer == null)
        {
           audioPlayer = ServiceRegistry.GetService<IAudio>();
        }

        if(ProtocolState.LockingTriggered.Value != true)
        {
            if(selectedDefinitions != null && selectedDefinitions.Count > 0)
            {
                foreach(var anchorDefinition in allAnchorDefinitions.Where(ar => !defsInstantiated.Contains(ar)))
                {
                    anchorViews[anchorDefinition].transform.gameObject.SetActive(true);
                    CreateModelStatusButton(anchorDefinition);
                }
                
                //activate panel and move it into view
                this.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.6f;
                this.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
                expectedObjectsText.SetActive(true);
                lockingPanel.SetActive(true);

                ProtocolState.LockingTriggered.Value = true;
                ServiceRegistry.GetService<ILighthouseControl>()?.DetectorMode(4, 1, 10f);
                currentAnchorDefinitions = selectedDefinitions;

                //select first button and associated view model
                currentButton = lockingContentGrid.GetChild(0).GetComponent<ModelStatusButtonViewController>();
                currentButton.ActivateBackplate();
                lockingContentGrid.GetChild(0).GetComponent<Interactable>().IsToggled = true;
                SetCurrentView(currentAnchorDefinitions[0]);

                if(AllModelsLocked())
                {
                    FinishLockingButton.IsEnabled = true;
                }
                else
                {
                    FinishLockingButton.IsEnabled = false;
                }
            }
            else
            {
                StartCoroutine(clearLockingDisplay("No objects selected"));
            }
        }
        else
        {
            Debug.LogWarning("Cannot trigger locking while in locking");
        }
    }

    private void CreateModelStatusButton(ArDefinition anchorDefinition)
    {
        var newButton = Instantiate(modelStatusButtonPrefab, lockingContentGrid.transform);
        var newButtonScript = newButton.transform.GetComponent<ModelStatusButtonViewController>();
        var newButtonInteractable = newButton.GetComponent<Interactable>();
        newButtonScript.InitButton(anchorDefinition, (ModelElementViewController)anchorViews[anchorDefinition]);
        
        newButtonInteractable.OnClick.AsObservable().Subscribe(_ => 
        {
            SetCurrentView(anchorDefinition);
            currentButton = newButtonScript;
            if(newButtonInteractable.IsToggled)
            {
                foreach(Transform button in lockingContentGrid)
                {
                    button.GetComponent<ModelStatusButtonViewController>().DeactivateBackplate();
                    button.GetComponent<Interactable>().IsToggled = false;
                }
                newButtonScript.ActivateBackplate();
            }
            else
            {
                newButtonScript.DeactivateBackplate();
            }
        }).AddTo(this);
        defsInstantiated.Add(anchorDefinition);
    }

    private void SetAnchor()
    {
        if(currentLockingAction == LockingAction.Anchoring && ((WorldPositionController)currentView).positionValid)
        {
            audioPlayer.Play(AudioEventEnum.SignOff);

            ((WorldPositionController)currentView).LockPosition();
            LockButton.gameObject.SetActive(false);
            UnlockButton.gameObject.SetActive(true);
            currentButton.ActivateLockIcon();

            if(currentView.TrackedObjects.Count > 0)
            {
                //SessionState.TrackedObjects.Remove(currentView.TrackedObjects[0]);
                currentView.TrackedObjects.Clear();
            }
            if(currentDef.arDefinitionType == ArDefinitionType.Model)
            {
                setLockingDisplayText(((AnchorCondition)currentDef.condition).filter + " Locked. Set alignment by saying 'Align'");
                AlignmentController.TriggerAlignment(currentView);
                currentLockingAction = LockingAction.Aligning;
            }
            if(AllModelsLocked())
            {
                FinishLockingButton.IsEnabled = true;
            }
            /*else. This is for when or if we reimplement world container view controllers, if not we can depend on the current model needing to be aligned
            {
                if(currentAnchorDefinitions.Count() < currentAnchorDefinition)
                {
                    string lockingString = "Anchor set for " + ((AnchorCondition)currentDef.condition).filter;
                    currentAnchorDefinition++;
                    currentDef = currentAnchorDefinitions[currentAnchorDefinition];
                    currentView = anchorViews[currentDef];
                    lockingString += (". Set anchor for " + ((AnchorCondition)currentDef.condition).filter + " by saying 'Anchor'");

                    ((WorldPositionController)currentView).UnlockPosition();
                    setLockingDisplayText(lockingString);
                    currentLockingAction = LockingAction.Anchoring; //anchoring activated for next model
                }else
                {
                    endLocking();
                }
            }*/
        }
    }

    //unlock position of model
    //add the tracked object that is not in the radius of another model
    private void ResetAnchor()
    {
        if(((WorldPositionController)currentView).positionLocked)
        {
            ((WorldPositionController)currentView).UnlockPosition();
            foreach(TrackedObject trackedObject in SessionState.TrackedObjects)
            {
                if(TrackedObjectAvailable(trackedObject))
                {
                    currentView.TrackedObjects.Add(trackedObject);
                }
                else
                {
                    Debug.Log("tracked object unavailable for selected model");
                }
            }
            audioPlayer.Play(AudioEventEnum.SignOff);
            UnlockButton.gameObject.SetActive(false);
            LockButton.gameObject.SetActive(true);
            currentButton.DeactivateLockIcon();

            if(currentLockingAction == LockingAction.Aligning)
            {
                SetAlignment();
            }
            currentLockingAction = LockingAction.Anchoring;
            setLockingDisplayText("Place " + ((AnchorCondition)currentDef.condition).filter + " on your workspace.");

            FinishLockingButton.IsEnabled = false;
        }
    }

    private bool TrackedObjectAvailable(TrackedObject to)
    {
        foreach(var anchorView in anchorViews)
        {
            ModelElementViewController modelController = (ModelElementViewController)anchorViews[anchorView.Key];
            if ((Vector3.Distance(to.position, modelController.transform.position) < (modelController.GetComponent<CapsuleCollider>().radius * 0.9f)) && modelController.positionLocked)
            {
                return false;
            }
        }
        return true;
    }

    private void SetAlignment()
    {
        if(currentLockingAction == LockingAction.Aligning)
        {
            AlignmentController.ResetModel(currentView);
            setLockingDisplayText("Alignment set for " + ((AnchorCondition)currentDef.condition).filter);
        }
        else
        {
            Debug.LogWarning("Cannot set alignment if not in alignment action");
        }
    }

    private void SetCurrentView(ArDefinition def)
    {
        if(anchorViews[def] != null)
        {
            if(currentLockingAction == LockingAction.Aligning)
            {
                SetAlignment();
            }

            if(currentView != null)
            {
                ((WorldPositionController)currentView).selectedForLocking = false;
            }

            currentDef = def;
            currentView = anchorViews[def];
            ((WorldPositionController)anchorViews[def]).selectedForLocking = true;

            if(((WorldPositionController)anchorViews[def]).positionLocked && ((WorldPositionController)anchorViews[def]).hasBeenLocked) //if the model is already locked
            {
                LockButton.gameObject.SetActive(false);
                UnlockButton.gameObject.SetActive(true);
                setLockingDisplayText(((AnchorCondition)currentDef.condition).filter + " is Locked. Say 'Unlock' to reposition this model");
            }
            else//activate anchoring
            {
                LockButton.gameObject.SetActive(true);
                UnlockButton.gameObject.SetActive(false);
                setLockingDisplayText("Place " + ((AnchorCondition)currentDef.condition).filter + " on your workspace.");
                ((WorldPositionController)currentView).UnlockPosition();

                currentLockingAction = LockingAction.Anchoring;
            }
            
            //invoke detection checks for this model
            CancelInvoke("CheckForDetections");
            InvokeRepeating("CheckForDetections", 20f, 1f);
        }
    }

    private void setLockingDisplayText(string text)
    {
        lockingText.text = text;
    }

    private void endLocking()
    {
        audioPlayer.Play(AudioEventEnum.CalibrationCompleted);
        if(currentLockingAction == LockingAction.Aligning)
        {
            SetAlignment();
        }
        currentButton.DeactivateBackplate();
        ProtocolState.LockingTriggered.Value = false;
        ServiceRegistry.GetService<ILighthouseControl>()?.DetectorMode(4, 0, 10f);
        currentLockingAction = LockingAction.None;

        StartCoroutine(clearLockingDisplay(""));
        expectedObjectsText.SetActive(false);
        lockingPanel.SetActive(false);
        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    IEnumerator clearLockingDisplay(string text)
    {
        lockingText.text = text;
        yield return new WaitForSeconds(2f);
        lockingText.text = "";
    }

    private bool AllModelsLocked()
    {
        foreach(var def in currentAnchorDefinitions)
        {
            if(!((WorldPositionController)anchorViews[def]).positionLocked)
            {
                return false;
            }
        }
        return true;
    }

    private void SetupButtonEvents()
    {
        LockButton.OnClick.AsObservable().Subscribe(_ => SetAnchor());
        UnlockButton.OnClick.AsObservable().Subscribe(_ => ResetAnchor());
        AlignButton.OnClick.AsObservable().Subscribe(_ => SetAlignment());
        RealignButton.OnClick.AsObservable().Subscribe(_ => SetAlignment());
        FinishLockingButton.OnClick.AsObservable().Subscribe(_ => endLocking());
    }

    private void SetupVoiceCommands()
    {
        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(new Dictionary<string, Action>()
        {
            {"locke", () =>
                {
                    SetAnchor();
                }
            },
            {"unlock", () =>
                {
                    ResetAnchor();
                }
            },
            {"finished", () =>
                {
                    endLocking();
                }
            }
        });
    }

    public LockingAction GetCurrentAction()
    {
        return currentLockingAction;
    }

    public LockingAction GetNextAction()
    {
        if(ProtocolState.LockingTriggered.Value)
        {
            if(currentLockingAction == LockingAction.Aligning)
            {
                if(currentAnchorDefinition + 1 < currentAnchorDefinitions.Count)
                {
                    return LockingAction.Anchoring;
                }else
                {
                    return LockingAction.None;
                }
            }
            else if(currentLockingAction == LockingAction.Anchoring)
            {
                if(currentDef.arDefinitionType == ArDefinitionType.Model)
                {
                    return LockingAction.Aligning; //since we are only using models right now this will trigger most of the time
                }else
                {
                    return LockingAction.Anchoring; //proceed to next model if it is world container
                    //TODO add case or button for this to appear during locking of model if necessary
                }
            }
        }
        return LockingAction.None;
    }

    public LockingAction GetPreviousAction()
    {
        if(ProtocolState.LockingTriggered.Value)
        {
            if(currentLockingAction == LockingAction.Aligning) //unlock model
            {
                return LockingAction.Anchoring;
            }
            else if(currentLockingAction == LockingAction.Anchoring)//set new model to unlock
            {
                if(currentAnchorDefinition == 0)
                {
                    return LockingAction.None;
                }else
                {
                    return LockingAction.Anchoring; //unlock previous model
                }
            }
        }
        return LockingAction.None;
    }

    public LockingAction NextAction()
    {
        if(ProtocolState.LockingTriggered.Value)
        {
            if(currentLockingAction == LockingAction.Aligning)
            {
                SetAlignment();
            }else if(currentLockingAction == LockingAction.Anchoring)
            {
                SetAnchor();
            }
        }
        return GetCurrentAction();
    }
    public LockingAction PreviousAction()
    {
        if(ProtocolState.LockingTriggered.Value && currentLockingAction == LockingAction.Aligning)
        {
            ((WorldPositionController)currentView).UnlockPosition();
            setLockingDisplayText("Set anchor for " + ((AnchorCondition)currentDef.condition).filter);
            AlignmentController.ResetModel(currentView);
            currentLockingAction = LockingAction.Anchoring;
        }
        else
        {
            Debug.LogWarning("Cannot navigate to previous locking action because locking is not in progress");
            currentLockingAction = LockingAction.None;
        }
        return GetCurrentAction();
    }

    public void TriggerLockingPanel()
    {
        if(ProtocolState.LockingTriggered.Value != true)
        {
            if(allAnchorDefinitions.Count > 0)
            {
                InitializeLockingSession(allAnchorDefinitions);
            }else
            {
                Debug.LogWarning("no anchors in scene");
                currentLockingAction = LockingAction.None;
            }
        }else
        {
            Debug.LogWarning("Cannot trigger locking while in locking");
        }
    }

    public LockingAction TriggerLocking(List<ArDefinition> anchors, Dictionary<ArDefinition, ArElementViewController> arViews)
    {
        if(ProtocolState.LockingTriggered.Value != true)
        {
            if(anchors.Count > 0)
            {
                anchorViews = arViews;
                if(allAnchorDefinitions.Count != 0)
                {
                    foreach(ArDefinition ardef in anchors)
                    {
                        if(!allAnchorDefinitions.Contains(ardef))
                        {
                            allAnchorDefinitions.Add(ardef);
                        }
                    }
                }
                else
                {
                    allAnchorDefinitions = anchors;
                }
                InitializeLockingSession(anchors);
            }
            else
            {
                Debug.LogWarning("no anchors selected");
                currentLockingAction = LockingAction.None;
            }
        }
        else
        {
            Debug.LogWarning("Cannot trigger locking while in locking");
        }

        return GetCurrentAction();
    }

    public void CancelLocking()
    {
        endLocking();
    }
    
    public Dictionary<ArDefinition, ArElementViewController> GetSpecificArViews()
    {
        Dictionary<ArDefinition, ArElementViewController> arViews = new Dictionary<ArDefinition, ArElementViewController>(anchorViews);
        return arViews;
    }

    private void CheckForDetections()
    {
        if(!((WorldPositionController)currentView).positionLocked)
        {
            if(currentView.TrackedObjects.Count == 0)
            {
                setLockingDisplayText("No " + ((AnchorCondition)currentDef.condition).filter + " detected. Please manually place the model.");
            }
            else
            {
                setLockingDisplayText(((AnchorCondition)currentDef.condition).filter + " detected. Say 'Lock' to save the position.");
            }
        }

    }

    /*
    IEnumerator triggerManualPlacementPanel()
    {
        ArViewElementController tempView = currentView;
        yield return new WaitForSeconds(15f);
        if(tempView == currentView)
        {
            if(!alignmentTriggered)
            {
                //activate panel
            }
        }
    }*/

}