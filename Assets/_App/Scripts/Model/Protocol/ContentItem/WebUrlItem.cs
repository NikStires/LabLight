using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebUrlItem : ContentItem
{
    public string url;

    public WebUrlItem()
    {
        contentType = ContentType.WebUrl;
    }
}
