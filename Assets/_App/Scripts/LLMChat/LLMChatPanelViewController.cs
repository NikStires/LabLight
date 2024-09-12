using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

public class LLMChatPanelViewController : LLBasePanel
{
    [SerializeField] TextMeshProUGUI panelText;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] XRSimpleInteractable recordButton;
    [SerializeField] XRSimpleInteractable testButton;
    [SerializeField] XRSimpleInteractable submitButton;

    [SerializeField] AnthropicEventChannel anthropicEventChannel;

    private TouchScreenKeyboard keyboard;

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        recordButton.selectEntered.AddListener(_ => Record());
        testButton.selectEntered.AddListener(_ => Test());
        submitButton.selectEntered.AddListener(_ => Submit());
        inputField.onSubmit.AddListener(_ => Submit());
        // inputField.onSelect.AddListener(_ => keyboard = TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.Default));

        anthropicEventChannel.OnResponse.AddListener(HandleResponse);
    }

    void Update()
    {
        if(TouchScreenKeyboard.visible)
        {
            inputField.ActivateInputField();
            inputField.text = keyboard.text;
            inputField.MoveTextEnd(true);
        }
    }

    public void Record()
    {
        SpeechRecognizer.Instance.RecognizedTextHandler = HandleRecognizedText;
    }

    public void HandleRecognizedText(string recognizedText)
    {
        inputField.text = recognizedText;
        SpeechRecognizer.Instance.RecognizedTextHandler = null;
    }

    void HandleResponse(string response)
    {
        panelText.text = panelText.text + "<color=white>" + response + "\n\n";
    }

    void Test()
    {
        HandleRecognizedText("Hello, Claude!");
    }

    void Submit()
    {
        string query = inputField.text;
        inputField.text = "";
        panelText.text = panelText.text + "<color=blue>" + query + "\n\n";
        anthropicEventChannel.RaiseQuery(query);
    }
}
