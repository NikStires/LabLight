using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ContentItemController : MonoBehaviour
{
    private TextController textPrefab;
    private ImageController imagePrefab;
    private VideoController videoPrefab;
    private SoundController soundPrefab;

    public TextController TextPrefab { set => textPrefab = value; }
    public ImageController ImagePrefab { set => imagePrefab = value; }
    public VideoController VideoPrefab { set => videoPrefab = value; }
    public SoundController SoundPrefab { set => soundPrefab = value; }

    private List<MonoBehaviour> contentItemInstances = new List<MonoBehaviour>();
    private UnityUIDriver uiDriver;

    private void Awake()
    {
        uiDriver = (UnityUIDriver)ServiceRegistry.GetService<IUIDriver>();
    }

    public void CreateContentItems(List<ContentItem> contentItems, LayoutGroup container)
    {
        foreach (var contentItem in contentItems)
        {
            CreateContentItem(contentItem, container);
        }
    }

    private void CreateContentItem(ContentItem contentItem, LayoutGroup container)
    {
        MonoBehaviour controller = null;

        switch (contentItem.contentType.ToLower())
        {
            // case "property": depricated
            //     var propertyController = Instantiate(propertyPrefab, container.transform);
            //     propertyController.ContentItem = contentItem;
            //     propertyController.ContainerController = containerController;
            //     controller = propertyController;
            //     break;

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

            // case "layout": depricated
            //     bool isVertical = contentItem.properties.TryGetValue("layoutType", out object layoutType) 
            //         && layoutType.ToString().ToLower() == "vertical";
                
            //     var layoutController = Instantiate(
            //         isVertical ? containerVerticalPrefab : containerHorizontalPrefab, 
            //         container.transform
            //     );
            //     layoutController.ContentItem = contentItem;
            //     controller = layoutController;

            //     if (contentItem.properties.TryGetValue("contentItems", out object items) 
            //         && items is List<ContentItem> childItems)
            //     {
            //         CreateContentItems(childItems, layoutController.LayoutGroup, containerController);
            //     }
            //     break;

            case "weburl":
                if (contentItem.properties.TryGetValue("url", out object url))
                {
                    uiDriver.DisplayWebPage(url.ToString());
                }
                break;
        }

        if (controller != null)
        {
            contentItemInstances.Add(controller);
        }
    }

    public void ClearContentItems()
    {
        foreach (var contentItem in contentItemInstances)
        {
            Destroy(contentItem.gameObject);
        }
        contentItemInstances.Clear();
    }
} 