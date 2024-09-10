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
    // Replace with your actual API endpoint from Anthropic
    private string apiUrl = "https://api.anthropic.com/v1/messages";
    
    // Replace with your actual API key
    private string apiKey = "";

    [SerializeField] AnthropicEventChannel anthropicEventChannel;

    void Start()
    {
        anthropicEventChannel.OnQuery.AddListener(QueryClaudeWithString);
    }

    // Method to query Claude with a string prompt
    public void QueryClaudeWithString(string query)
    {
        Debug.Log("Query: " + query);
        StartCoroutine(SendQueryToClaude(query));
    }

    // Coroutine to send the query to Claude API
    IEnumerator SendQueryToClaude(string query)
    {
        // Create the request payload
        var requestData = new Dictionary<string, object>
        {
            {"model", "claude-3-5-sonnet-20240620"},  // Model name
            {"max_tokens", 1024},                    // Adjust tokens as per your needs
            {"messages", new List<Dictionary<string, string>> {
                new Dictionary<string, string> {{"role", "user"}, {"content", query}}
            }}  // User query goes inside the messages list
        };

        // Convert the dictionary to JSON string
        string jsonData = JsonConvert.SerializeObject(requestData);

        // Create a UnityWebRequest for a POST request
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            // Convert the JSON data to byte array
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            // Attach body data
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            // Receive response
            request.downloadHandler = new DownloadHandlerBuffer();

            // Set request headers
            request.SetRequestHeader("content-type", "application/json");
            request.SetRequestHeader("x-api-key", apiKey);
            request.SetRequestHeader("anthropic-version", "2023-06-01");

            // Send the request and wait for the response
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse the JSON response
                string responseText = request.downloadHandler.text;
                try
                {
                    // Parse the JSON response and access the 'content' field
                    var responseJson = JsonConvert.DeserializeObject<ClaudeAPIResponse>(responseText);

                    // Log only the content text
                    if (responseJson != null && responseJson.content != null && responseJson.content.Count > 0)
                    {
                        Debug.Log("Claude's Response Content: " + responseJson.content[0].text);
                        anthropicEventChannel.RaiseResponse(responseJson.content[0].text);
                    }
                    else
                    {
                        Debug.LogWarning("Unexpected response format or no messages found.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error parsing response JSON: " + ex.Message);
                }
            }
            else
            {
                // Log any errors
                 Debug.LogError("Error: " + request.error + "\nResponse: " + request.downloadHandler.text);
            }
        }
    }
}

// Define classes to deserialize the response JSON
[System.Serializable]
public class ClaudeAPIResponse
{
    public List<ClaudeContent> content;
    public string id;
    public string model;
    public string role;
    public string stop_reason;
    public string stop_sequence;
    public string type;
    public ClaudeUsage usage;
}

[System.Serializable]
public class ClaudeContent
{
    public string text;
    public string type;
}

[System.Serializable]
public class ClaudeUsage
{
    public int input_tokens;
    public int output_tokens;
}
