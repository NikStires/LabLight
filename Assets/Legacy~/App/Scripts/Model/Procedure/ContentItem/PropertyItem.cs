using Sirenix.OdinInspector;
using System.Collections;

/// <summary>
/// TrackedObject property as string
/// </summary>
public class PropertyItem : ContentItem
{
    [ValueDropdown("TrackedObjectFields")]
    public string propertyName;

    private static IEnumerable TrackedObjectFields = new ValueDropdownList<string>()
    {
      { "label" },
      { "id" },
      { "classId" },
      { "lastUpdate" },
      { "color" }
    };


    [HideInEditorMode]
    public int fontsize = 7;

    public PropertyItem()
    {
        contentType = ContentType.Property;
    }
}
