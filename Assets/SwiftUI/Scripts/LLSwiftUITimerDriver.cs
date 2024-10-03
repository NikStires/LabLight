using UnityEngine;
using UniRx;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Runtime.InteropServices;

public class LLSwiftUITimerDriver : MonoBehaviour
{
    [SerializeField]
    XRSimpleInteractable m_Button;

    bool m_SwiftUIWindowOpen = false;

    void Start()
    {
        ProtocolState.Instance.ChecklistStream.Subscribe(_ =>
        {
            if(ProtocolState.Instance.HasCurrentChecklist())
            {
                var currentCheckItem = ProtocolState.Instance.CurrentCheckItemDefinition;
                if(currentCheckItem.activateTimer)
                {
                    int timeSeconds = (currentCheckItem.hours * 60 * 60) + (currentCheckItem.minutes * 60) + currentCheckItem.seconds;
                    OpenSwiftTimerWindow(timeSeconds);
                }
            }
        });
    }

    void OnEnable()
    {
        m_Button.selectEntered.AddListener(_ => ToggleTimerWindow());
    }

    void OnDisable()
    {
        CloseSwiftUIWindow("Timer");
    }

    void ToggleTimerWindow()
    {
        if (m_SwiftUIWindowOpen)
        {
            CloseSwiftUIWindow("Timer");
            m_SwiftUIWindowOpen = false;
        }
        else
        {
            OpenSwiftTimerWindow(300); // Open with 5 minutes (300 seconds) by default
            m_SwiftUIWindowOpen = true;
        }
    }

    public void OpenTimerWithoutDuration()
    {
        if (!m_SwiftUIWindowOpen)
        {
            OpenSwiftTimerWindow(0); // Open without a predefined duration
            m_SwiftUIWindowOpen = true;
        }
    }

    #if UNITY_VISIONOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void OpenSwiftTimerWindow(int duration);
    [DllImport("__Internal")]
    static extern void CloseSwiftUIWindow(string name);
    #else
    static void OpenSwiftTimerWindow(int duration) 
    {
        Debug.Log($"Timer window opened with duration: {duration} seconds");
    }
    static void CloseSwiftUIWindow(string name) 
    {
        Debug.Log($"Closed SwiftUI window: {name}");
    }
    #endif
}