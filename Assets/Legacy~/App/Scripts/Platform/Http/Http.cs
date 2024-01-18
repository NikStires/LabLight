using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using System.Threading;
using UnityEngine.Video;
using System.Collections.Generic;

public class HttpImpl : IHttp, IMediaProvider
{
    public IObservable<string> Get(string url)
    {
        return Observable.FromCoroutine<string>((observer, cancellation) => CoGet(url, observer, cancellation));
    }

    private static IEnumerator CoGet(string url, IObserver<string> observer, CancellationToken cancel)
    {
        using (var www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (cancel.IsCancellationRequested)
            {
                yield break;
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                observer.OnError(new Exception(www.error));
                yield break;
            }

            observer.OnNext(www.downloadHandler.text);
            observer.OnCompleted();
        }
    }

    /// Remember to `.Subscribe()` to fire operation
    public IObservable<string> Post(string url, string data)
    {
        return Observable.FromCoroutine<string>((observer, cancellation) => CoPostString(url, data, observer, cancellation));
    }

    private static IEnumerator CoPostString(string url, string data, IObserver<string> observer, CancellationToken cancel)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(data);
        using (var www = UnityWebRequest.Put(url, bytes))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            yield return www.SendWebRequest();

            if (cancel.IsCancellationRequested)
            {
                yield break;
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                observer.OnError(new Exception(www.error));
                yield break;
            }

            observer.OnNext(www.downloadHandler.text);
            observer.OnCompleted();
        }
    }

    /// Remember to `.Subscribe()` to fire operation
    public IObservable<string> PostJson<T>(string url, T data)
    {
        return Observable.FromCoroutine<string>((observer, cancellation) => CoPostJson(url, data, observer, cancellation));
    }

    private static IEnumerator CoPostJson<T>(string url, T data, IObserver<string> observer, CancellationToken cancel)
    {
        var jsonString = JsonUtility.ToJson(data);
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
        using (var www = UnityWebRequest.Put(url, bytes))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            www.method = UnityWebRequest.kHttpVerbPOST;

            yield return www.SendWebRequest();

            if (cancel.IsCancellationRequested)
            {
                yield break;
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                observer.OnError(new Exception(www.error));
                yield break;
            }

            observer.OnNext(www.downloadHandler.text);
            observer.OnCompleted();
        }
    }

    public IObservable<Texture2D> GetImage(string mediaPath)
    {
        var url = Config.GetResourcePath(mediaPath);
        return Observable.FromCoroutine<Texture2D>((observer, cancellation) => CoGetImage(url, observer, cancellation));
    }

    private static IEnumerator CoGetImage(string url, IObserver<Texture2D> observer, CancellationToken cancel)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (cancel.IsCancellationRequested)
            {
                yield break;
            }

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                observer.OnError(new Exception(uwr.error));
                yield break;
            }

            observer.OnNext(DownloadHandlerTexture.GetContent(uwr));
            observer.OnCompleted();
        }
    }

    public IObservable<AudioClip> GetSound(string mediaPath)
    {
        var url = Config.GetResourcePath(mediaPath);
        return Observable.FromCoroutine<AudioClip>((observer, cancellation) => CoGetSound(url, observer, cancellation));
    }

    private static IEnumerator CoGetSound(string url, IObserver<AudioClip> observer, CancellationToken cancel)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS))
        {
            yield return www.SendWebRequest();

            if (cancel.IsCancellationRequested)
            {
                yield break;
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                observer.OnError(new Exception(www.error));
                yield break;
            }

            observer.OnNext(DownloadHandlerAudioClip.GetContent(www));
            observer.OnCompleted();
        }
    }

    public IObservable<VideoClip> GetVideo(string mediaPath)
    {
        throw new NotImplementedException();
    }

    public IObservable<GameObject> GetPrefab(string mediaPath)
    {
        throw new NotImplementedException();
    }

    public IObservable<List<MediaDescriptor>> GetMediaList(string mediaBasePath)
    {
        throw new NotImplementedException();
    }

    public IObservable<Sprite> GetSprite(string mediaPath)
    {
        throw new NotImplementedException();
    }
}