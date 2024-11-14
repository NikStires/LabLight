using System;
using UnityEngine;
using UnityEngine.Video;
using UniRx;
using UnityEngine.UI;

/// <summary>
/// Video content item controller
/// </summary>
public class VideoController : ContentController<ContentItem>
{
    public VideoPlayer videoPlayer;
    public RawImage videoTargetImage;
    public GameObject loadingIndicator;
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

        // Show loading indicator
        videoTargetImage.enabled = false;
        loadingIndicator.SetActive(true);

        // Start new download using GetContentItem
        downloadSubscription = ServiceRegistry.GetService<IMediaProvider>().GetContentItem(ContentItem).Subscribe(content =>
        {
            VideoClip clip = content as VideoClip;
            if (clip == null)
            {
                ServiceRegistry.Logger.LogError("Content is not a VideoClip.");
                return;
            }

            videoPlayer.clip = clip;
            videoTargetImage.enabled = true;
            loadingIndicator.SetActive(false);
            videoPlayer.targetTexture = new RenderTexture((int)clip.width, (int)clip.height, 1);
            videoTargetImage.texture = videoPlayer.targetTexture;

            var fitter = this.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                var ratio = (float)clip.width / (float)clip.height;
                fitter.aspectRatio = ratio;
            }
        }, (e) =>
        {
            ServiceRegistry.Logger.LogError("Could not load video content. " + e.ToString());
        });
    }
}