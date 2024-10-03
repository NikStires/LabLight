
using System.Collections.Generic;

public enum LayoutType
{
    Horizontal = 1,
    Vertical = 2,
}

/// <summary>
/// Collection of contentitems with a certain arrangement 
/// </summary>
public class LayoutItem : ContentItem
{
    public LayoutType layoutType = LayoutType.Vertical;

    public List<ContentItem> contentItems = new List<ContentItem>();

    public LayoutItem()
    {
        contentType = ContentType.Layout;
    }
}
