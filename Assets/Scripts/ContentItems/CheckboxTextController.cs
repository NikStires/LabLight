using TMPro;

/// <summary>
/// Text contentitem
/// </summary>
public class CheckboxTextController : ContentController<TextItem>
{
    public TextMeshProUGUI Text;

    public override TextItem ContentItem 
    { 
        get => base.ContentItem; 
        set 
        {
            base.ContentItem = value;
            UpdateView();
        }
    }

    private void UpdateView()
    {
        Text.text = ContentItem.text;

        // Can be updated with more types and type specific styling
        Text.fontSize = (ContentItem.textType == TextType.Header) ? 8 : 6;
    }
}
