using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whisper;
using Whisper.Utils;
using System.Linq;

/// <summary>
/// Creates a whisperstream and start listening for keywords in the dictionary
/// Invokes action in response to found keyword
/// Stops the stream if no keywords left for recognition
/// New dictionaries can be added with the Listen method
/// </summary>
public class SpeechRecognizer : MonoBehaviour
{
    public static SpeechRecognizer Instance;

    WhisperManager _whisper;
    WhisperStream _stream;
    MicrophoneRecord _microphoneRecord;

    public Dictionary<string, Action> Keywords;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple SpeechRecognizer instances detected. Destroying duplicate (newest).");
            DestroyImmediate(gameObject);
        }

        _whisper = GetComponent<WhisperManager>();
        _microphoneRecord = GetComponent<MicrophoneRecord>();

        Keywords = new Dictionary<string, Action>();
    }

    private async void Start()
    {
        _stream = await _whisper.CreateStream(_microphoneRecord);

        _stream.OnSegmentFinished += Segment =>
        {
            var result = Segment.Result;
            // Remove special characters and convert to lowercase
            var recognizedWords = Regex.Replace(result, @"[^\w\d\s]","").ToLower().Split(' ');

            //Debug.Log("Number of Keywords: " + Keywords.Count);
            if(result == null || Keywords.Count == 0)
            {
                Debug.LogWarning("No keywords provided. Cannot listen for nothing.");
                return;
            }

            // Check if recognized words match any keywords
            foreach(var word in recognizedWords)
            {
                Action action;
                if(Keywords.TryGetValue(word, out action))
                {
                    Debug.Log("Keyword recognized: " + word);
                    action.Invoke();
                }
            }
        };
    }

    public Action Listen(Dictionary<string, Action> newKeywords)
    {
        if(_stream == null)
        {
            Debug.LogWarning("WhisperStream not initialized. Cannot listen for keywords.");
            return () =>
            {
            };
        }
        // If no new keywords are provided, return an empty action
        if (newKeywords.Count == 0)
        {
            Debug.LogWarning("No keywords provided. Cannot listen for nothing.");
            return () =>
            {
            };
        }

        // Add new keywords to the list
        foreach(var word in newKeywords.Keys)
        {
            if (!Keywords.ContainsKey(word))
            {
                Keywords.Add(word, newKeywords[word]);
            }
            else
            {
                Debug.LogWarning("Keyword already exists: " + word);
            }
        }

        // Start listening if not already listening
        if(!_microphoneRecord.IsRecording && Keywords.Count > 0)
        {
            _microphoneRecord.StartRecord();
            _stream.StartStream();
        }
        else if(_microphoneRecord.IsRecording && Keywords.Count == 0)
        {
            _microphoneRecord.StopRecord();
            _stream.StopStream();
        }
        
        // Return an Action to remove the requested keywords
        return () =>
        {
            foreach(var word in newKeywords.Keys)
            {
                Keywords.Remove(word);
            }

            // Stop listening if no keywords are left
            if(Keywords.Count == 0 && _microphoneRecord.IsRecording)
            {
                _stream.StopStream();
                _microphoneRecord.StopRecord();
            }
        };
    }
}
