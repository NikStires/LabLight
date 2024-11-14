using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

/// <summary>
/// Image content item controller
/// </summary>
public class ImageController : ContentController<ContentItem>
{
    public Image Image;
    private IDisposable downloadSubscription;

    public override ContentItem ContentItem
    {
        get => base.ContentItem;
        set
        {
            base.ContentItem = value;
            UpdateView();
        }
    }

    private void OnDisable()
    {
        // Cancel previous download
        downloadSubscription?.Dispose();
        downloadSubscription = null;
    }

    private void UpdateView()
    {
        // Cancel previous download
        downloadSubscription?.Dispose();
        downloadSubscription = null;

        Image.enabled = false;

        // Start new download using GetContentItem
        downloadSubscription = ServiceRegistry.GetService<IMediaProvider>().GetContentItem(ContentItem).Subscribe(content =>
        {
            Sprite sprite = content as Sprite;
            if (sprite == null)
            {
                ServiceRegistry.Logger.LogError("Content is not a Sprite.");
                return;
            }

            Image.sprite = sprite;
            Image.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, transform.parent.GetComponent<RectTransform>().rect.width);
            Image.enabled = true;

            var fitter = this.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                var ratio = (float)Image.sprite.rect.width / (float)Image.sprite.rect.height;
                fitter.aspectRatio = ratio;
            }
        }, (e) =>
        {
            ServiceRegistry.Logger.LogError("Could not load sprite content. " + e.ToString());
        });
    }
}