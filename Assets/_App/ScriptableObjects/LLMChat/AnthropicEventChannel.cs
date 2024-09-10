using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StringEvent : UnityEvent<string> { }

[CreateAssetMenu(menuName = "ScriptableObjects/AnthropicEventChannel")]
public class AnthropicEventChannel : ScriptableObject
{
    public StringEvent OnQuery = new StringEvent();
    public StringEvent OnResponse = new StringEvent();

    public void RaiseQuery(string query)
    {
        OnQuery?.Invoke(query);
    }
    public void RaiseResponse(string response)
    {
        OnResponse?.Invoke(response);
    }
}
