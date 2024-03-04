using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using UnityEngine.XR.Interaction.Toolkit;

public class ProtocolPanelViewController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI procedureTitle;
    [SerializeField] TextMeshProUGUI stepText;
    [SerializeField] Transform contentFrame;

    //content item prefabs
    [SerializeField] LayoutController ContainerHorizontalItem;
    [SerializeField] LayoutController ContainerVerticalItem;
    [SerializeField] TextController TextItem;
    [SerializeField] PropertyTextController PropertyItem;
    [SerializeField] ImageController ImageItem;
    [SerializeField] VideoController VideoItem;
    [SerializeField] SoundController SoundItem;

    private List<MonoBehaviour> contentItemInstances = new List<MonoBehaviour>();

    private void Awake()
    {
        //ProtocolState.stepStream.Subscribe(_ => { UpdateStepDisplay(); }).AddTo(this);

        ProtocolState.checklistStream.Subscribe(_ => UpdateContentItems()).AddTo(this);
    }

    void Start()
    {
        procedureTitle.text = ProtocolState.procedureDef.title;
        //UpdateStepDisplay();
        UpdateContentItems();
    }

    private void UpdateContentItems()
    {
        //remove all content item views
        foreach (Transform child in contentFrame)
        {
            var imageController = child.GetComponent<ImageController>();
            if (imageController)
            {
                contentItemInstances.Remove(imageController);
                Destroy(imageController.gameObject);
                Destroy(child);
                continue;
            }
            var textController = child.GetComponent<TextController>();
            if (textController)
            {
                contentItemInstances.Remove(textController);
                Destroy(textController.gameObject);
                Destroy(child);
            }
        }

        if (ProtocolState.procedureDef != null)
        {
            //create content items for this step
            var currentStep = ProtocolState.procedureDef.steps[ProtocolState.Step];

            //create content items for current check item, if no content items for check item show step content items
            if (currentStep.checklist != null && currentStep.checklist[ProtocolState.CheckItem].contentItems.Count > 0)
            {
                CreateContentItem(currentStep.contentItems, contentFrame.GetComponent<LayoutGroup>(), null);
                CreateContentItem(currentStep.checklist[ProtocolState.CheckItem].contentItems, contentFrame.GetComponent<LayoutGroup>(), null);
            }
            else
            {
                CreateContentItem(currentStep.contentItems, contentFrame.GetComponent<LayoutGroup>(), null);
            }
        }
    }

    private void CreateContentItem(List<ContentItem> contentItems, LayoutGroup container, ContainerElementViewController containerController, bool store = true)
    {
        foreach (var contentItem in contentItems)
        {
            switch (contentItem.contentType)
            {
                case ContentType.Property:
                    PropertyTextController propertyTextController = Instantiate(PropertyItem, container.transform);
                    propertyTextController.ContentItem = contentItem as PropertyItem;
                    propertyTextController.ContainerController = containerController;

                    if (store)
                    {
                        contentItemInstances.Add(propertyTextController);
                    }
                    break;
                case ContentType.Text:
                    var textController = Instantiate(TextItem, container.transform);
                    textController.ContentItem = contentItem as TextItem;

                    if (store)
                    {
                        contentItemInstances.Add(textController);
                    }
                    break;
                case ContentType.Image:
                    var imageController = Instantiate(ImageItem, container.transform);
                    imageController.ContentItem = contentItem as ImageItem;

                    if (store)
                    {
                        contentItemInstances.Add(imageController);
                    }
                    break;
                case ContentType.Video:
                    var videoController = Instantiate(VideoItem, container.transform);
                    videoController.ContentItem = contentItem as VideoItem;

                    if (store)
                    {
                        contentItemInstances.Add(videoController);
                    }
                    break;
                case ContentType.Sound:
                    var soundController = Instantiate(SoundItem, container.transform);
                    soundController.ContentItem = contentItem as SoundItem;

                    if (store)
                    {
                        contentItemInstances.Add(soundController);
                    }
                    break;
                case ContentType.Layout:
                    // Recurse into subcontainers and their items
                    LayoutItem layoutItem = ((LayoutItem)contentItem);
                    var layoutController = Instantiate((layoutItem.layoutType == LayoutType.Vertical) ? ContainerVerticalItem : ContainerHorizontalItem, container.transform);
                    layoutController.ContentItem = contentItem as LayoutItem;

                    if (store)
                    {
                        contentItemInstances.Add(layoutController);
                    }
                    CreateContentItem(layoutItem.contentItems, layoutController.LayoutGroup, containerController);
                    break;
            }
        }
    }

    private void clearContentItems()
    {
        foreach (var contentItem in contentItemInstances)
        {
            Destroy(contentItem.gameObject);
        }
        contentItemInstances.Clear();
    }

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
                    //confirmationPanelVC.SignOffMessage();
                    return;
                }
                else
                {
                    //update confirmation panel UI and button controls
                    Debug.LogWarning("trying to go to the next step without checking all items");
                    //confirmationPanelVC.ChecklistIncompleteMessage();
                    return;
                }
            }

            if (ProtocolState.LockingTriggered.Value)
            {
                //audioPlayer?.Play(AudioEventEnum.Error);
                Debug.LogWarning("cannot navigate to next step: locking in progress");
                return;
            }

            ProtocolState.SetStep(ProtocolState.Step + 1);
        }
    }

    void UpdateStepDisplay()
    {
        if (ProtocolState.procedureDef == null)
        {
            stepText.text = "0/0";
            return;
        }

        int stepCount = (ProtocolState.procedureDef.steps != null) ? ProtocolState.procedureDef.steps.Count : 0;

        stepText.text = string.Format("{0}/{1}", Math.Min(stepCount, ProtocolState.Step + 1), stepCount);

        if (ProtocolState.procedureDef.steps[ProtocolState.Step].isCritical || (ProtocolState.Step > 0 && ProtocolState.procedureDef.steps[ProtocolState.Step - 1].isCritical))
        {
            if (SessionState.Recording)
            {
               ServiceRegistry.GetService<ILighthouseControl>()?.StopRecordingVideo();
            }
            //if (ProtocolState.procedureDef.steps[ProtocolState.Step].isCritical & confirmationPanelVC != null)
            //{
            //    confirmationPanelVC.CriticalStepMessage();
            //}
        }
    }
}
