using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sound content item controller
/// </summary>
public class SoundController : ContentController<ContentItem>
{
    public AudioSource audioSource;
    public GameObject playerUI;
    public GameObject loadingIndicator;
    private IDisposable downloadSubscription;
    public TextMeshProUGUI Text;
    public Slider progressIndicator;

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
        loadingIndicator.SetActive(true);
        playerUI.SetActive(false);

        // Start new download using GetContentItem
        downloadSubscription = ServiceRegistry.GetService<IMediaProvider>().GetContentItem(ContentItem).Subscribe(content =>
        {
            AudioClip clip = content as AudioClip;
            if (clip == null)
            {
                ServiceRegistry.Logger.LogError("Content is not an AudioClip.");
                return;
            }

            if (ContentItem.Properties != null && ContentItem.Properties.TryGetValue("Url", out object url))
            {
                Text.text = url.ToString();
            }

            audioSource.clip = clip;
            audioSource.Play();

            progressIndicator.minValue = 0;
            progressIndicator.maxValue = audioSource.clip.length;

            loadingIndicator.SetActive(false);
            playerUI.SetActive(true);
        }, (e) =>
        {
            ServiceRegistry.Logger.LogError("Could not load sound content. " + e.ToString());
        });
    }

    private void Update()
    {
        if (audioSource.clip != null)
        {
            progressIndicator.value = audioSource.time;
        }
    }
}