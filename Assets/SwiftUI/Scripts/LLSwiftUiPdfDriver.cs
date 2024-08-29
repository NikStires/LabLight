using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LLSwiftUiPdfDriver : MonoBehaviour
{
    [SerializeField]
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable m_Button;

    bool m_SwiftUIWindowOpen = false;

    void OnEnable()
    {
        m_Button.selectEntered.AddListener(_ => TogglePDFWindow());
    }

    void OnDisable()
    {
        CloseSwiftUIWindow("PDF");
    }

    void Start()
    {
        if(string.IsNullOrEmpty(ProtocolState.procedureDef.pdfPath))
        {
            m_Button.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = Color.gray;
            m_Button.enabled = false;
        }
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
            if(string.IsNullOrEmpty(ProtocolState.procedureDef.pdfPath))
            {
                return;
            }
            OpenSwiftPdfWindow(Path.GetFileNameWithoutExtension(ProtocolState.procedureDef.pdfPath));
            m_SwiftUIWindowOpen = true;
        }
    }

    #if UNITY_VISIONOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void OpenSwiftPdfWindow(string pdfUrlString);
    [DllImport("__Internal")]
    static extern void CloseSwiftUIWindow(string name);
    #else
    static void OpenSwiftPdfWindow(string pdfUrlString) {}
    static void CloseSwiftUIWindow(string name) {}
    #endif
}
