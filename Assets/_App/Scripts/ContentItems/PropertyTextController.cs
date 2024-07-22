using TMPro;
using UnityEngine;

/// <summary>
/// Property contentitem
/// </summary>
public class PropertyTextController : ContentController<PropertyItem>
{
    public TextMeshProUGUI Text;
    public Transform Backplate;
    private ContainerElementViewController containerController;

    public override PropertyItem ContentItem 
    { 
        get => base.ContentItem; 
        set 
        {
            base.ContentItem = value;
            UpdateView();
        }
    }

    public ContainerElementViewController ContainerController
    {
        get
        {
            return containerController;
        }
        set
        {
            containerController = value;
            containerController.positionLocked = false;
            UpdateView();
        }
    }

    private void UpdateView()
    {
        Text.text = ContentItem.propertyName;

        if (containerController != null && containerController.TrackedObjects != null  && containerController.TrackedObjects.Count == 1)
        {
            var toType = containerController.TrackedObjects[0].GetType();
            var fieldInfo = toType.GetField(ContentItem.propertyName);
            if (fieldInfo != null)
            {
                // Use reflection to gather property value
                Text.text = fieldInfo.GetValue(containerController.TrackedObjects[0]).ToString();
            }
        }
    }

    void OnPostRender()
    {
        Backplate.localScale = new Vector3(this.transform.localScale.x + 2, Backplate.localScale.y, Backplate.localScale.z);
    }
}
