using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reset scrollrect to top position when the height of the specified content changes
/// Feels a bit like a hacky solution, but it takes into account delay loaded images/videos etc.
/// </summary>
public class ScrollReset : MonoBehaviour
{
    private ScrollRect scrollRect;
    public RectTransform content;


    // Start is called before the first frame update
    void Awake()
    {
        scrollRect = this.GetComponent<ScrollRect>();
    }

    float lastHeight;


    void Update()
    {
        if (content.rect.height != lastHeight)
        {
            lastHeight = content.rect.height;
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
    }
}
