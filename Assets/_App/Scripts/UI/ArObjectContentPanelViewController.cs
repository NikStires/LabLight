using UnityEngine;
using UniRx;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ArObjectContentPanelViewController : MonoBehaviour
{
    [SerializeField] private Transform contentFrame;
    [SerializeField] private TextController textPrefab;
    [SerializeField] private ImageController imagePrefab;
    [SerializeField] private SoundController soundPrefab;

    private ArObject associatedArObject;
    private List<ContentItem> currentContentItems = new List<ContentItem>();
    private List<MonoBehaviour> contentItemInstances = new List<MonoBehaviour>();
    private IUIDriver uiDriver;

    private void Awake()
    {
        // Get the associated ArObject from the parent GameObject
        associatedArObject = GetComponentInParent<ArObjectViewController>()?.arObject;
        
        if (associatedArObject == null)
        {
            Debug.LogError("[ArObjectContentPanelViewController] No ArObject found in parent hierarchy");
            return;
        }

        uiDriver = ServiceRegistry.GetService<IUIDriver>();
        InitializeSubscriptions();
    }

    private void InitializeSubscriptions()
    {
        if (ProtocolState.Instance == null)
        {
            Debug.LogError("[ArObjectContentPanelViewController] ProtocolState.Instance is null");
            return;
        }

        // Subscribe to step changes
        ProtocolState.Instance.StepStream
            .Subscribe(_ => UpdateContentPanel())
            .AddTo(this);

        // Subscribe to checklist item changes
        ProtocolState.Instance.ChecklistStream
            .Subscribe(_ => UpdateContentPanel())
            .AddTo(this);
    }

    private void UpdateContentPanel()
    {
        if (associatedArObject == null || string.IsNullOrEmpty(associatedArObject.arObjectID))
        {
            Debug.Log($"[ArObjectContentPanelViewController] No valid AR object. ID: {associatedArObject?.arObjectID} Attempting to get it from parent");
            associatedArObject = GetComponentInParent<ArObjectViewController>()?.arObject;
            if (associatedArObject == null || string.IsNullOrEmpty(associatedArObject.arObjectID))
            {
                Debug.LogError("[ArObjectContentPanelViewController] No valid AR object found after attempting to get it from parent");
                return;
            }
        }

        // Get current step content items
        var stepContentItems = ProtocolState.Instance.CurrentStepDefinition?.contentItems ?? new List<ContentItem>();
        Debug.Log($"[ArObjectContentPanelViewController] Step content items count: {stepContentItems.Count}");
        
        // Get current checklist item content items
        var checklistContentItems = ProtocolState.Instance.CurrentCheckItemDefinition?.contentItems ?? new List<ContentItem>();
        Debug.Log($"[ArObjectContentPanelViewController] Checklist content items count: {checklistContentItems.Count}");

        // Combine and filter content items
        var relevantContentItems = stepContentItems
            .Concat(checklistContentItems)
            .Where(item => 
                !string.IsNullOrEmpty(item.arObjectID) && 
                item.arObjectID.Equals(associatedArObject.arObjectID))
            .ToList();

        Debug.Log($"[ArObjectContentPanelViewController] Relevant content items for AR object '{associatedArObject.arObjectID}': {relevantContentItems.Count}");
        foreach (var item in relevantContentItems)
        {
            Debug.Log($"[ArObjectContentPanelViewController] Content item - Type: {item.contentType}, AR Object ID: {item.arObjectID}");
        }

        // If no relevant content items, hide the panel
        if (!relevantContentItems.Any())
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            ClearContent();
            return;
        }

        // If content hasn't changed, do nothing
        if (currentContentItems != null && currentContentItems.SequenceEqual(relevantContentItems))
        {
            return;
        }

        // Clear existing content and create new content items
        ClearContent();
        foreach (var contentItem in relevantContentItems)
        {
            CreateContentItem(contentItem, contentFrame.GetComponent<LayoutGroup>());
        }
        currentContentItems = relevantContentItems;

        // Show the panel
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void CreateContentItem(ContentItem contentItem, LayoutGroup container)
    {
        Debug.Log($"[ArObjectContentPanelViewController] Creating content item of type: {contentItem.contentType}");
        MonoBehaviour controller = null;

        switch (contentItem.contentType.ToLower())
        {
            case "text":
                var textController = Instantiate(textPrefab, container.transform);
                textController.ContentItem = contentItem;
                controller = textController;
                break;

            case "image":
                var imageController = Instantiate(imagePrefab, container.transform);
                imageController.ContentItem = contentItem;
                controller = imageController;
                break;

            case "sound":
                var soundController = Instantiate(soundPrefab, container.transform);
                soundController.ContentItem = contentItem;
                controller = soundController;
                break;

            case "video":
                if (contentItem.properties.TryGetValue("URL", out object videoURL))
                {
                    uiDriver.DisplayVideoPlayer(videoURL.ToString());
                }
                break;

            case "weburl":
                if (contentItem.properties.TryGetValue("url", out object webURL))
                {
                    uiDriver.DisplayWebPage(webURL.ToString());
                }
                break;
            case "timer":
                if (contentItem.properties.TryGetValue("durationInSeconds", out object durationObj) && 
                    int.TryParse(durationObj.ToString(), out int duration))
                {
                    uiDriver.DisplayTimer(duration);
                }
                break;
            case "pdf":
                if (contentItem.properties.TryGetValue("url", out object pdfURL))
                {
                    uiDriver.DisplayPDFReader(pdfURL.ToString());
                }
                break;
            default:
                Debug.LogWarning($"[ArObjectContentPanelViewController] Unsupported content type: {contentItem.contentType}");
                break;
        }

        if (controller != null)
        {
            Debug.Log($"[ArObjectContentPanelViewController] Successfully created controller for content type: {contentItem.contentType}");
            contentItemInstances.Add(controller);
        }
    }

    private void ClearContent()
    {
        foreach (var contentItem in contentItemInstances)
        {
            if (contentItem != null && contentItem.gameObject != null)
            {
                Destroy(contentItem.gameObject);
            }
        }
        contentItemInstances.Clear();
        currentContentItems.Clear();
    }

    private void OnDestroy()
    {
        ClearContent();
    }
}
