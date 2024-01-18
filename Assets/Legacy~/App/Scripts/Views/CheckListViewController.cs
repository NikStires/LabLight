using System;
using System.Text;
using System.Net.Mail;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;


/// <summary>
/// UI controller for draggable checklist panel
/// </summary>
/// 
public class CheckListViewController : MonoBehaviour
{
    public GameObject View;
    public Transform Content;
    public Transform SignoffIndicator;
    public Sprite lockIcon;
    public Sprite unlockIcon;
    public RadialView radialView;
    public TextMeshProUGUI noCheckItemsText;

    //public GameObject emailEntryPanel;

    private IAudio audioPlayer;
    private List<ProtocolState.CheckItemState> prevChecklist;
    private Vector3 originalContentPos;
    private SpriteRenderer SignoffSprite;
    private RectTransform ContentRect;
    private Snappable SnappableScript;
    private CheckItemPool checkItemPool;

    //used to prevent constant scrolling to active item
    private bool firstRender;
    private bool updateBackplate;
    private bool procedureUpdated;
    private bool pinned = false;

    void Awake()
    {
        checkItemPool = this.GetComponent<CheckItemPool>();
        audioPlayer = ServiceRegistry.GetService<IAudio>();
        //ServiceRegistry.GetService<IMailService>().recipiants.Add("janthonymein@gmail.com");

        SignoffSprite = SignoffIndicator.GetComponent<SpriteRenderer>();

        originalContentPos = Content.GetComponent<RectTransform>().localPosition;
        ContentRect = Content.GetComponent<RectTransform>();
        SnappableScript = this.GetComponent<Snappable>();

        //subscribe to step data stream
        ProtocolState.stepStream.Subscribe(_ => UpdateVisualState()).AddTo(this);

        ProtocolState.procedureDef.Subscribe(_ => procedureUpdated = true).AddTo(this);

        ProtocolState.checklistStream.Subscribe(_ => UpdateVisualState()).AddTo(this);
    }

    void Start()
    {
        CheckItemPool.SharedInstance.CreatePooledObjects();
    }

    Action disposeVoice;

    private void OnEnable()
    {
        SetupVoiceCommands();
        //scroll down to active item if not the first two items
        if (ProtocolState.CheckItem > 1)
        {
            StartCoroutine(ScrollToActiveItem(Content as RectTransform, 0.5f));
        }
    }

    private void OnDisable()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    private void SetupVoiceCommands()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(new Dictionary<string, Action>()
        {
            {"check", () => CheckItem() },
            {"cheh", () => CheckItem() },
            {"uncheck", () => UnCheckItem() },
            {"sign off", () => SignOff() },
            {"fetch checklist", () => StartCoroutine(MoveIntoView(1f)) },
        });
    }

    /// <summary>
    /// Handles UI operations that need to take place post render
    /// </summary>
    private void LateUpdate()
    {
        if (updateBackplate || firstRender)
        {
            StartCoroutine(UpdateBackplates());
        }
        if (firstRender)
        {
            //scroll down to active item if not the first two items
            if (ProtocolState.CheckItem > 1)
            {
                StartCoroutine(ScrollToActiveItem(Content as RectTransform, 0.5f));
            }
            firstRender = false;
        }
        if(procedureUpdated)
        {
            UpdateVisualState();
            procedureUpdated = false;
        }
    }

    /// <summary>
    /// Checks the next unchecked item if possible
    /// </summary>
    public void CheckItem()
    {
        if (!ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {

            var firstUncheckedItem = (from item in ProtocolState.Steps[ProtocolState.Step].Checklist
                                      where !item.IsChecked.Value
                                      select item).FirstOrDefault();

            if (firstUncheckedItem != null && !ProtocolState.LockingTriggered.Value)
            {
                audioPlayer.Play(AudioEventEnum.Check);

                //if this is not the first or last check item scroll the checklist
                if (ProtocolState.CheckItem > 0 && ProtocolState.CheckItem < ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1)
                {
                    //get height of last checked item
                    float ItemHeight = Content.GetChild(ProtocolState.CheckItem).GetComponent<RectTransform>().rect.height;
                    //start slide animation
                    StartCoroutine(SlideRectTransform(Content as RectTransform, ItemHeight, 0.25f));
                }

                //check it and log timestamp
                firstUncheckedItem.IsChecked.Value = true;
                firstUncheckedItem.CompletionTime = DateTime.Now;
                //allow backplate update
                updateBackplate = true;

                //Increment checkItem if this is not the last check item
                if(ProtocolState.CheckItem < ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1)
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
                audioPlayer.Play(AudioEventEnum.Error);
                Debug.LogWarning("No item to check or locking triggered");
            }
        }
        else
        {
            audioPlayer.Play(AudioEventEnum.Error);
            Debug.LogWarning("Already signed off");
        }
    }

    /// <summary>
    /// unchecks the last checked item if possible
    /// </summary>
    public void UnCheckItem()
    {
        if (!ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {

            var lastCheckedItem = (from item in ProtocolState.Steps[ProtocolState.Step].Checklist
                                   where item.IsChecked.Value
                                   select item).LastOrDefault();

            if (lastCheckedItem != null && !ProtocolState.LockingTriggered.Value)
            {
                //uncheck the item
                audioPlayer.Play(AudioEventEnum.Uncheck);
                lastCheckedItem.IsChecked.Value = false;

                //allow backplate update
                updateBackplate = true;

                ProtocolState.SetCheckItem(ProtocolState.Steps[ProtocolState.Step].Checklist.IndexOf(lastCheckedItem));

                // //if the last checked item is not the final item in the list decrement the check item
                // if (ProtocolState.Steps[ProtocolState.Step].Checklist.IndexOf(lastCheckedItem) != ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1)
                // {
                //     ProtocolState.SetCheckItem(ProtocolState.CheckItem - 1);
                //     //allow backplate update
                //     updateBackplate = true;
                // }
                // else
                // {
                //     ProtocolState.SetCheckItem(ProtocolState.CheckItem);
                //     //allow backplate update
                //     updateBackplate = true;
                // }

                if (ProtocolState.CheckItem > 0 && ProtocolState.CheckItem < ProtocolState.Steps[ProtocolState.Step].Checklist.Count - 1)
                {
                    //get the height of the last checked item
                    float ItemHeight = Content.GetChild(ProtocolState.CheckItem).GetComponent<RectTransform>().rect.height;
                    //start scroll animation
                    StartCoroutine(SlideRectTransform(Content as RectTransform, -ItemHeight, 0.25f));
                }
            }
            else
            {
                audioPlayer.Play(AudioEventEnum.Error);
                Debug.LogWarning("No item to uncheck or locking triggered");
            }
        }
        else
        {
            audioPlayer.Play(AudioEventEnum.Error);
            Debug.LogWarning("Already signed off");
        }
    }

    /// <summary>
    /// Locks the checklist from further change if all items are complete
    /// </summary>
    public void SignOff()
    {
        if (!ProtocolState.Steps[ProtocolState.Step].SignedOff)
        {

            var uncheckedItemsCount = (from item in ProtocolState.Steps[ProtocolState.Step].Checklist
                                       where !item.IsChecked.Value
                                       select item).Count();

            if (uncheckedItemsCount == 0)
            {
                audioPlayer.Play(AudioEventEnum.SignOff);
                //update protocol state
                ProtocolState.SignOff();
                //lock sign off indicator in UI
                SignoffSprite.sprite = lockIcon;
                //write checklist for this step to CSV
                WriteChecklistToCSV(ProtocolState.Steps[ProtocolState.Step].Checklist);

/*                //if this is the final step of the procedure send the CSV
                if ((SessionState.Step + 1) == SessionState.procedureDef.Value.steps.Count)
                {
                    emailEntryPanel.SetActive(true);
                }*/
            }
            else
            {
                audioPlayer.Play(AudioEventEnum.Error);
                Debug.LogWarning("Not all items are checked");
            }
        }
        else
        {
            audioPlayer.Play(AudioEventEnum.Error);
            Debug.LogWarning("Already signed off");
        }
    }

    /// <summary>
    /// Destroys stale UI and creates active UI when a new step is loaded 
    /// </summary>
    private void UpdateVisualState()
    {
        // if the procedure, step or checklist is null exit
        if (ProtocolState.procedureDef.Value == null || ProtocolState.Steps[ProtocolState.Step] == null || ProtocolState.Steps[ProtocolState.Step].Checklist == null)
        {
            prevChecklist = null;

            foreach(var item in CheckItemPool.SharedInstance.pooledObjects)
            {
                item.SetActive(false);
            }

            View.SetActive(false);
            noCheckItemsText.gameObject.SetActive(true);

            return;
        }

        //if we have a new checklist delete old items and create new ones
        if (prevChecklist != ProtocolState.Steps[ProtocolState.Step].Checklist)
        {
            //reset checklist scroll position
            ContentRect.localPosition = originalContentPos;

            //allow autoscroll to active item
            firstRender = true;

            //save reference to current checklist
            prevChecklist = ProtocolState.Steps[ProtocolState.Step].Checklist;

            //clear previous checklist UI
            foreach (var item in CheckItemPool.SharedInstance.pooledObjects)
            {
                item.SetActive(false);
            }

            foreach (var cli in ProtocolState.Steps[ProtocolState.Step].Checklist)
            {
                //update the check item on the checklist UI
                Interactable interactableToggle = checkItemPool.GetPooledObject().GetComponent<Interactable>();
                interactableToggle.GetComponent<TextController>().ContentItem = new TextItem
                {
                    text = cli.Text
                };

                // Update value by setting click handlers
                interactableToggle.OnClick.AsObservable().
                    Subscribe(_ => { cli.IsChecked.Value = interactableToggle.IsToggled; });

                // Update UI by subscribing to changes
                cli.IsChecked.Subscribe(value =>
                {
                    interactableToggle.IsToggled = value;
                }).AddTo(this);

                interactableToggle.gameObject.SetActive(true);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(ContentRect);
        }

        //display proper signoff icon
        if (!ProtocolState.Steps[ProtocolState.Step].SignedOff)
            SignoffSprite.sprite = unlockIcon;
        else
            SignoffSprite.sprite = lockIcon;

        noCheckItemsText.gameObject.SetActive(false);
        View.SetActive(true);
    }


    //coroutines for animations

    /// <summary>
    /// scrolls rect transform up or down
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="distance"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator SlideRectTransform(RectTransform rectTransform, float distance, float duration)
    {
        var originalPosition = rectTransform.localPosition;

        Vector3 endPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y + distance, rectTransform.localPosition.z);

        float startTime = Time.time;

        // Slide the rect transform smoothly from its original position to the end position over the specified duration
        while (Time.time < startTime + duration)
        {
            rectTransform.localPosition = Vector3.Lerp(originalPosition, endPosition, (Time.time - startTime) / duration);
            yield return null;
        }

        // Set the rect transform to the end position to ensure it is exactly at the desired location
        rectTransform.localPosition = endPosition;
    }

    /// <summary>
    /// Scrolls to active checklist item when a new step is loaded
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator ScrollToActiveItem(RectTransform rectTransform, float duration)
    {
        //We need 3 waits for this to work properly dont ask me why NS
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        //reset checklist scroll position
        ContentRect.localPosition = originalContentPos;

        var originalPosition = rectTransform.localPosition;

        float ScrollHeight = 0f;

        //get height of all checked items
        for (int i = 0; i < ProtocolState.CheckItem - 1; i++)
        {
            ScrollHeight += Content.GetChild(i).GetComponent<RectTransform>().rect.height;
        }

        Vector3 endPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y + ScrollHeight, rectTransform.localPosition.z);

        float startTime = Time.time;

        // Slide the rect transform smoothly from its original position to the end position over the specified duration
        while (Time.time < startTime + duration)
        {
            rectTransform.localPosition = Vector3.Lerp(originalPosition, endPosition, (Time.time - startTime) / duration);
            yield return null;
        }

        // Set the rect transform to the end position to ensure it is exactly at the desired location
        rectTransform.localPosition = endPosition;
    }

    /// <summary>
    /// Triggers smoothe movement of the checklist panel into operators POV
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator MoveIntoView(float duration)
    {
        //make sure the checklist is not snapped to the protocol panel
        SnappableScript.UnSnap();

        float time = 0;
        Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.6f;
        
        var rotationEuler = Camera.main.transform.rotation.eulerAngles;
        this.transform.rotation = Quaternion.Euler(0, rotationEuler.y, 0);
        
        Vector3 startPosition = transform.position;
        
        //smoothe movement of panel into operators POv
        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        //Make sure we got where we wanted to go
        transform.position = targetPosition;
    }

    IEnumerator UpdateBackplates()
    {
        yield return new WaitForEndOfFrame();
        CheckItemViewController checkItem;

        //deactivate all backplates except the active item backplate
        for (int i = 0; i < ProtocolState.Steps[ProtocolState.Step].Checklist.Count(); i++)
        {
            checkItem = Content.GetChild(i).GetComponent<CheckItemViewController>();

            if (i < ProtocolState.CheckItem - 1)
            {
                checkItem.DeactivateBackplate();
                checkItem.DeactivateCheckmarkGroup();
            }
            else if (i == ProtocolState.CheckItem - 1)
            {
                checkItem.DeactivateBackplate();
                checkItem.ActiveCheckmarkGroup();
            }
            else if (i == ProtocolState.CheckItem)
            {
                checkItem.ActivateBackplate();
                checkItem.ActiveCheckmarkGroup();
            }
            else
            {
                checkItem.DeactivateBackplate();
                checkItem.ActiveCheckmarkGroup();
            }
        }

        updateBackplate = false;
    }

    public void TogglePin()
    {
        if(pinned)
        {
            radialView.enabled = true;
            pinned = false;
        }
        else
        {
            radialView.enabled = false;
            pinned = true;
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
