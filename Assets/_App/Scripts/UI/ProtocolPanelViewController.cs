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
    [SerializeField] private TextMeshProUGUI procedureTitle;
    [SerializeField] private Transform contentFrame;
    [SerializeField] private XRSimpleInteractable openPDFButton;
    [SerializeField] private ContentItemController contentItemController;

    private UnityUIDriver uiDriver;
    private List<ContentItem> currentContentItems = new List<ContentItem>();

    void Start()
    {
        uiDriver = (UnityUIDriver)ServiceRegistry.GetService<IUIDriver>();
        procedureTitle.text = ProtocolState.Instance.ActiveProtocol.Value.title;
        contentItemController = new ContentItemController();
        UpdateContentItems();
        openPDFButton.selectExited.AddListener(_ => OnOpenPDFButtonClicked());
        
    }

    public void UpdateContentItems()
    {
        var newContentItems = new List<ContentItem>();
        if(ProtocolState.Instance.HasCurrentChecklist() && 
           ProtocolState.Instance.CurrentCheckItemDefinition.contentItems.Count > 0)
        {
            newContentItems.AddRange(ProtocolState.Instance.CurrentStepDefinition.contentItems
                .Where(item => item.contentType.ToLower() != "video"));
            newContentItems.AddRange(ProtocolState.Instance.CurrentCheckItemDefinition.contentItems
                .Where(item => item.contentType.ToLower() != "video"));
        }
        else
        {
            newContentItems.AddRange(ProtocolState.Instance.CurrentStepDefinition.contentItems
                .Where(item => item.contentType.ToLower() != "video"));
        }

        if(newContentItems.Count == 0)
        {
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        else if(currentContentItems != null && currentContentItems == newContentItems)
        {
            currentContentItems = newContentItems;
        }
        else
        {
            contentItemController.ClearContentItems();
            contentItemController.CreateContentItems(newContentItems, contentFrame.GetComponent<LayoutGroup>());
            currentContentItems = newContentItems;
            
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private void OnOpenPDFButtonClicked()
    {
        var protocol = ProtocolState.Instance.ActiveProtocol.Value;
        if (protocol.protocolPDFNames?.Count > 0)
        {
            string pdfName = protocol.protocolPDFNames[0]; // Use first PDF for now
            Debug.Log($"displaying PDF: {pdfName}");
            uiDriver.DisplayPDFReader(pdfName);
        }
        else
        {
            Debug.Log("no PDFs available");
        }
    }
}
