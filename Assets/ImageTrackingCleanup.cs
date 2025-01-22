using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ImageTrackingObjectManager))]
public class ImageTrackingCleanup : MonoBehaviour
{
    private ARTrackedImageManager m_ImageManager;
    
    void Start()
    {
        // Get reference to the image manager
        m_ImageManager = GetComponentInChildren<ARTrackedImageManager>();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && m_ImageManager != null)
        {
            // Cleanup when app is paused
            m_ImageManager.referenceLibrary = null;
        }
    }
    
    void OnApplicationQuit()
    {
        if (m_ImageManager != null)
        {
            // Cleanup when app is quitting
            m_ImageManager.referenceLibrary = null;
        }
    }
}