using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using PolySpatial.Samples;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using UniRx;

public class LLSwiftUiVideoDriver : MonoBehaviour
{
    bool _swiftUIWindowOpen = false;
    string _currentVideoItem;

    void OnDisable()
    {
        CloseSwiftUIWindow("Video");
    }

    void Start()
    {
        ProtocolState.checklistStream.Subscribe(_ => UpdateContent()).AddTo(this);
    }

    void UpdateContent()
    {
        var currentStep = ProtocolState.procedureDef.steps[ProtocolState.Step];

        VideoItem newVideoItem = null;

        //check the current check step for a video then checkitem, only one video will be shown at a time
        if(currentStep.contentItems.Count > 0)
        {
            newVideoItem = (VideoItem)currentStep.contentItems.Where(x => x.contentType == ContentType.Video).FirstOrDefault();
        }
        if(currentStep.checklist != null && currentStep.checklist[ProtocolState.CheckItem].contentItems.Count > 0)
        {
            newVideoItem = (VideoItem)currentStep.checklist[ProtocolState.CheckItem].contentItems.Where(x => x.contentType == ContentType.Video).FirstOrDefault();
        }

        if(newVideoItem == null && _swiftUIWindowOpen)
        {
            //if there are no video items disable view
            CloseSwiftUIWindow("Video");
            _swiftUIWindowOpen = false;
        }
        else if(newVideoItem != null && _currentVideoItem == newVideoItem.url)
        {
            //if video item is the same as the previous video item do nothing
            return;
        }
        else
        {
            Debug.Log("######LABLIGHT Requesting swift video window open: " + newVideoItem.url);
            _currentVideoItem = newVideoItem.url;
            //if we have a new video item then load the video
            if(!_swiftUIWindowOpen)
            {
                OpenSwiftVideoWindow(newVideoItem.url);
                _swiftUIWindowOpen = true;
            }
            else
            {
                Debug.Log("######LABLIGHT Requesting swift video swap: " + newVideoItem.url);
                //SwapVideo(newVideoItem.url);
            }
        }
    }

    #if UNITY_VISIONOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void OpenSwiftVideoWindow(string videoTitle);
    // [DllImport("__Internal")]
    // static extern void SwapVideo(string videoTitle);
    [DllImport("__Internal")]
    static extern void CloseSwiftUIWindow(string name);
    #else
    static void OpenSwiftVideoWindow(string videoTitle) {}
    // static void SwapVideo(string videoTitle) {}
    static void CloseSwiftUIWindow(string name) {}
    #endif
}
