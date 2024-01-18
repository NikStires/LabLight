using System.Collections.Generic;
using UniRx;
using UnityEngine;

/// <summary>
/// Panel with overview of all current keywords that are listened to
///
/// This panel is more for debug purposes. 
/// It could visualize the last recognized keyword and corresponding confidence level.
/// 
/// Design wise it is better to attach the voice keywords to the UI counterpart with a see-it-say-it label
/// </summary>
public class VoiceCommandViewController : MonoBehaviour
{
    public TextController TextItem;
    public Transform contentFrame;
    private Dictionary<string, GameObject> keywordGameObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        var voiceController = ServiceRegistry.GetService<IVoiceController>();
        if (voiceController != null)
        {
            // Initialize with already active keywords
            foreach (var kw in voiceController.Keywords)
            {
                var textController = Instantiate(TextItem, contentFrame);
                textController.ContentItem = new TextItem
                {
                    text = kw
                };
            }

            // Handle dynamic addition
            voiceController.Keywords.ObserveAdd().Subscribe(x =>
            {
                var textController = Instantiate(TextItem, contentFrame);
                textController.ContentItem = new TextItem
                {
                    text = x.Value
                };
                keywordGameObjects[x.Value] = textController.gameObject;
            }).AddTo(this);

            // Handle dynamic removal
            voiceController.Keywords.ObserveRemove().Subscribe(x =>
            {
                GameObject go;
                if (keywordGameObjects.TryGetValue(x.Value, out go))
                {
                    GameObject.Destroy(go);
                    keywordGameObjects.Remove(x.Value);
                }
            }).AddTo(this);
        }
    }
}
