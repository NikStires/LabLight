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
        return LoadTextAsset("ProtocolV2/index").Select(jsonString =>
        {
            try
            {
                return Parsers.ParseProtocols(jsonString);
            }
            catch (Exception e)
            {
                ServiceRegistry.Logger.LogError("Could not create procedures " + e.ToString());
                throw;
            }
        }).ToTask();
    }

    public IObservable<ProtocolDefinition> GetOrCreateProtocolDefinition(string protocolName)
    {
        var basePath = "ProtocolV2/" + protocolName;
        var systemIoPath = @"Assets/Resources/" + basePath;

        return LoadTextAsset(basePath + "/index").Select(jsonString =>
        {
            try
            {
                var protocol = Parsers.ParseProtocol(jsonString);

                // if (protocol.version < 9)
                // {
                //     UpdateProtocolVersion(protocol);
                // }

                // Set basepath for media to the same path
                protocol.mediaBasePath = basePath;

                return protocol;
            }
            catch (Exception e)
            {
                ServiceRegistry.Logger.LogError("Parsing protocol definition " + e.ToString());
                throw;
            }
        });
    }

    // public void DeleteProtocolDefinition(string protocolName)
    // {
    //     string indexPath;
    //     #if UNITY_EDITOR
    //         indexPath = Application.dataPath + "/Resources/Protocol/index.json";
    //     #else
    //         indexPath = Application.persistentDataPath + "/Resources/Protocol/index.json";
    //     #endif

    //     string jsonString = File.ReadAllText(indexPath);
    //     Debug.Log(jsonString);
    //     var protocols = JsonConvert.DeserializeObject<List<ProtocolDescriptor>>(jsonString);
    //     var protocolToDelete = protocols.Find(p => p.title == protocolName);
    //     protocols.Remove(protocolToDelete);
    //     string updatedIndex = JsonConvert.SerializeObject(protocols, Formatting.Indented);
    //     Debug.Log(updatedIndex);
    //     File.WriteAllText(indexPath, updatedIndex);
    // }

    /// <summary>
    /// Version 1 switched to automatic serialization/deserialization withouth manual parsing
    /// Version 2 introduces Containers with ContentItems, SlideArdDefinitions and LabelArDefinitions are obsolete and need to be replaced with containers and content 
    /// Version 3 introduces SoundItems that replace the SoundArDefinitions
    /// Version 4 replaces font size with enumeration for header or block 
    /// Version 5 renamed action to arDefinitionType, added globalArDefinitions array, removed frame from ArDefinition; everything is now assumed in Charuco frame, but behaviour depends on targetId (eg. container without target goes to slide frame)
    /// Version 6 model, prefab name is now renamed to url to have the same structure as content items
    /// Version 7 propertyitem, new contentitem that shows a trackedobject property as string
    /// Version 8 removed target from ArDefinition and replaced with condition to handle more flexible visualization conditions
    /// Version 9 all ArDefinitions are now global at the procedureDef level, removed ArDefinitions from steps and checkItems, seperated ArDefintions and ContentItems
    /// 
    /// LabelARDefitions are converted to containers with TextItem
    /// SlideARDefinitions are converted to containers with TextItems, Images and Videos where applicable
    /// </summary>
    /// <param name="protocol"></param>
    // private static void UpdateProtocolVersion(ProtocolDefinition protocol)
    // {
    //     // Convert to version 9 content
    //     protocol.version = 9;

    //     Debug.Log("Updating '" + protocol.title + "'  to file version " + protocol.version);

    //     var newList = new List<ArDefinition>();
    //     UpdateArDefinitions(protocol.globalArElements, newList);
    //     protocol.globalArElements = newList;
    // }

    // private static void UpdateArDefinitions(List<ArDefinition> oldList, List<ArDefinition> newList)
    // {
    //     foreach (var ar in oldList)
    //     {
    //         ArDefinition updatedItem = ar;

    //         var containerDef = ar as ContainerArDefinition;
    //         if (containerDef != null)
    //         {
    //             foreach (var content in containerDef.layout.contentItems)
    //             {
    //                 var textItem = content as TextItem;
    //                 if (textItem != null)
    //                 {
    //                     textItem.textType = (textItem.fontsize < 10) ? TextType.Block : TextType.Header;
    //                 }
    //             }
    //         }

    //         newList.Add(updatedItem);
    //     }
    // }

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