using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeechRecognitionManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speechRecognitionStatusText;
    [SerializeField] private TextMeshProUGUI speechRecognitionResultText;

    public void OnSpeechRecognitionAuthorized(string result)
    {
        // Handle the speech recognition authorization result
        Debug.Log("Speech authorization granted");
        speechRecognitionResultText.text = result;
    }

    public void OnSpeechRecognitonUnauthorized(string result)
    {
        // Handle the speech recognition authorization result
        Debug.LogError("Speech authorization denied");
        speechRecognitionResultText.text = result;
    }

    public void OnSpeechRecognitionResult(string result)
    {
        // Handle the speech recognition result
        Debug.Log("Transcription: " + result);
        speechRecognitionResultText.text = result;
    }

    public void OnSpeechRecognitionError(string error)
    {
        // Handle the speech recognition error
        Debug.LogError("Error: " + error);
        speechRecognitionResultText.text = error;
    }
}
