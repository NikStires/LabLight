using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class LLSwiftUiSafariDriver : MonoBehaviour
{
    [SerializeField] UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable m_Button;

    void OnEnable()
    {
        m_Button.selectEntered.AddListener(_ => ToggleSafariWindow());
    }

    void ToggleSafariWindow()
    {
        OpenSwiftSafariWindow("http://172.16.0.134:8080/");
    }

    #if UNITY_VISIONOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void OpenSwiftSafariWindow(string urlString);
    #else
    static void OpenSwiftSafariWindow(string urlString) {}
    #endif
}
