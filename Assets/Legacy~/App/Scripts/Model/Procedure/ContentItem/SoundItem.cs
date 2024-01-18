using Sirenix.OdinInspector;

/// <summary>
/// Playable sound
/// </summary>
public class SoundItem : ContentItem
{
    [ValueDropdown("@ProcedureExplorer.GetAllSounds()")]
    public string url;

    public SoundItem()
    {
        contentType = ContentType.Sound;
    }
}

