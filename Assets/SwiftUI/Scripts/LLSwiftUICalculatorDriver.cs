using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Runtime.InteropServices;

public class LLSwiftUICalculatorDriver : MonoBehaviour
{
    [SerializeField]
    XRSimpleInteractable m_Button;

    bool m_SwiftUIWindowOpen = false;

    void OnEnable()
    {
        m_Button.selectEntered.AddListener(_ => ToggleCalculatorWindow());
    }

    void OnDisable()
    {
        CloseSwiftUIWindow("Calculator");
    }

    void ToggleCalculatorWindow()
    {
        if (m_SwiftUIWindowOpen)
        {
            CloseSwiftUIWindow("Calculator");
            m_SwiftUIWindowOpen = false;
        }
        else
        {
            OpenSwiftCalculatorWindow();
            m_SwiftUIWindowOpen = true;
        }
    }

    #if UNITY_VISIONOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void OpenSwiftCalculatorWindow();
    [DllImport("__Internal")]
    static extern void CloseSwiftUIWindow(string name);
    #else
    static void OpenSwiftCalculatorWindow() 
    {
        Debug.Log("Calculator window opened");
    }
    static void CloseSwiftUIWindow(string name) 
    {
        Debug.Log($"Closed SwiftUI window: {name}");
    }
    #endif
}