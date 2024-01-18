using System.Collections.Generic;
using System;
using UniRx;

public interface IVoiceController
{
    /// <summary>
    /// List of keywords that can be recognized
    /// </summary>
    ReactiveCollection<string> Keywords
    {
        get;
    }

    /// <summary>
    /// Setup a list of keywords to listen to and the actions to invoke
    /// </summary>
    /// <param name="keywords"></param>
    /// <returns></returns>
    Action Listen(Dictionary<string, Action> keywords);
}