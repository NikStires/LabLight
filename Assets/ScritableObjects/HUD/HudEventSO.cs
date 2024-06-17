using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "HudEventSO", menuName = "ScriptableObjects/HudEventSO", order = 0)]
public class HudEventSO : ScriptableObject
{
    [SerializeField] public UnityEvent<string> DisplayMessage = new UnityEvent<string>();
    [SerializeField] public UnityEvent<string> DisplayWarning = new UnityEvent<string>();
    [SerializeField] public UnityEvent<string> DisplayError = new UnityEvent<string>();

    void Start()
    {
        if(DisplayMessage == null)
        {
            DisplayMessage = new UnityEvent<string>();
        }
        if(DisplayWarning == null)
        {
            DisplayWarning = new UnityEvent<string>();
        }
        if(DisplayError == null)
        {
            DisplayError = new UnityEvent<string>();
        }
    }

    public void DisplayHudMessage(string message)
    {
        DisplayMessage.Invoke(message);
    }

    public void DisplayHudWarning(string message)
    {
        DisplayWarning.Invoke(message);
    }

    public void DisplayHudError(string message)
    {
        DisplayError.Invoke(message);
    }
}
