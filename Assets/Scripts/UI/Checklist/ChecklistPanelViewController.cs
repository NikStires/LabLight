using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ChecklistPanelViewController : MonoBehaviour
{
    [SerializeField] GameObject unlockedIcon;
    [SerializeField] GameObject lockedIcon;

    [SerializeField] GameObject noChecklistText;

    [SerializeField] XRSimpleInteractable closeProtocolButton;

    private List<ProtocolState.CheckItemState> prevChecklist;
    public List<CheckitemView> checkitemViews;

    //popups
    [SerializeField] PopupEventSO signOffPopupEventSO;
    [SerializeField] PopupEventSO checklistIncompletePopupEventSO;
    [SerializeField] PopupEventSO closeProtocolPopupEventSO;

    private void Awake()
    {
        ProtocolState.checklistStream.Subscribe(_ => UpdateVisualState()).AddTo(this);

        closeProtocolButton.selectEntered.AddListener(_ =>
        {
            var lastStepWithChecklist = ProtocolState.Steps.Where(step => step.Checklist != null).LastOrDefault();
            
            //if we are on the last step and the last checklist has been signed off close the protocol
            if(ProtocolState.Step == (ProtocolState.Steps.Count - 1) && lastStepWithChecklist != null && lastStepWithChecklist.SignedOff)
            {
                SessionState.Instance.activeProtocol = null;
                SceneLoader.Instance.LoadSceneClean("ProtocolMenu");   
            }
            //open a popup to confirm closing the protocol
            else
            {
                closeProtocolPopupEventSO.Open();
            }
        });
    }

    void Start()
    {
        UpdateVisualState();

        signOffPopupEventSO.OnYesButtonPressed.AddListener(() =>
        {
            SignOff(); 
            NextStep();
        });

        checklistIncompletePopupEventSO.OnYesButtonPressed.AddListener(() =>
        {
            ProtocolState.SetStep(ProtocolState.Step + 1);
        });

        closeProtocolPopupEventSO.OnYesButtonPressed.AddListener(() =>
        {
            SessionState.Instance.activeProtocol = null;
            SceneLoader.Instance.LoadSceneClean("ProtocolMenu");
        });
    }

    /// <summary>
    /// Checks the next unchecked item if possible
    /// </summary>
    public void CheckItem()
    {
        if (ProtocolState.Steps[ProtocolState.Step].Checklist != null && !ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {
            var firstUncheckedItem = (from item in ProtocolState.Steps[ProtocolState.Step].Checklist
                                      where !item.IsChecked.Value
                                      select item).FirstOrDefault();

            if (firstUncheckedItem != null && !ProtocolState.LockingTriggered.Value)
            {
                //check it and log timestamp
                firstUncheckedItem.IsChecked.Value = true;
                //firstUncheckedItem.CompletionTime = DateTime.Now;

                //Increment checkItem if this is not the last check item
                if (ProtocolState.CheckItem < ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1)
                {
                    ProtocolState.SetCheckItem(ProtocolState.CheckItem + 1);
                }
                else
                {
                    ProtocolState.SetCheckItem(ProtocolState.CheckItem);
                }
            }
            else
            {
                Debug.LogWarning("No item to check or locking triggered");
            }
        }
        else
        {
            Debug.LogWarning("Already signed off or no item to check");
        }
    }

    /// <summary>
    /// unchecks the last checked item if possible
    /// </summary>
    public void UnCheckItem()
    {
        if (ProtocolState.Steps[ProtocolState.Step].Checklist != null && !ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {

            var lastCheckedItem = (from item in ProtocolState.Steps[ProtocolState.Step].Checklist
                                   where item.IsChecked.Value
                                   select item).LastOrDefault();

            if (lastCheckedItem != null && !ProtocolState.LockingTriggered.Value)
            {
                //uncheck the item
                lastCheckedItem.IsChecked.Value = false;
                ProtocolState.SetCheckItem(ProtocolState.Steps[ProtocolState.Step].Checklist.IndexOf(lastCheckedItem));
            }
            else
            {
                Debug.LogWarning("No item to uncheck or locking triggered");
            }
        }
        else
        {
            Debug.LogWarning("Already signed off or no item to uncheck");
        }
    }

    /// <summary>
    /// Locks the checklist from further change if all items are complete
    /// </summary>
    public void SignOff()
    {
        if (ProtocolState.Steps[ProtocolState.Step].Checklist != null && !ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {

            var uncheckedItemsCount = (from item in ProtocolState.Steps[ProtocolState.Step].Checklist
                                       where !item.IsChecked.Value
                                       select item).Count();

            if (uncheckedItemsCount == 0)
            {
                //update protocol state
                ProtocolState.SignOff();
                //lock sign off indicator in UI
                lockedIcon.SetActive(true);
                unlockedIcon.SetActive(false);
                //write checklist for this step to CSV
                WriteChecklistToCSV(ProtocolState.Steps[ProtocolState.Step].Checklist);
            }
            else
            {
                Debug.LogWarning("Not all items are checked");
            }
        }
        else
        {
            Debug.LogWarning("Already signed off or no checklist for this step");
        }
    }


    /// <summary>
    /// Migrating step control to checklist panel...
    /// </summary>
    public void PreviousStep()
    {
        if (ProtocolState.LockingTriggered.Value)
        {
            //audioPlayer?.Play(AudioEventEnum.Error);
            Debug.LogWarning("cannot navigate to previous step: locking in progress");
            return;
        }
        //if (!SessionState.ConfirmationPanelVisible.Value)
        //{
        //    audioPlayer?.Play(AudioEventEnum.PreviousStep);
        //    ProtocolState.SetStep(ProtocolState.Step - 1);
        //}
        if(ProtocolState.Step == 0)
        {
            return;
        }
        ProtocolState.SetStep(ProtocolState.Step - 1);
    }

    public void NextStep()
    {
        //if there is a checklist that has not been signed off verify that the operator wants to progress
        if (ProtocolState.Steps[ProtocolState.Step].Checklist != null)
        {
            if(!ProtocolState.Steps[ProtocolState.Step].SignedOff)
            {
                //if all items are checked but checklist is not signed off
                if (ProtocolState.CheckItem == ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1 &&
                    ProtocolState.Steps[ProtocolState.Step].Checklist[ProtocolState.CheckItem].IsChecked.Value)
                {
                    //update confirmation panel UI and button controls
                    Debug.LogWarning("trying to go to next step without signing off");
                    signOffPopupEventSO.Open();
                    return;
                }
                else
                {
                    //update confirmation panel UI and button controls
                    Debug.LogWarning("trying to go to the next step without checking all items");
                    checklistIncompletePopupEventSO.Open();
                    return;
                }
            }

            if (ProtocolState.LockingTriggered.Value)
            {
                //audioPlayer?.Play(AudioEventEnum.Error);
                Debug.LogWarning("cannot navigate to next step: locking in progress");
                return;
            }
        }
        ProtocolState.SetStep(ProtocolState.Step + 1);
    }

    /// <summary>
    /// Destroys stale UI and creates active UI when a new step is loaded 
    /// </summary>
    private void UpdateVisualState()
    {
        Debug.Log("Updating checklist panel visual state");
        // if the procedure, step or checklist is null exit
        if (ProtocolState.procedureDef == null || ProtocolState.Steps[ProtocolState.Step] == null || ProtocolState.Steps[ProtocolState.Step].Checklist == null)
        {
            prevChecklist = null;

            foreach(var view in checkitemViews)
            {
                view.gameObject.SetActive(false);
            }

            noChecklistText.SetActive(true);

            return;
        }
        
        noChecklistText.SetActive(false);

        //Update views to display the 5 most relevant items

        //case for start of checklist
        if (ProtocolState.CheckItem < 2)
        {
            for (int i = 0; i < 5; i++)
            {
                CheckitemView checkitemView = checkitemViews[i];
                if (ProtocolState.Steps[ProtocolState.Step].Checklist.Count > i)
                {
                    checkitemView.InitalizeCheckItem(ProtocolState.Steps[ProtocolState.Step].Checklist[i]);

                    //scale the item based on its position relative to the active item
                    float scaleFactor = (float)Math.Pow(1.3, Math.Abs(i - ProtocolState.CheckItem));
                    checkitemView.transform.localScale = new Vector3(7.5f / scaleFactor, 7.5f / scaleFactor, 0.075f);

                    checkitemView.gameObject.SetActive(true);

                    if (i == ProtocolState.CheckItem)
                    {
                        checkitemView.SetAsActiveItem();
                    }
                    else
                    {
                        checkitemView.SetAsInactiveItem();
                    }
                }
                else
                {
                    checkitemView.gameObject.SetActive(false);
                }
            }
        }
        //default case
        else
        {
            for(int i = ProtocolState.CheckItem - 2; i < ProtocolState.CheckItem + 3; i++)
            {
                CheckitemView checkitemView = checkitemViews[i - (ProtocolState.CheckItem - 2)];
                if (ProtocolState.Steps[ProtocolState.Step].Checklist.Count > i)
                {
                    checkitemView.InitalizeCheckItem(ProtocolState.Steps[ProtocolState.Step].Checklist[i]);

                    //scale the item based on its position relative to the active item
                    float scaleFactor = (float)Math.Pow(1.3, Math.Abs(i - ProtocolState.CheckItem));
                    checkitemView.transform.localScale = new Vector3(7.5f / scaleFactor, 7.5f / scaleFactor, 0.075f);

                    checkitemView.gameObject.SetActive(true);

                    if (i == ProtocolState.CheckItem)
                    {
                        checkitemView.SetAsActiveItem();
                    }
                    else
                    {
                        checkitemView.SetAsInactiveItem();
                    }
                }
                else
                {
                    checkitemView.gameObject.SetActive(false);
                }
            }
        }

        //if we have a new checklist delete old items and create new ones
        if (prevChecklist != ProtocolState.Steps[ProtocolState.Step].Checklist)
        {
            //save reference to current checklist
            prevChecklist = ProtocolState.Steps[ProtocolState.Step].Checklist;
        }

        //display proper signoff icon
        if (ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {
            lockedIcon.SetActive(true);
            unlockedIcon.SetActive(false);
        }
        else
        {
            lockedIcon.SetActive(false);
            unlockedIcon.SetActive(true);
        }
    }

    public void WriteChecklistToCSV(List<ProtocolState.CheckItemState> CheckList)
    {
        if (ProtocolState.CsvPath == null)
        {
            Debug.LogWarning("no CSV file initalized");
            return;
        }

        // Initalize text writer to specified file and append
        var tw = new StreamWriter(ProtocolState.CsvPath, true);
        string line;

        // Write checklist items to CSV
        if (CheckList != null)
        {
            foreach (var item in CheckList)
            {
                line = item.Text;
                if (line.Contains(","))
                {
                    line = line.Replace(",", "");
                }

                tw.WriteLine(line + ',' + "Completed, " + item.CompletionTime);
            }
            tw.Close();
        }
    }
}
