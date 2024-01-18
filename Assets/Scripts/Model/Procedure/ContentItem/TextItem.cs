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

    public int fontsize = 7;

    public TextItem()
    {
        contentType = ContentType.Text;
    }
}
