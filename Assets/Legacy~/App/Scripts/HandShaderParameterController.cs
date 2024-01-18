using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Transfers hand info to shader parameter so shaders can use this for adapting visualizations.
/// </summary>
public class HandShaderParameterController : MonoBehaviour
{
    private Handedness _monitoredHand = Handedness.Any;

    void Update()
    {
        MixedRealityPose palmPose;
        var hand = HandJointUtils.FindHand(_monitoredHand);
        if (hand != null && hand.TrackingState == Microsoft.MixedReality.Toolkit.TrackingState.Tracked)
        {
            _monitoredHand = hand.ControllerHandedness;
            if (hand.TryGetJoint(TrackedHandJoint.Palm, out palmPose))
            {
                //Shader.SetGlobalFloat("_FingerGlow", val);
                Shader.SetGlobalVector("_PalmPosition", palmPose.Position);
//                Shader.SetGlobalVector("_PalmRotation", palmPose.Rotation);
            }
        }
        else
        {
            _monitoredHand = Handedness.Any;
        }
    }
}
