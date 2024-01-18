using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Transfers hand info to shader parameter so shaders can use this for adapting visualizations.
/// </summary>
public class HandToolVolumeController : MonoBehaviour
{
    public Vector3 localPosition = Vector3.zero;
    public Quaternion localRotation = Quaternion.identity;
    private Handedness _monitoredHand = Handedness.Any;
    private Renderer _renderer;

    private void Start()
    {
        _renderer = this.GetComponent<Renderer>();
    }

    void Update()
    {
        MixedRealityPose palmPose;
        var hand = HandJointUtils.FindHand(_monitoredHand);
        if (hand != null && hand.TrackingState == Microsoft.MixedReality.Toolkit.TrackingState.Tracked)
        {
            _monitoredHand = hand.ControllerHandedness;

            if (hand.TryGetJoint(TrackedHandJoint.Palm, out palmPose))
            {
                Matrix4x4 transformMatrix = Matrix4x4.TRS(palmPose.Position, palmPose.Rotation, Vector3.one);

                transform.position = transformMatrix.MultiplyPoint(localPosition);
                transform.rotation = palmPose.Rotation * localRotation;
            }
        }
        else
        {
            _monitoredHand = Handedness.Any;
        }

        _renderer.enabled = (_monitoredHand != Handedness.Any);
    }
}
