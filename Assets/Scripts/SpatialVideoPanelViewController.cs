using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.PolySpatial;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(VisionOSVideoComponent))]
public class SpatialVideoPanelViewController : LLBasePanel
{
    VisionOSVideoComponent _videoComponent;
    [SerializeField] XRSimpleInteractable _playPauseInteractable;
    [SerializeField] Image _playPauseImageComponent;
    [SerializeField] Sprite _playSprite;
    [SerializeField] Sprite _pauseSprite;

    [SerializeField] XRSimpleInteractable _muteInteractable;
    [SerializeField] Image _muteImageComponent;
    [SerializeField] Sprite _muteSprite;
    [SerializeField] Sprite _unmuteSprite;

    // Start is called before the first frame update
    void Start()
    {
        _videoComponent = GetComponent<VisionOSVideoComponent>();
        _playPauseInteractable.selectEntered.AddListener(_ => PlayPause());
        _muteInteractable.selectEntered.AddListener(_ => Mute());
    }

    void PlayPause()
    {
        if (_videoComponent.GetState() == VisionOSVideoComponent.PlayerState.IsPlaying)
        {
            _videoComponent.Pause();
            _playPauseImageComponent.sprite = _playSprite;
        }
        else
        {
            _videoComponent.Play();
            _playPauseImageComponent.sprite = _pauseSprite;
        }
    }

    void Mute()
    {
        if (_videoComponent.GetDirectAudioMute(0))
        {
            _videoComponent.SetDirectAudioMute(0, false);
            _muteImageComponent.sprite = _unmuteSprite;
        }
        else
        {
            _videoComponent.SetDirectAudioMute(0, true);
            _muteImageComponent.sprite = _muteSprite;
        }
    }
}
