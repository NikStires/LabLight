using OpenCVForUnity.UnityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Application = UnityEngine.WSA.Application;

[Serializable]
public class ArucoMapping
{
    public string Name;
    public int ArucoNumber;
}

/// <summary>
/// Uses OpenCV detector to generate a marker object with corresponding ObjectFrame
/// </summary>
public class OpenCvObjectFrameInjector : MonoBehaviour
{
    internal class GameObjectFrame
    {
        public GameObject gameObject;
        public TrackedObject objectFrame;
        public DateTime lastUpdated;
    }

    [SerializeField]
    private Transform CharucoFrame;

    [SerializeField]
    private GameObject HelperPrefab;

    [SerializeField]
    private int DestroyDelayInMilliseconds = 500;

    [SerializeField]
    private float MarkerLengthMeters = .031f;

    [Tooltip("Maps aruco values to detection labels")]
    public List<ArucoMapping> ArucoMappings;

    ArucoDetector detector;

    // Charuco conversion matrices
    private static Matrix4x4 invY = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
    private static Matrix4x4 invZ = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
    private static Matrix4x4 xRot = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));

    private int reservedId;
    private IDisposable videoFrameSubscription;

    // Use this for initialization
    void Start()
    {
        Debug.Log("OpenCvObjectFrameInjector started");

        reservedId = TrackedObject.IdCount++;
        detector = new ArucoDetector(MarkerLengthMeters);

        var camera = ServiceRegistry.GetService<IVideoCamera>();

        // Note: Callback can be on a background thread
        videoFrameSubscription = camera.GetFrames().Subscribe(frame =>
        {
            // Find marker
            var detectedArucos = detector.DetectArucos(frame, camera.Flip, camera.Format);

            // Ensure execution on main thread
            Application.InvokeOnAppThread(() => updateMarkers(frame.camera2World, detectedArucos), false);
        });

        camera.Running.Subscribe(running =>
        {
            if (!running && videoFrameSubscription != null)
            {
                Debug.Log("Shutdown camera");

                videoFrameSubscription.Dispose();
                videoFrameSubscription = null;

                // Ensure execution on main thread
                Application.InvokeOnAppThread(() => removeMarkers(), false);
            }
        });
    }

    private Dictionary<int, GameObjectFrame> arucoGameObjectFrames = new Dictionary<int, GameObjectFrame>();

    void updateMarkers(Matrix4x4 camera2World, List<DetectedAruco> detectedArucos)
    {
        var arucoIdToRemoveList = arucoGameObjectFrames.Keys.ToList();

        foreach (var detectedAruco in detectedArucos)
        {
            GameObjectFrame gameObjectFrame;
            if (!arucoGameObjectFrames.TryGetValue(detectedAruco.id, out gameObjectFrame))
            {
                // Add new objectframe and visual
                var arucoMapping = ArucoMappings.Find(am => am.ArucoNumber == detectedAruco.id);

                gameObjectFrame = new GameObjectFrame()
                {
                    gameObject = Instantiate(HelperPrefab, new Vector3(0, 0, 0), Quaternion.identity),
                    objectFrame = new TrackedObject()
                    {
                        id = detectedAruco.id,
                        label = (arucoMapping != null) ? arucoMapping.Name : "unknown"
                    }
                };

                gameObjectFrame.gameObject.name = "Aruco_" + detectedAruco.id;
                arucoGameObjectFrames.Add(detectedAruco.id, gameObjectFrame);
                SessionState.TrackedObjects.Add(gameObjectFrame.objectFrame);
            }
            else
            {
                // Still there, do not remove
                arucoIdToRemoveList.Remove(detectedAruco.id);
            }

            // Transform to unity coords (correctly this is a copy owned by main thread)
#if UNITY_EDITOR
            var markerTransform = camera2World * invY * detectedAruco.pose * invY;
#else
                var markerTransform = invZ * camera2World * invZ * invY * detectedAruco.pose * invY * xRot;
#endif

            // Update gameobject 
            ARUtils.SetTransformFromMatrix(gameObjectFrame.gameObject.transform, ref markerTransform);

            // Update objectFrame
            gameObjectFrame.objectFrame.position = CharucoFrame.InverseTransformPoint(gameObjectFrame.gameObject.transform.transform.position);
            gameObjectFrame.objectFrame.rotation = Quaternion.Inverse(CharucoFrame.rotation) * gameObjectFrame.gameObject.transform.transform.rotation;
            gameObjectFrame.lastUpdated = DateTime.Now;
        }

        foreach (var id in arucoIdToRemoveList)
        {
            removeMarker(id);
        }
    }

    void removeMarker(int id)
    {
        GameObjectFrame gameObjectFrame;
        if (arucoGameObjectFrames.TryGetValue(id, out gameObjectFrame))
        {
            // Only remove if old enough
            if (DestroyDelayInMilliseconds == 0 || DateTime.Now > gameObjectFrame.lastUpdated + TimeSpan.FromMilliseconds(DestroyDelayInMilliseconds))
            {
                Destroy(gameObjectFrame.gameObject);
                SessionState.TrackedObjects.Remove(gameObjectFrame.objectFrame);
                arucoGameObjectFrames.Remove(id);
            }
        }
    }

    void removeMarkers()
    {
        var arucoIdToRemoveList = arucoGameObjectFrames.Keys.ToList();

        foreach (var id in arucoIdToRemoveList)
        {
            removeMarker(id);
        }
    }
}

