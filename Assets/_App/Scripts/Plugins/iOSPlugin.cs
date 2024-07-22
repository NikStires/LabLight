using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class iOSPlugin : MonoBehaviour
{    
    [DllImport("__Internal")]
    private static extern void _requestSpeechAuthorization();
    public static void RequestSpeechAuthorization()
    {
        #if UNITY_VISIONOS && !UNITY_EDITOR
            _requestSpeechAuthorization();
        #else
            Debug.LogWarning("Speech recognition is only available on visionOS devices");
        #endif
    }

    [DllImport("__Internal")]
    private static extern void _requestSpeechRecognition();
    public static void RequestSpeechRecognition()
    {
        #if UNITY_VISIONOS && !UNITY_EDITOR
            _requestSpeechRecognition();
        #else
            Debug.LogWarning("Speech recognition is only available on visionOS devices");
        #endif
    }
}