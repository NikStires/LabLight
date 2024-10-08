using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using Newtonsoft.Json;

public class LLMChatController : MonoBehaviour
{
    private string apiUrl = "https://2fdv197i13.execute-api.us-east-1.amazonaws.com/dev/message";

    [SerializeField] bool useTestCredentials;
    [SerializeField] string testUsername;
    [SerializeField] string testPassword;

    void Start()
    {
        AnthropicEventChannel.Instance.OnQuery.AddListener(QueryClaudeWithString);
        if(!ServiceRegistry.GetService<IUserAuthProvider>().IsAuthenticated())
        {
            Debug.Log("User is not authenticated. Please log in.");
            if(useTestCredentials)
            {
                Debug.Log("Trying to authenticate with test credentials...");
                StartCoroutine(ServiceRegistry.GetService<IUserAuthProvider>().TryAuthenticateUser(testUsername, testPassword));
            }
        }
    }

    // Method to query Claude with a string prompt
    public void QueryClaudeWithString(string query)
    {
        if(!ServiceRegistry.GetService<IUserAuthProvider>().IsAuthenticated())
        {
            Debug.LogError("User is not authenticated. Cannot query Claude.");
            return;
        }
        StartCoroutine(SendQueryToClaude(query));
    }

    // Coroutine to send the query to Claude API
    IEnumerator SendQueryToClaude(string query)
    {
        string jsonBody = $@"{{
            ""query"": ""{query}""
        }}";

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            // Add the JSON body to the request
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("auth-token", ServiceRegistry.GetService<IUserAuthProvider>().TryGetIdToken()); // Include the Cognito ID token in the request

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse the outer JSON response
                var outerResponse = JsonUtility.FromJson<OuterResponse>(request.downloadHandler.text);

                // Parse the body (which is a JSON string) to extract the message
                var bodyJson = JsonUtility.FromJson<InnerBody>(outerResponse.body);
                AnthropicEventChannel.Instance.OnResponse.Invoke(bodyJson.message);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                Debug.Log("Response: " + request.downloadHandler.text);
            }
        }
    }

    // Define classes to match the structure of the JSON response
    [System.Serializable]
    public class OuterResponse
    {
        public int statusCode;
        public string body; // This is a stringified JSON
    }

    [System.Serializable]
    public class InnerBody
    {
        public string message;
    }
}
