using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class EmulatedDetection
{
    public string Name;
    public bool Enabled;
}

public class GameObjectId
{
    public GameObject GameObject;
    public int Id;
}

/// <summary>
/// Editor only detection generator
/// Generates prefab instance(s) that can be used to emulate detections in editor.
/// </summary>
public class EditorObjectFrameInjector : MonoBehaviour
{
    [SerializeField]
    private Transform CharucoFrame;

    [Tooltip("Gameobject that will be used as a source for generating ObjectFrame detections.")]
    public GameObject HelperPrefab;

    [Tooltip("Emulated detections with labels and their enabled state")]
    public List<EmulatedDetection> Detections;

    private Dictionary<EmulatedDetection, GameObjectId> HelperInstances = new Dictionary<EmulatedDetection, GameObjectId>();

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        float offset = 0;
        foreach (var det in Detections)
        {
            if (!HelperInstances.ContainsKey(det))
            {
                HelperInstances[det] = new GameObjectId()
                {
                    GameObject = Instantiate(HelperPrefab, new Vector3(offset, 0.1f, 0), HelperPrefab.transform.rotation, CharucoFrame),
                    Id = TrackedObject.IdCount++
                };
                offset -= 0.1f;
            }
            HelperInstances[det].GameObject.SetActive(det.Enabled);
        }

        // Build a list of objectFrames
        foreach (var helperInstance in HelperInstances)
        {
            var detection = SessionState.TrackedObjects.Where(o => o.id == helperInstance.Value.Id).FirstOrDefault();
            if (helperInstance.Key.Enabled)
            {
                // Create
                if (detection == null)
                {
                    detection = new TrackedObject()
                    {
                        id = helperInstance.Value.Id,
                        label = helperInstance.Key.Name
                    };
                    SessionState.TrackedObjects.Add(detection);
                }

                // Update
                // Convert World Coordinates to local coordinates in Charuco Frame
                detection.position = CharucoFrame.InverseTransformPoint(helperInstance.Value.GameObject.transform.position);
                detection.rotation = Quaternion.Inverse(CharucoFrame.rotation) * helperInstance.Value.GameObject.transform.rotation;
            }
            else if (!helperInstance.Key.Enabled && detection != null)
            {
                // Remove
                SessionState.TrackedObjects.Remove(detection);
            }
        }
#endif
    }
}
