using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Originally known as Api 
///  
/// Loads data from Resources folder (builtin readonly Unity folder)
/// 
/// Implements IDataProvider interface for accessing available procedures and updating runtime state
/// Implements IMediaProvider interface for accessing images, sound, videos and prefabs
/// </summary>
public class ResourceFileDataProvider : IProtocolDataProvider, IMediaProvider
{
    public Task<List<ProtocolDescriptor>> GetProtocolList()
    {
        try
        {
            var protocolJsonFiles = new List<string>();
            var protocolsPath = "ProtocolV2/";

            // Get all JSON files recursively from Resources/ProtocolV2 folder
            var resourceFiles = Resources.LoadAll(protocolsPath, typeof(TextAsset));
            foreach (var file in resourceFiles)
            {
                var textAsset = file as TextAsset;
                if (textAsset != null)
                {
                    Debug.Log($"Found protocol file: {textAsset.name}");
                    protocolJsonFiles.Add(textAsset.text);
                }
            }

            if (protocolJsonFiles.Count == 0)
            {
                Debug.LogWarning($"No files found in Resources/{protocolsPath}");
            }

            return Task.FromResult(Parsers.ParseProtocolDescriptions(protocolJsonFiles));
        }
        catch (Exception e)
        {
            ServiceRegistry.Logger.LogError("Could not create procedures " + e.ToString());
            throw;
        }
    }

    public IObservable<ProtocolDefinition> GetOrCreateProtocolDefinition(ProtocolDescriptor protocolDescriptor)
    {
        return Observable.Create<ProtocolDefinition>(observer =>
        {
            try
            {
                var allProtocolFiles = Resources.LoadAll<TextAsset>("ProtocolV2");
                foreach (var file in allProtocolFiles)
                {
                    if (file.name == "index")
                    {
                        try
                        {
                            var candidateProtocol = Parsers.ParseProtocol(file.text);
                            if (candidateProtocol.title == protocolDescriptor.title &&
                                candidateProtocol.description == protocolDescriptor.description &&
                                candidateProtocol.version == protocolDescriptor.version)
                            {
                                //NOTE: I DONT THINK WE USE THIS ANYMORE? NS
                                // var relativePath = AssetDatabase.GetAssetPath(file);
                                // var protocolFolder = Path.GetDirectoryName(relativePath)
                                //     .Replace("Assets/Resources/", "")
                                //     .Replace("\\", "/");
                                // candidateProtocol.mediaBasePath = protocolFolder;
                                
                                observer.OnNext(candidateProtocol);
                                observer.OnCompleted();
                                return Disposable.Empty;
                            }
                        }
                        catch
                        {
                            // Skip files that can't be parsed
                            continue;
                        }
                    }
                }

                observer.OnError(new Exception($"No matching protocol found for {protocolDescriptor.title}"));
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }

            return Disposable.Empty;
        });
    }

    /// <summary>
    /// Some jumping through hoops to match the HttpDataProvider way of working
    /// Async loading returns an observable immediately, and the listening is triggered when that observable is updated asynchonously
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public IObservable<string> LoadTextAsset(string url)
    {
        return Observable.FromCoroutine<string>((observer, cancellation) => LoadTextAssetCoroutine(url, observer, cancellation));
    }

    /// <summary>
    /// Async loading from resources
    /// </summary>
    /// <param name="url"></param>
    /// <param name="observer"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    private static IEnumerator LoadTextAssetCoroutine(string url, IObserver<string> observer, CancellationToken cancel)
    {
        // First try direct loading
        ResourceRequest resourceRequest = Resources.LoadAsync<TextAsset>(url);
        yield return new WaitUntil(() => resourceRequest.isDone);

        if (cancel.IsCancellationRequested)
        {
            yield break;
        }

        TextAsset textAsset = resourceRequest.asset as TextAsset;

        // If direct loading failed, search through all Resources
        if (textAsset == null)
        {
            string fileName = Path.GetFileName(url);
            TextAsset[] allAssets = Resources.LoadAll<TextAsset>("");

            foreach (var asset in allAssets)
            {
                if (asset.name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    textAsset = asset;
                    break;
                }
            }
        }

        if (textAsset == null || string.IsNullOrEmpty(textAsset.text))
        {
            observer.OnError(new Exception("Error loading TextAsset from url: " + url));
            yield break;
        }

        observer.OnNext(textAsset.text);
        observer.OnCompleted();
    }

    public IObservable<Texture2D> GetImage(string url)
    {
        Debug.Log("GetImage " + url);
        return Observable.FromCoroutine<Texture2D>((observer, cancellation) => CoGetImage(url, observer, cancellation));
    }

    private static IEnumerator CoGetImage(string url, IObserver<Texture2D> observer, CancellationToken cancel)
    {
        var fileName = Path.ChangeExtension(url, null);
        ResourceRequest resourceRequest = Resources.LoadAsync<Texture2D>(fileName);
        yield return new WaitUntil(() => resourceRequest.isDone);

        if (cancel.IsCancellationRequested)
        {
            yield break;
        }

        Texture2D textureAsset = resourceRequest.asset as Texture2D;

        // If direct loading failed, search through all Resources
        if (textureAsset == null)
        {
            string searchName = Path.GetFileName(fileName);
            Texture2D[] allAssets = Resources.LoadAll<Texture2D>("");

            foreach (var asset in allAssets)
            {
                if (asset.name.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                {
                    textureAsset = asset;
                    break;
                }
            }
        }

        if (textureAsset == null)
        {
            observer.OnError(new Exception("Error loading TextureAsset from url: " + url));
            yield break;
        }

        observer.OnNext(textureAsset);
        observer.OnCompleted();
    }

    public IObservable<Sprite> GetSprite(string url)
    {
        return Observable.FromCoroutine<Sprite>((observer, cancellation) => CoGetSprite(url, observer, cancellation));
    }

    private static IEnumerator CoGetSprite(string url, IObserver<Sprite> observer, CancellationToken cancel)
    {
        var fileName = Path.ChangeExtension(url, null);
        ResourceRequest resourceRequest = Resources.LoadAsync<Sprite>(fileName);
        yield return new WaitUntil(() => resourceRequest.isDone);

        if (cancel.IsCancellationRequested)
        {
            yield break;
        }

        Sprite spriteAsset = resourceRequest.asset as Sprite;

        // If direct loading failed, search through all Resources
        if (spriteAsset == null)
        {
            string searchName = Path.GetFileName(fileName);
            Sprite[] allAssets = Resources.LoadAll<Sprite>("");

            foreach (var asset in allAssets)
            {
                if (asset.name.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                {
                    spriteAsset = asset;
                    break;
                }
            }
        }

        if (spriteAsset == null)
        {
            observer.OnError(new Exception("Error loading SpriteAsset from url: " + url));
            yield break;
        }

        observer.OnNext(spriteAsset);
        observer.OnCompleted();
    }


    public IObservable<AudioClip> GetSound(string url)
    {
        return Observable.FromCoroutine<AudioClip>((observer, cancellation) => CoGetSound(url, observer, cancellation));
    }

    private static IEnumerator CoGetSound(string url, IObserver<AudioClip> observer, CancellationToken cancel)
    {
        var fileName = Path.ChangeExtension(url, null);
        ResourceRequest resourceRequest = Resources.LoadAsync<AudioClip>(fileName);
        yield return new WaitUntil(() => resourceRequest.isDone);

        if (cancel.IsCancellationRequested)
        {
            yield break;
        }

        AudioClip audioClipAsset = resourceRequest.asset as AudioClip;

        // If direct loading failed, search through all Resources
        if (audioClipAsset == null)
        {
            string searchName = Path.GetFileName(fileName);
            AudioClip[] allAssets = Resources.LoadAll<AudioClip>("");

            foreach (var asset in allAssets)
            {
                if (asset.name.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                {
                    audioClipAsset = asset;
                    break;
                }
            }
        }

        if (audioClipAsset == null)
        {
            observer.OnError(new Exception("Error loading AudioClip from url: " + url));
            yield break;
        }

        observer.OnNext(audioClipAsset);
        observer.OnCompleted();
    }

    public IObservable<VideoClip> GetVideo(string mediaPath)
    {
        return Observable.FromCoroutine<VideoClip>((observer, cancellation) => CoGetVideo(mediaPath, observer, cancellation));
    }

    private static IEnumerator CoGetVideo(string url, IObserver<VideoClip> observer, CancellationToken cancel)
    {
        var fileName = Path.ChangeExtension(url, null);
        ResourceRequest resourceRequest = Resources.LoadAsync<VideoClip>(fileName);
        yield return new WaitUntil(() => resourceRequest.isDone);

        if (cancel.IsCancellationRequested)
        {
            yield break;
        }

        VideoClip videoClipAsset = resourceRequest.asset as VideoClip;

        // If direct loading failed, search through all Resources
        if (videoClipAsset == null)
        {
            string searchName = Path.GetFileName(fileName);
            VideoClip[] allAssets = Resources.LoadAll<VideoClip>("");

            foreach (var asset in allAssets)
            {
                if (asset.name.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                {
                    videoClipAsset = asset;
                    break;
                }
            }
        }

        if (videoClipAsset == null)
        {
            observer.OnError(new Exception("Error loading VideoClip from url: " + url));
            yield break;
        }

        observer.OnNext(videoClipAsset);
        observer.OnCompleted();
    }

    public IObservable<GameObject> GetPrefab(string mediaPath)
    {
        return Observable.FromCoroutine<GameObject>((observer, cancellation) => CoGetPrefab(mediaPath, observer, cancellation));
    }

    private static IEnumerator CoGetPrefab(string url, IObserver<GameObject> observer, CancellationToken cancel)
    {
        var fileName = Path.ChangeExtension(url, null);
        ResourceRequest resourceRequest = Resources.LoadAsync<GameObject>(fileName);
        yield return new WaitUntil(() => resourceRequest.isDone);

        if (cancel.IsCancellationRequested)
        {
            yield break;
        }

        GameObject prefabAsset = resourceRequest.asset as GameObject;

        // If direct loading failed, search through all Resources
        if (prefabAsset == null)
        {
            string searchName = Path.GetFileName(fileName);
            GameObject[] allAssets = Resources.LoadAll<GameObject>("");

            foreach (var asset in allAssets)
            {
                if (asset.name.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                {
                    prefabAsset = asset;
                    break;
                }
            }
        }

        if (prefabAsset == null)
        {
            observer.OnError(new Exception("Error loading Prefab from url: " + url));
            yield break;
        }

        observer.OnNext(prefabAsset);
        observer.OnCompleted();
    }

    /// <summary>
    /// Save protocol to resources folder (can only be done inside Unity editor)
    /// </summary>
    /// <param name="protocolName"></param>
    /// <param name="protocol"></param>
    public void SaveProtocolDefinition(string protocolName, ProtocolDefinition protocol)
    {
#if UNITY_EDITOR
        string path = "Assets/Resources/Protocol/" + protocolName;
        string filePath = path + "/index.json";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        StreamWriter writer = new StreamWriter(filePath, false);
        var output = JsonConvert.SerializeObject(protocol, Formatting.Indented, Parsers.serializerSettings);
        writer.WriteLine(output);
        writer.Close();

        AssetDatabase.ImportAsset(filePath);
#endif
    }
}