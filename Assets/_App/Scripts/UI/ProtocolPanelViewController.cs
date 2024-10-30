using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.IO;

public class ProtocolPanelViewController : MonoBehaviour
{
    UnityUIDriver UIDriver;
    [SerializeField] TextMeshProUGUI procedureTitle;
    //[SerializeField] TextMeshProUGUI stepText;
    [SerializeField] Transform contentFrame;

    [SerializeField] XRSimpleInteractable OpenPDFButton;

    //content item prefabs
    [SerializeField] LayoutController ContainerHorizontalItem;
    [SerializeField] LayoutController ContainerVerticalItem;
    [SerializeField] TextController TextItem;
    [SerializeField] PropertyTextController PropertyItem;
    [SerializeField] ImageController ImageItem;
    [SerializeField] VideoController VideoItem;
    [SerializeField] SoundController SoundItem;

    private List<MonoBehaviour> contentItemInstances = new List<MonoBehaviour>();
    private List<ContentItem> currentContentItems = new List<ContentItem>();

    void Start()
    {
        UIDriver = (UnityUIDriver)ServiceRegistry.GetService<IUIDriver>();
        procedureTitle.text = ProtocolState.Instance.ActiveProtocol.Value.title;
        UpdateContentItems();
        OpenPDFButton.selectExited.AddListener(_ => OnOpenPDFButtonClicked());
    }

    public void UpdateContentItems()
    {
        //Get new content items
        var newContentItems = new List<ContentItem>();
        if(ProtocolState.Instance.HasCurrentChecklist() && ProtocolState.Instance.CurrentCheckItemDefinition.contentItems.Count > 0)
        {
            newContentItems.AddRange(ProtocolState.Instance.CurrentStepDefinition.contentItems.Where(contentItem => contentItem.contentType != ContentType.Video));
            newContentItems.AddRange(ProtocolState.Instance.CurrentCheckItemDefinition.contentItems.Where(contentItem => contentItem.contentType != ContentType.Video));
        }
        else
        {
            newContentItems.AddRange(ProtocolState.Instance.CurrentStepDefinition.contentItems.Where(contentItem => contentItem.contentType != ContentType.Video));
        }

        if(newContentItems.Count == 0)
        {
            //if there are no content items disable view
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        else if(currentContentItems != null && currentContentItems == newContentItems)
        {
            //if content items are the same as the previous content items do nothing
            currentContentItems = newContentItems;
        }
        else
        {
            //if we have new items then clear the old ones and create new ones
            ClearContentItems();
            CreateContentItems(newContentItems, contentFrame.GetComponent<LayoutGroup>(), null);  
            currentContentItems = newContentItems; 
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private void CreateContentItems(List<ContentItem> contentItems, LayoutGroup container, ContainerElementViewController containerController, bool store = true)
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
                    CreateContentItems(layoutItem.contentItems, layoutController.LayoutGroup, containerController);
                    break;
                case ContentType.WebUrl:
                    // Open a web browser
                    WebUrlItem webUrlItem = contentItem as WebUrlItem;
                    UIDriver.DisplayWebPage(webUrlItem.url);
                    break;
                default:
                    break;
            }
        }
    }

    private void ClearContentItems()
    {
        foreach (var contentItem in contentItemInstances)
        {
            Destroy(contentItem.gameObject);
        }
        contentItemInstances.Clear();
    }

    void OnOpenPDFButtonClicked()
    {
        if(string.IsNullOrEmpty(ProtocolState.Instance.ActiveProtocol.Value.pdfPath))
        {
            return;
        }
        UIDriver.DisplayPDFReader(Path.GetFileNameWithoutExtension(ProtocolState.Instance.ActiveProtocol.Value.pdfPath));
    }
}
