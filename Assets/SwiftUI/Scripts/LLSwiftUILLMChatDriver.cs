using UnityEngine;
using System.Runtime.InteropServices;

public class LLSwiftUILLMChatDriver : MonoBehaviour
{
    [SerializeField]
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable m_ChatButton;

    bool m_SwiftUIWindowOpen = false;

    [SerializeField] AnthropicEventChannel anthropicEventChannel;

    private IUserAuthProvider authProvider;

    void Awake()
    {
        authProvider = ServiceRegistry.GetService<IUserAuthProvider>();
    }

    void OnEnable()
    {
        m_ChatButton.selectEntered.AddListener(_ => ToggleChatWindow());
        anthropicEventChannel.OnResponse.AddListener(HandleResponse);
        SetNativeCallback(OnMessageReceived);
    }

    void OnDisable()
    {
        m_ChatButton.selectEntered.RemoveListener(_ => ToggleChatWindow());
        anthropicEventChannel.OnResponse.RemoveListener(HandleResponse);
    }

    void ToggleChatWindow()
    {
        if (m_SwiftUIWindowOpen)
        {
            CloseSwiftUIWindow("LLMChat");
            m_SwiftUIWindowOpen = false;
        }
        else
        {
            OpenSwiftUIWindow("LLMChat");
            m_SwiftUIWindowOpen = true;
        }
    }

    void HandleResponse(string response)
    {
        if (!string.IsNullOrWhiteSpace(response))
        {
            SendMessageToSwiftUI(response);
        }
    }

    [AOT.MonoPInvokeCallback(typeof(CallbackDelegate))]
    static void OnMessageReceived(string message)
    {
        var driver = UnityEngine.Object.FindObjectOfType<LLSwiftUILLMChatDriver>();
        if (driver != null)
        {
            driver.HandleMessage(message);
        }
        else
        {
            Debug.LogError("No LLSwiftUILLMChatDriver instance found");
        }
    }

    public void HandleMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        if (message.StartsWith("login:"))
        {
            string[] loginData = message.Split(':');
            if (loginData.Length == 3)
            {
                StartCoroutine(LoginCoroutine(loginData[1], loginData[2]));
            }
        }
        else if (message.StartsWith("sendMessage:"))
        {
            string query = message.Substring("sendMessage:".Length);
            if (!string.IsNullOrWhiteSpace(query))
            {
                anthropicEventChannel.RaiseQuery(query);
            }
        }
    }

    private System.Collections.IEnumerator LoginCoroutine(string email, string password)
    {
        yield return StartCoroutine(authProvider.TryAuthenticateUser(email, password));

        SendMessageToSwiftUI(authProvider.IsAuthenticated() ? "authStatus:true" : "authStatus:false");
    }

    [DllImport("__Internal")]
    private static extern void OpenSwiftUIWindow(string name);

    [DllImport("__Internal")]
    private static extern void CloseSwiftUIWindow(string name);

    [DllImport("__Internal")]
    private static extern void SendMessageToSwiftUI(string message);

    [DllImport("__Internal")]
    private static extern void SetNativeCallback(CallbackDelegate callback);

    delegate void CallbackDelegate(string command);
}