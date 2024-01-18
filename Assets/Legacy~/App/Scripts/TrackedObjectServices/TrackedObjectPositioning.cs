using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackedObjectPositioning : MonoBehaviour
{
    private Dictionary<int, RecognizedObjectBehavior> _trackedGameObjects = new Dictionary<int, RecognizedObjectBehavior>();
    public GameObject _trackedPrefab;
    protected bool IsLockedObjectPositions;

    public void ToggleLockObjectPositions()
    {
        IsLockedObjectPositions = !IsLockedObjectPositions;
    }

    // Update is called once per frame
    void Update()
    {

        foreach (var t in SessionState.TrackedObjects.Reverse())
        {
            if (_trackedGameObjects.ContainsKey(t.classId))
            {
                // tracked object
                if (!IsLockedObjectPositions)
                {
                    _trackedGameObjects[t.classId]._newPosition = t.position;
                }

                _trackedGameObjects[t.classId].MaskPoints = t.mask;
                _trackedGameObjects[t.classId].refreshTime = t.lastUpdate;
            }
            else
            {
                // new object
                var clone = Instantiate(_trackedPrefab, this.transform);
                var behavior = clone.GetComponent<RecognizedObjectBehavior>();
                behavior.Name = t.label;
                behavior.Category = t.label;
                behavior._newPosition = t.position;
                var mat = new Material(Shader.Find("Mixed Reality Toolkit/Standard"));
                mat.SetColor("_Color", t.color);
                behavior.ApplyMaterial(mat);
                behavior.refreshTime = DateTime.Now;
                _trackedGameObjects.Add(t.classId, behavior);
            }
        }
    }
}
