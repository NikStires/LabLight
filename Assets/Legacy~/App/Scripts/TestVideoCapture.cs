using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVideoCapture : MonoBehaviour
{
    [Tooltip("Webcam camera to use in editor. Will fallback to PreloadedImageVideoCamera when null")]
    public WebCamTexureVideoCamera webCamVideoCamera;

    private void Awake()
    {
        var debugLogger = new DebugLogger();
        ServiceRegistry.RegisterService<LoggerImpl>(debugLogger);

#if UNITY_EDITOR
        ServiceRegistry.RegisterService<IVideoCamera>(webCamVideoCamera);
#else
        ServiceRegistry.RegisterService<IVideoCamera>(new VideoCameraDevice());
        Debug.Log("*** Starting VideoCameraDevice");
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
