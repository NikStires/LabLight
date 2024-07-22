using UnityEngine;
using UnityEngine.UI;

public class UIBindings : MonoBehaviour
{
    public void RequestSpeechAuthorization() 
    {
        iOSPlugin.RequestSpeechAuthorization();
    }
    public void RequestSpeechRecognition() 
    {
        iOSPlugin.RequestSpeechRecognition();
    }
}