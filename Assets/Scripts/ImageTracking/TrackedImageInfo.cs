using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TrackedImageInfo : MonoBehaviour
{
    public GameObject CharucoPrefab;
    private Dictionary<string, GameObject> ArucoMarkers = new();

    [SerializeField]
    ARTrackedImageManager m_TrackedImageManager;

    void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;

    void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            // Handle added event
            ListAllImages();
            if (!ArucoMarkers.ContainsKey(newImage.referenceImage.name))
            {
                ArucoMarkers.TryAdd(newImage.referenceImage.name, Instantiate(CharucoPrefab, newImage.transform));
            }
            else
            {
                ArucoMarkers[newImage.referenceImage.name].SetActive(true);
            }
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            if(ArucoMarkers.ContainsKey(updatedImage.referenceImage.name))
            {
                ArucoMarkers[updatedImage.referenceImage.name].SetActive(true);
                ArucoMarkers[updatedImage.referenceImage.name].transform.position = updatedImage.transform.localPosition;
            }
            else
            {
                Debug.Log("updating uninstatiated tracked image");
            }
        }

        foreach (var removedImage in eventArgs.removed)
        {
            if(ArucoMarkers.ContainsKey(removedImage.referenceImage.name))
            {
                ArucoMarkers[removedImage.referenceImage.name].SetActive(false);
            }
        }
    }

    void ListAllImages()
    {
        Debug.Log(
            $"There are {m_TrackedImageManager.trackables.count} images being tracked.");

        foreach (var trackedImage in m_TrackedImageManager.trackables)
        {
            Debug.Log($"Image: {trackedImage.referenceImage.name} is at " +
                      $"{trackedImage.transform.position}");
        }
    }
}
