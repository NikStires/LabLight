using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine;

/// <summary>
/// Public Update for SpacePin 
/// </summary>
public class SimpleSpacePinHandler : SpacePinOrientable
{
    public bool Frozen = false;

    public void UpdateSpacePin()
    {
        var pose = ExtractModelPose();
        
        if (Frozen)
        {
            SetFrozenPose(ExtractModelPose());
        }
        else
        {
            SetSpongyPose(ExtractModelPose());
        }
    }
}
