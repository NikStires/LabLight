using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText = default;

    private ScrollRect scrollRect;

    SynchronizationContext synchronizationContext;

    public event EventHandler WindowActiveChanged;

    [SerializeField]
    [Tooltip("The maximum number of log messages that will be concatenated. Text will be reset after this to prevent clogging the rendering.")]
    private int MaxMessages = 20;
    private int currentCount;

    private void Awake()
    {
        this.synchronizationContext = SynchronizationContext.Current;

        // Cache references
        scrollRect = GetComponentInChildren<ScrollRect>();

        // Subscribe to log message events
        Application.logMessageReceivedThreaded += HandleLog;

        // Set the starting text
        debugText.text = "Debug messages will appear here.\n\n";

        currentCount = 0;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string message, string stackTrace, LogType type)
    {
        this.synchronizationContext.Post((s) =>
        {
            currentCount++;
            if (currentCount < MaxMessages || MaxMessages <= 0)
            {
                debugText.text += message + " \n";
            }
            else
            {
                // Reset debug text to prevent large amount of text
                currentCount = 0;
                debugText.text = message + " \n";
            }
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }, null);
    }

    public void Clear()
    {
        debugText.text = "Debug messages will appear here.\n\n";
    }

    public void WindowClosed()
    {
        WindowActiveChanged?.Invoke(this, new EventArgs());
    }
}

