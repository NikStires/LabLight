using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

/// <summary>
/// Image content item 
/// </summary>
public class ImageController : ContentController<ImageItem>
{
    public Image Image;
    public GameObject LoadingIndicator;
    private IDisposable downloadSubscription;

    public override ImageItem ContentItem
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
        var imagePath = ProtocolState.procedureDef.mediaBasePath + "/" + ContentItem.url;

        // Cancel previous download
        downloadSubscription?.Dispose();
        downloadSubscription = null;

        Image.enabled = false;
        LoadingIndicator.SetActive(true);

        // Start new download
        downloadSubscription = ServiceRegistry.GetService<IMediaProvider>().GetSprite(imagePath).Subscribe(sprite =>
        {
            if (sprite == null)
            {
                return;
            }

            Image.sprite = sprite;
            Image.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, transform.parent.GetComponent<RectTransform>().rect.width);
            Image.enabled = true;
            LoadingIndicator.SetActive(false);

            var fitter = this.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
               var ratio = (float)Image.sprite.rect.width / (float)Image.sprite.rect.height;
               fitter.aspectRatio = ratio;
            }
        }, (e) =>
        {
            ServiceRegistry.Logger.LogError("Could not load image " + imagePath + ". " + e.ToString());
        });
    }
}