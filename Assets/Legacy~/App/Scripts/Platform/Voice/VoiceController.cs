using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine.Windows.Speech;

public class VoiceController : IVoiceController
{
    private ReactiveCollection<string> keywordCollection = new ReactiveCollection<string>();
    public ReactiveCollection<string> Keywords
    {
        get 
        {
            return keywordCollection;
        }
    }

    /// Returns an action to execute to stop listening
    public Action Listen(Dictionary<string, Action> keywords)
    {
        var keywordLst = keywords.Keys.ToArray();

        if (keywords.Count == 0)
        {
            return () =>
            {
            };
        }

        var keywordRecognizer = new KeywordRecognizer(keywordLst);

        foreach (var keyword in keywordLst)
        {
            keywordCollection.Add(keyword);
        }

        keywordRecognizer.OnPhraseRecognized += args =>
        {
            ServiceRegistry.Logger.Log("Voice recognized \"" + args.text + "\" with confidence " + args.confidence);

            Action keywordAction;
            if (keywords.TryGetValue(args.text, out keywordAction))
            {
                keywordAction.Invoke();
            }
        };

        keywordRecognizer.Start();
        

        return () =>
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();

            foreach (var keyword in keywordRecognizer.Keywords)
            {
                keywordCollection.Remove(keyword);
            }
            keywordRecognizer = null;
        };
    }
}