using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sound content item
/// </summary>
public class SoundController : ContentController<SoundItem>
{
    public AudioSource audioSource;
    public GameObject playerUI;
    public GameObject loadingIndicator;
    public IDisposable downloadSubscription;
    public TextMeshProUGUI Text;
    public Slider progressIndicator;

    public override SoundItem ContentItem
    {
        get => base.ContentItem;
        set
        {
            base.ContentItem = value;
            UpdateView();
        }
    }

    private void UpdateView()
    {
        var soundPath = ProtocolState.Instance.ActiveProtocol.Value.mediaBasePath + "/" + ContentItem.url;

        // Cancel previous download
        downloadSubscription?.Dispose();
        downloadSubscription = null;

        // Show loading indicator
        loadingIndicator.SetActive(true);
        playerUI.SetActive(false);

        // Start new download
        downloadSubscription = ServiceRegistry.GetService<IMediaProvider>().GetSound(soundPath).Subscribe(clip =>
        {
            if (clip == null)
            {
                return;
            }

            Text.text = ContentItem.url;
            audioSource.clip = clip;
            audioSource.Play();

            progressIndicator.minValue = 0;
            progressIndicator.maxValue = audioSource.clip.length;

            loadingIndicator.SetActive(false);
            playerUI.SetActive(true);
        }, (e) =>
        {
            ServiceRegistry.Logger.LogError("Could not load sound " + soundPath + ". " + e.ToString());
        });
    }


    private void Update()
    {
        progressIndicator.value = audioSource.time;
    }
}