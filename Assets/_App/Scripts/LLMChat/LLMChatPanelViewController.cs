using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

public class LLMChatPanelViewController : LLBasePanel
{
    [SerializeField] TextMeshProUGUI panelText;
    [SerializeField] XRSimpleInteractable recordButton;
    [SerializeField] XRSimpleInteractable testButton;

    [SerializeField] AnthropicEventChannel anthropicEventChannel;

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        recordButton.selectEntered.AddListener(_ => Record());
        testButton.selectEntered.AddListener(_ => Test());
        anthropicEventChannel.OnResponse.AddListener(HandleResponse);
    }

    public void Record()
    {
        SpeechRecognizer.Instance.RecognizedTextHandler = HandleRecognizedText;
    }

    public void HandleRecognizedText(string recognizedText)
    {
        panelText.text = panelText.text + "<color=blue>" + recognizedText + "\n\n";
        SpeechRecognizer.Instance.RecognizedTextHandler = null;

        anthropicEventChannel.RaiseQuery(recognizedText);
    }

    void HandleResponse(string response)
    {
        panelText.text = panelText.text + "<color=white>" + response + "\n\n";
    }

    void Test()
    {
        HandleRecognizedText("Hello, Claude!");
    }
}
