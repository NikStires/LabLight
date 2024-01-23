using UnityEngine;

public enum TextType
{
    Header = 1,
    Block = 2,
}

/// <summary>
/// Multiple lines of text
/// </summary>
public class TextItem : ContentItem
{
    public TextType textType = TextType.Block;

    [Multiline(5)]
    public string text;

    public float fontsize = 0.15f;

    public TextItem()
    {
        contentType = ContentType.Text;
    }
}
