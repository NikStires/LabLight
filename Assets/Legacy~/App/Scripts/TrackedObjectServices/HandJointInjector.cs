using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Injects left and right hand fingertips as trackedobjects
/// </summary>
public class HandJointInjector : MonoBehaviour
{
    internal class GameObjectFrame
    {
        public GameObject gameObject;
        public TrackedObject objectFrame;
        public DateTime lastUpdated;
    }

    private Dictionary<int, GameObjectFrame> fingerTipDictionary = new Dictionary<int, GameObjectFrame>();


    private List<TrackedHandJoint> fingerTips = new List<TrackedHandJoint>()
    {
        TrackedHandJoint.ThumbTip,
        TrackedHandJoint.IndexTip,
        TrackedHandJoint.MiddleTip,
        TrackedHandJoint.RingTip,
        TrackedHandJoint.PinkyTip
    };

    void Update()
    {
        //Removed hand tracking from tracked objects
        //UpdateFingerTips(Handedness.Left);
        //UpdateFingerTips(Handedness.Right);
    }

    private void  UpdateFingerTips(Handedness handedness)
    {
        var hand = HandJointUtils.FindHand(handedness);

        MixedRealityPose fingerTipPose;
        if (hand != null && hand.TrackingState == Microsoft.MixedReality.Toolkit.TrackingState.Tracked)
        {
            GameObjectFrame gameObjectFrame;

            for (int i = 0; i < fingerTips.Count; i++)
            {
                var fingertip = fingerTips[i];
                int id = handedness.IsRight() ? 10000 + i : 10005 + i;
                if (hand.TryGetJoint(fingertip, out fingerTipPose))
                {
                    // Find trackedobject for fingertip
                    if (!fingerTipDictionary.TryGetValue(id, out gameObjectFrame))
                    {
                        // create if non found
                        gameObjectFrame = new GameObjectFrame()
                        {
                            // gameObject = Instantiate(HelperPrefab, new Vector3(0, 0, 0), Quaternion.identity),
                            objectFrame = new TrackedObject()
                            {
                                id = id,
                                label = handedness.ToString() + fingertip.ToString()
                            }
                        };

                        fingerTipDictionary[id] = gameObjectFrame;
                        SessionState.TrackedObjects.Add(gameObjectFrame.objectFrame);
                    }

                    // update trackedobject
                    gameObjectFrame.objectFrame.position = fingerTipPose.Position;
                }
            }
        }
        else
        {
            // removed trackedobject for fingertip
            for (int i = 0; i < fingerTips.Count; i++)
            {
                int id = handedness.IsRight() ? 10000 + i : 10005 + i;
                removeFingerTipObject(id);
            }
        }
    }

    void removeFingerTipObject(int id)
    {
        GameObjectFrame gameObjectFrame;
        if (fingerTipDictionary.TryGetValue(id, out gameObjectFrame))
        {
            SessionState.TrackedObjects.Remove(gameObjectFrame.objectFrame);
            fingerTipDictionary.Remove(id);
        }
    }
}

