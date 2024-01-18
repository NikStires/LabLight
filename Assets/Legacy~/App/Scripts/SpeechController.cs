using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Audio;
using UniRx;

public class SpeechController : MonoBehaviour
{
    TextToSpeech textToSpeech;
    // Start is called before the first frame update
    void Start()
    {
        textToSpeech = GetComponent<TextToSpeech>();

        ProtocolState.stepStream.Subscribe(_ => PlayAudio()).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlayAudio()
    {
        if (SessionState.TextToSpeech.Value && ProtocolState.procedureDef.Value != null && ProtocolState.procedureDef.Value.steps != null && ProtocolState.procedureDef.Value.steps[ProtocolState.Step] != null && ProtocolState.procedureDef.Value.steps[ProtocolState.Step].checklist != null)
        {
            textToSpeech.StartSpeaking(ProtocolState.procedureDef.Value.steps[ProtocolState.Step].checklist[ProtocolState.CheckItem].Text);
        }
    }
}
