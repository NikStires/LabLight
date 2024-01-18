using Sirenix.OdinInspector;

/// <summary>
/// Image
/// </summary>
public class ImageItem : ContentItem
{
    [ValueDropdown("@ProcedureExplorer.GetAllImages()")]
    public string url;

    public ImageItem()
    {
        contentType = ContentType.Image;
    }
}
