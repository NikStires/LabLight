using System;
using UnityEngine;

public interface IHttp
{
    IObservable<string> Get(string url);
    IObservable<string> Post(string url, string data);
    IObservable<string> PostJson<T>(string url, T data);
}