using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Handles objects that use image-based tracking. When successfully tracked, 
/// locks the object and signals the appropriate channels.
/// </summary>
public class ImageTrackingObjectManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Image manager on the AR Session Origin")]
    ARTrackedImageManager m_ImageManager;

    ArObjectViewController m_objectToLock;

    public ImageTrackingEventChannel imageTrackingEventChannel;

    private void OnEnable()
    {
        imageTrackingEventChannel.SetImageTrackedObject.AddListener(HandleImageTrackedObject);
        m_ImageManager.trackablesChanged.AddListener(ImageManagerOnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        imageTrackingEventChannel.SetImageTrackedObject.RemoveListener(HandleImageTrackedObject);
        m_ImageManager.trackablesChanged.RemoveListener(ImageManagerOnTrackedImagesChanged);    
    }

    public void HandleImageTrackedObject(GameObject obj)
    {
        m_objectToLock = obj.GetComponent<ArObjectViewController>();
        if (m_objectToLock == null)
        {
            Debug.LogWarning($"HandleImageTrackedObject: No ArObjectViewController on {obj.name}.");
            return;
        }
    }

    void ImageManagerOnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if(m_objectToLock == null)
        {
            return;
        }
        // added, spawn prefab
        foreach (var image in eventArgs.added)
        {
            m_objectToLock.transform.position = image.transform.position;
            m_objectToLock.transform.rotation = image.transform.rotation;
            m_objectToLock.gameObject.SetActive(true);
            Debug.Log($"{m_objectToLock.ObjectName} successfully image-tracked and locked.");
            m_objectToLock = null;
            ProtocolState.Instance.LockingTriggered.Value = true;
            imageTrackingEventChannel.OnCurrentPrefabLocked();
        }
        foreach (var image in eventArgs.updated)
        {
            m_objectToLock.transform.position = image.transform.position;
            m_objectToLock.transform.rotation = image.transform.rotation;
            m_objectToLock.gameObject.SetActive(true);
            Debug.Log($"{m_objectToLock.ObjectName} successfully image-tracked and locked.");
            m_objectToLock = null;
            ProtocolState.Instance.LockingTriggered.Value = true;
            imageTrackingEventChannel.OnCurrentPrefabLocked();
        }
    }
} 