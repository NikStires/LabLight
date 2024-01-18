using System.Collections;

/// <summary>
/// TrackedObject property as string
/// </summary>
public class PropertyItem : ContentItem
{
    public string propertyName;

    //private static IEnumerable TrackedObjectFields = new ValueDropdownList<string>()
    //{
    //  { "label" },
    //  { "id" },
    //  { "classId" },
    //  { "lastUpdate" },
    //  { "color" }
    //};


    public int fontsize = 7;

    public PropertyItem()
    {
        contentType = ContentType.Property;
    }
}
