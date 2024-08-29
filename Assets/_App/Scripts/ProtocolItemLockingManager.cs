using UniRx;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Hands;

public class ProtocolItemLockingManager : MonoBehaviour
{
    public HeadPlacementEventChannel headPlacementEventChannel;

    private Queue<GameObject> objectsQueue = new Queue<GameObject>();

    private void OnEnable()
    {
        AddSubscriptions();
    }

    private void OnDisable()
    {
        RemoveSubscriptions();
        if(objectsQueue.Count > 0)
        {
            headPlacementEventChannel.RequestDisablePlaneInteractionManager.Invoke();
            objectsQueue.Clear();
        }
    }

    private void AddSubscriptions()
    {
        headPlacementEventChannel.CurrentPrefabLocked.AddListener(ObjectLocked);
    }

    private void RemoveSubscriptions()
    {
        headPlacementEventChannel.CurrentPrefabLocked.RemoveListener(ObjectLocked);
    } 

    public void EnqueueObjects(List<GameObject> objectsToLock)
    {
        Debug.Log("Added " + objectsToLock.Count + " objects to lock");
        if(objectsQueue.Count == 0 && objectsToLock.Count > 0)
        {
            headPlacementEventChannel.SetHeadtrackedObject.Invoke(objectsToLock[0]);
            objectsToLock.RemoveAt(0);
        }
        foreach (GameObject obj in objectsToLock)
        {
            objectsQueue.Enqueue(obj);
        }
    }

    private void ObjectLocked()
    {
        if(objectsQueue.Count > 0)
        {
            headPlacementEventChannel.SetHeadtrackedObject.Invoke(objectsQueue.Dequeue());
        }
        if(objectsQueue.Count == 0)
        {
            headPlacementEventChannel.RequestDisablePlaneInteractionManager.Invoke();
        }
    }
}