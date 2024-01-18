using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class HandJointVisualizer : MonoBehaviour
{
    public GameObject jointPrefab;

    XRHandSubsystem m_HandSubsystem;

    Dictionary<XRHandJointID, GameObject> jointsRight = new();
    Dictionary<XRHandJointID, GameObject> jointsLeft = new();

    void Start()
    {
        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);

        for (var i = 0; i < handSubsystems.Count; ++i)
        {
            var handSubsystem = handSubsystems[i];
            if (handSubsystem.running)
            {
                m_HandSubsystem = handSubsystem;
                break;
            }
        }

        if (m_HandSubsystem != null)
            m_HandSubsystem.updatedHands += OnUpdatedHands;
    }

    void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        switch (updateType)
        {
            case XRHandSubsystem.UpdateType.Dynamic:
                // Update game logic that uses hand data
                break;
            case XRHandSubsystem.UpdateType.BeforeRender:
                for (var i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex();  i++)
                {
                    XRHandJointID jointID = XRHandJointIDUtility.FromIndex(i);
                    if (!jointsRight.ContainsKey(jointID))
                    {
                        jointsRight.TryAdd(jointID, Instantiate(jointPrefab, this.transform));
                    }
                    if (!jointsLeft.ContainsKey(jointID))
                    {
                        jointsLeft.TryAdd(jointID, Instantiate(jointPrefab, this.transform));
                    }

                    var trackingDataRight = m_HandSubsystem.rightHand.GetJoint(jointID);
                    var trackingDataLeft = m_HandSubsystem.leftHand.GetJoint(jointID);

                    if (trackingDataRight.TryGetPose(out Pose poseRight))
                    {
                        jointsRight[jointID].transform.position = poseRight.position;
                        jointsRight[jointID].transform.rotation = poseRight.rotation;
                    }
                    if (trackingDataLeft.TryGetPose(out Pose poseLeft))
                    {
                        jointsLeft[jointID].transform.position = poseLeft.position;
                        jointsLeft[jointID].transform.rotation = poseLeft.rotation;
                    }
                }
                break;
        }
    }
}
