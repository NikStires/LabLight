using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.PolySpatial;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Video;
using UniRx;

[RequireComponent(typeof(VisionOSVideoComponent))]
public class SpatialVideoPanelViewController : LLBasePanel
{
    [SerializeField] GameObject _view;
    VisionOSVideoComponent _videoComponent;
    public NamedVideoClip[] videos;
    public Dictionary<string, VideoClip> videoDictionary;
    private string currentVideoItem;

    [Header("Video Controls")]
    [SerializeField] XRSimpleInteractable _playPauseInteractable;
    [SerializeField] Image _playPauseImageComponent;
    [SerializeField] Sprite _playSprite;
    [SerializeField] Sprite _pauseSprite;

    [SerializeField] XRSimpleInteractable _muteInteractable;
    [SerializeField] Image _muteImageComponent;
    [SerializeField] Sprite _muteSprite;
    [SerializeField] Sprite _unmuteSprite;

    protected override void Awake()
    {
        base.Awake();
        
        videoDictionary = new Dictionary<string, VideoClip>();
        foreach (NamedVideoClip namedVideo in videos)
        {
            videoDictionary.Add(namedVideo.name, namedVideo.video);
        }

        _view = transform.GetChild(0).gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        _videoComponent = GetComponent<VisionOSVideoComponent>();
        _playPauseInteractable.selectEntered.AddListener(_ => PlayPause());
        _muteInteractable.selectEntered.AddListener(_ => Mute());
        ProtocolState.checklistStream.Subscribe(_ => UpdateContent()).AddTo(this);
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

    void UpdateContent()
    {
        var currentStep = ProtocolState.procedureDef.steps[ProtocolState.Step];

        //Get new content items
        VideoItem newVideoItem = null;

        //check the current check item for a video then check the step, only one video will be shown at a time
        if(currentStep.contentItems.Count > 0)
        {
            newVideoItem = (VideoItem)currentStep.contentItems.Where(x => x.contentType == ContentType.Video).FirstOrDefault();
        }
        if(currentStep.checklist != null && currentStep.checklist[ProtocolState.CheckItem].contentItems.Count > 0)
        {
            newVideoItem = (VideoItem)currentStep.checklist[ProtocolState.CheckItem].contentItems.Where(x => x.contentType == ContentType.Video).FirstOrDefault();
        }

        if(newVideoItem == null)
        {
            //if there are no video items disable view
            _view.SetActive(false);
        }
        else if(newVideoItem != null && currentVideoItem == newVideoItem.url)
        {
            //if video item is the same as the previous video item do nothing
            return;
        }
        else
        {
            //if we have a new video item then load the video
            LoadVideo(newVideoItem.url);
        }
    }

    void LoadVideo(string videoName)
    {
        if (videoDictionary.ContainsKey(videoName))
        {
            currentVideoItem = videoName;
            _videoComponent.clip = videoDictionary[videoName];
            _videoComponent.targetMaterialRenderer = _videoComponent.targetMaterialRenderer;
            _view.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Video not found: " + videoName);
        }
    }

    [Serializable]
    public struct NamedVideoClip {
        public string name;
        public VideoClip video;
    }
}
