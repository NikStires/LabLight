using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ChecklistPanelViewController : LLBasePanel
{
    [SerializeField] GameObject noChecklistText;
    [SerializeField] AudioSource audioPlayer;

    private List<ProtocolState.CheckItemState> prevChecklist;
    [SerializeField] List<CheckitemView> checkitemViews;

    [Header("Signoff Icons")]
    [SerializeField] GameObject unlockedIcon;
    [SerializeField] GameObject lockedIcon;

    [Header("Protocol Navigation Buttons")]
    [SerializeField] XRSimpleInteractable closeProtocolButton;
    [SerializeField] XRSimpleInteractable checkItemButton;
    [SerializeField] XRSimpleInteractable unCheckItemButton;
    [SerializeField] XRSimpleInteractable signOffButton;
    [SerializeField] XRSimpleInteractable nextStepButton;
    [SerializeField] XRSimpleInteractable previousStepButton;

    [Header("Popup Event SOs")]
    [SerializeField] PopupEventSO signOffPopupEventSO;
    [SerializeField] PopupEventSO checklistIncompletePopupEventSO;
    [SerializeField] PopupEventSO closeProtocolPopupEventSO;

    private void Awake()
    {
        base.Awake();
        ProtocolState.checklistStream.Subscribe(_ => UpdateVisualState()).AddTo(this);
    }

    void OnEnable()
    {
        SetupButtonEvents();
        SetupVoiceCommands();
    }

    void OnDisable()
    {
        RemoveButtonEvents();
        DisposeVoice?.Invoke();
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
        if (ProtocolState.Steps[ProtocolState.Step].Checklist == null || ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {
            Debug.LogWarning("Already signed off or no item to check");
            return;
        }

        var firstUncheckedItem = (from item in ProtocolState.Steps[ProtocolState.Step].Checklist
                                  where !item.IsChecked.Value
                                  select item).FirstOrDefault();

        if (firstUncheckedItem == null || ProtocolState.LockingTriggered.Value)
        {
            Debug.LogWarning("No item to check or locking triggered");
            return;
        }

        // Check the item and log timestamp
        firstUncheckedItem.IsChecked.Value = true;
        //firstUncheckedItem.CompletionTime = DateTime.Now;

        // Increment checkItem if this is not the last check item
        if (ProtocolState.CheckItem < ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1)
        {
            ProtocolState.SetCheckItem(ProtocolState.CheckItem + 1);
        }
        else
        {
            ProtocolState.SetCheckItem(ProtocolState.CheckItem);
        }
    }

    /// <summary>
    /// unchecks the last checked item if possible
    /// </summary>
    public void UnCheckItem()
    {
        if (ProtocolState.Steps[ProtocolState.Step].Checklist == null || ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {
            Debug.LogWarning("Already signed off or no item to uncheck");
            return;
        }

        var lastCheckedItem = (from item in ProtocolState.Steps[ProtocolState.Step].Checklist
                               where item.IsChecked.Value
                               select item).LastOrDefault();

        if (lastCheckedItem == null || ProtocolState.LockingTriggered.Value)
        {
            Debug.LogWarning("No item to uncheck or locking triggered");
            return;
        }

        // Uncheck the item
        lastCheckedItem.IsChecked.Value = false;
        ProtocolState.SetCheckItem(ProtocolState.Steps[ProtocolState.Step].Checklist.IndexOf(lastCheckedItem));
    }

    /// <summary>
    /// Locks the checklist from further change if all items are complete
    /// </summary>
    public void SignOff()
    {
        if (ProtocolState.Steps[ProtocolState.Step].Checklist == null || ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {
            Debug.LogWarning("Already signed off or no checklist for this step");
            return;
        }

        var uncheckedItemsCount = (from item in ProtocolState.Steps[ProtocolState.Step].Checklist
                                   where !item.IsChecked.Value
                                   select item).Count();

        if (uncheckedItemsCount != 0)
        {
            Debug.LogWarning("Not all items are checked");
            return;
        }

        audioPlayer.Play();
        //update protocol state
        ProtocolState.SignOff();
        //lock sign off indicator in UI
        lockedIcon.SetActive(true);
        unlockedIcon.SetActive(false);
        //write checklist for this step to CSV
        WriteChecklistToCSV(ProtocolState.Steps[ProtocolState.Step].Checklist);
    }


    /// <summary>
    /// Migrating step control to checklist panel...
    /// </summary>
    public void PreviousStep()
    {
        if (ProtocolState.LockingTriggered.Value)
        {
            Debug.LogWarning("cannot navigate to previous step: locking in progress");
            return;
        }
        if(ProtocolState.Step == 0)
        {
            return;
        }
        ProtocolState.SetStep(ProtocolState.Step - 1);
    }

    public void NextStep()
    {
        // If there is no checklist or the checklist is already signed off, proceed to the next step
        if (ProtocolState.Steps[ProtocolState.Step].Checklist == null || ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {
            if (ProtocolState.LockingTriggered.Value)
            {
                Debug.LogWarning("cannot navigate to next step: locking in progress");
                return;
            }
            ProtocolState.SetStep(ProtocolState.Step + 1);
            return;
        }

        // If all items are checked but the checklist is not signed off, show sign off confirmation panel
        if (ProtocolState.CheckItem == ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1 &&
            ProtocolState.Steps[ProtocolState.Step].Checklist[ProtocolState.CheckItem].IsChecked.Value)
        {
            // Update confirmation panel UI and button controls
            Debug.LogWarning("trying to go to next step without signing off");
            signOffPopupEventSO.Open();
            return;
        }

        // If not all items are checked, show checklist incomplete confirmation panel
        Debug.LogWarning("trying to go to the next step without checking all items");
        checklistIncompletePopupEventSO.Open();
    }

    /// <summary>
    /// Destroys stale UI and creates active UI when a new step is loaded 
    /// </summary>
    private void UpdateVisualState()
    {
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
                UpdateCheckItemVisualState(i, checkitemView);
            }
        }
        //default case
        else
        {
            for(int i = ProtocolState.CheckItem - 2; i < ProtocolState.CheckItem + 3; i++)
            {
                CheckitemView checkitemView = checkitemViews[i - (ProtocolState.CheckItem - 2)];
                UpdateCheckItemVisualState(i, checkitemView);
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

    private void UpdateCheckItemVisualState(int index, CheckitemView view)
    {
        if (ProtocolState.Steps[ProtocolState.Step].Checklist.Count > index)
        {
            view.InitalizeCheckItem(ProtocolState.Steps[ProtocolState.Step].Checklist[index]);

            //scale the item based on its position relative to the active item
            float scaleFactor = (float)Math.Pow(1.3, Math.Abs(index - ProtocolState.CheckItem));
            view.transform.localScale = new Vector3(7.5f / scaleFactor, 7.5f / scaleFactor, 0.075f);

            view.gameObject.SetActive(true);

            if (index == ProtocolState.CheckItem)
            {
                view.SetAsActiveItem();
            }
            else
            {
                view.SetAsInactiveItem();
            }
        }
        else
        {
            view.gameObject.SetActive(false);
        }
    }

    void CloseProtocol()
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

    void SetupButtonEvents()
    {
        closeProtocolButton.selectEntered.AddListener(_ => CloseProtocol());
        checkItemButton.selectEntered.AddListener(_ => CheckItem());
        unCheckItemButton.selectEntered.AddListener(_ => UnCheckItem());
        signOffButton.selectEntered.AddListener(_ => SignOff());
        nextStepButton.selectEntered.AddListener(_ => NextStep());
        previousStepButton.selectEntered.AddListener(_ => PreviousStep());
    }

    void RemoveButtonEvents()
    {
        closeProtocolButton.selectEntered.RemoveAllListeners();
        checkItemButton.selectEntered.RemoveAllListeners();
        unCheckItemButton.selectEntered.RemoveAllListeners();
        signOffButton.selectEntered.RemoveAllListeners();
        nextStepButton.selectEntered.RemoveAllListeners();
        previousStepButton.selectEntered.RemoveAllListeners();
    }

    Action DisposeVoice;
    
    //voice commands setup to emulate button presses
    void SetupVoiceCommands()
    {
        DisposeVoice = SpeechRecognizer.Instance.Listen(new Dictionary<string, Action>()
        {
            {"check", async () => 
            {
                checkItemButton.selectEntered.Invoke(null);
                await StartCoroutine(Wait());
                checkItemButton.selectExited.Invoke(null);                
            }},
            {"uncheck", async () => 
            {
                unCheckItemButton.selectEntered.Invoke(null);
                await StartCoroutine(Wait());
                unCheckItemButton.selectExited.Invoke(null);                
            }},
            {"sign", async () => 
            {
                signOffButton.selectEntered.Invoke(null);
                await StartCoroutine(Wait());
                signOffButton.selectExited.Invoke(null);                
            }},
            {"next", async () => 
            {
                nextStepButton.selectEntered.Invoke(null);
                await StartCoroutine(Wait());
                nextStepButton.selectExited.Invoke(null);                
            }},
            {"previous", async () => 
            {
                previousStepButton.selectEntered.Invoke(null);
                await StartCoroutine(Wait());
                previousStepButton.selectExited.Invoke(null);                
            }},
        });
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
    }
}
