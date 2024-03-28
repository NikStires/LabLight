using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using PolySpatial.Samples;
using UnityEngine;

/// <summary>
/// drives the interop with SwiftUI. It uses DllImport to access methods defined in Swift.
/// </summary>
public class LLSwiftUIDriver : MonoBehaviour
{
    [SerializeField]
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable m_Button;

    bool m_SwiftUIWindowOpen = false;

    void OnEnable()
    {
        m_Button.selectEntered.AddListener(_ => TogglePDFWindow());
        SetNativeCallback(CallbackFromNative);
    }

    void OnDisable()
    {
        SetNativeCallback(null);
        CloseSwiftUIWindow("PDF");
    }

    void TogglePDFWindow()
    {
        if (m_SwiftUIWindowOpen)
        {
            CloseSwiftUIWindow("PDF");
            m_SwiftUIWindowOpen = false;
        }
        else
        {
            //OpenSwiftPdfWindow("https://giraffeconservation.org/wp-content/uploads/2016/02/GCF-Giraffe-booklet-2017-LR-spreads-c-GCF.compressed.pdf");
            OpenSwiftPdfWindow("giraffe");
            m_SwiftUIWindowOpen = true;
        }
    }

    delegate void CallbackDelegate(string command);

    // This attribute is required for methods that are going to be called from native code
    // via a function pointer.
    [MonoPInvokeCallback(typeof(CallbackDelegate))]
    static void CallbackFromNative(string command)
    {
        Debug.Log("Callback from native: " + command);

        // This could be stored in a static field or a singleton.
        // If you need to deal with multiple windows and need to distinguish between them,
        // you could add an ID to this callback and use that to distinguish windows.
    }

    #if UNITY_VISIONOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void SetNativeCallback(CallbackDelegate callback);

    [DllImport("__Internal")]
    static extern void OpenSwiftUIWindow(string name);

    [DllImport("__Internal")]
    static extern void CloseSwiftUIWindow(string name);

    [DllImport("__Internal")]
    static extern void OpenSwiftPdfWindow(string pdfUrlString);
    #else
    static void SetNativeCallback(CallbackDelegate callback) {}
    static void OpenSwiftUIWindow(string name) {}
    static void CloseSwiftUIWindow(string name) {}
    static void OpenSwiftPdfWindow(string pdfUrlString) {}
    #endif

}
