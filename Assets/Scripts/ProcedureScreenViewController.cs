using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProcedureScreenViewController : MonoBehaviour
{
    public SpatialUIButton m_PreviousButton;
    public SpatialUIButton m_NextButton;

    public TextMeshProUGUI stepText;    

    private int currentStep = 0;
    private int totalSteps = 10;

    void OnEnable()
    {
        m_PreviousButton.WasPressed += PreviousStep;
        m_NextButton.WasPressed += NextStep;
    }

    void PreviousStep(string buttonText, MeshRenderer meshrenderer)
    {
        Debug.Log("previous button pressed");
        if(currentStep > 0)
        {
            currentStep--;
            stepText.text = currentStep + "/" + totalSteps;
        }
    }

    void NextStep(string buttonText, MeshRenderer meshrenderer)
    {
        Debug.Log("next button pressed");
        if(currentStep < totalSteps)
        {
            currentStep++;
            stepText.text = currentStep + "/" + totalSteps;
        }
    }
}
