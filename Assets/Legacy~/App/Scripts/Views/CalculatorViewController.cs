using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CalculatorViewController : MonoBehaviour
{
    public Transform ControlsUIParent;

    public TMP_Text EquationDisplay;
    public TMP_Text AnswerDisplay;
    public TMP_Text HistoryDisplay;

    private Queue<string> History;
    private string Equation;
    private string Answer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //set up voice commands
    Action disposeVoice;

    private void OnEnable()
    {
        SetupVoiceCommands();
    }

    private void OnDisable()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    private void SetupVoiceCommands()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(new Dictionary<string, Action>()
        {
            {"hide calculator", () => HideCalculator() },
        });
    }

    public void NumberPressed(string value)
    {
        Debug.Log("Button Pressed");
        Debug.Log(value);
        if(value == "." & Equation.Contains("."))
        {
            return;
        }
        Equation = Equation + value;
        EquationDisplay.text = Equation;
    }

    public void HideCalculator()
    {
        this.gameObject.SetActive(false);
    }

    public void ShowCalculator()
    {
        this.gameObject.SetActive(true);
    }
}
