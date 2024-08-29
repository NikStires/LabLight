using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

/// <summary>
/// drives the interop with SwiftUI. It uses DllImport to access methods defined in Swift.
/// </summary>
public class LLSwiftUIDriver : MonoBehaviour
{
    [SerializeField]
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable m_Button;

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
    #else
    static void SetNativeCallback(CallbackDelegate callback) {}
    static void OpenSwiftUIWindow(string name) {}
    static void CloseSwiftUIWindow(string name) {}
    #endif

}
