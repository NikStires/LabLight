using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class ChecklistPanelViewController : MonoBehaviour
{
    [SerializeField] GameObject unlockedIcon;
    [SerializeField] GameObject lockedIcon;

    private List<ProtocolState.CheckItemState> prevChecklist;

    public List<CheckitemView> checkitemViews;

    private void Awake()
    {
        ProtocolState.procedureDef.Subscribe(_ => UpdateVisualState()).AddTo(this);

        ProtocolState.stepStream.Subscribe(_ => UpdateVisualState()).AddTo(this);
            
        ProtocolState.checklistStream.Subscribe(_ => UpdateVisualState()).AddTo(this);
    }

    /// <summary>
    /// Checks the next unchecked item if possible
    /// </summary>
    public void CheckItem()
    {
        Debug.Log("check");
        if (!ProtocolState.Steps[ProtocolState.Step].SignedOff)
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
            Debug.LogWarning("Already signed off");
        }
    }

    /// <summary>
    /// unchecks the last checked item if possible
    /// </summary>
    public void UnCheckItem()
    {
        Debug.Log("uncheck");
        if (!ProtocolState.Steps[ProtocolState.Step].SignedOff)
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
            Debug.LogWarning("Already signed off");
        }
    }

    /// <summary>
    /// Locks the checklist from further change if all items are complete
    /// </summary>
    public void SignOff()
    {
        Debug.Log("SignOff");
        if (!ProtocolState.Steps[ProtocolState.Step].SignedOff)
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
            Debug.LogWarning("Already signed off");
        }
    }


    /// <summary>
    /// Destroys stale UI and creates active UI when a new step is loaded 
    /// </summary>
    private void UpdateVisualState()
    {
        Debug.Log("Updating checklist visual state");
        // if the procedure, step or checklist is null exit
        if (ProtocolState.procedureDef.Value == null || ProtocolState.Steps[ProtocolState.Step] == null || ProtocolState.Steps[ProtocolState.Step].Checklist == null)
        {
            prevChecklist = null;

            //TODO: Deactivate View here

            return;
        }

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
                    checkitemView.transform.localScale = new Vector3(1 / scaleFactor, 1 / scaleFactor, 1);

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
                    checkitemView.transform.localScale = new Vector3(1 / scaleFactor, 1 / scaleFactor, 1);

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
