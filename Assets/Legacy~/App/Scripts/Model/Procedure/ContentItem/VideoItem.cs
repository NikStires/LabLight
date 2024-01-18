using Sirenix.OdinInspector;

/// <summary>
/// Playable video
/// </summary>
public class VideoItem : ContentItem
{
    [ValueDropdown("@ProcedureExplorer.GetAllVideos()")]
    public string url;

    public VideoItem()
    {
        contentType = ContentType.Video;
    }
}
