using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using MoreMountains.Feedbacks;

public class ModelSettingsButton : MonoBehaviour
{
    private bool toggledOn = false;
    [SerializeField] private MMF_Player toggleOnAnimation;
    [SerializeField] private MMF_Player toggleOffAnimation;

    [SerializeField] private Material toggleOffMaterial;
    [SerializeField] private Material toggleOnMaterial;
    private MeshRenderer meshRenderer;

    public void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        toggledOn = false;
    }

    public void ToggleButtons()
    {
        Debug.Log("Toggling buttons");
        toggledOn = !toggledOn;
        if(toggledOn)
        {
            meshRenderer.material = toggleOnMaterial;
            toggleOffAnimation.StopFeedbacks();
            toggleOnAnimation.PlayFeedbacks();
        }
        else
        {
            meshRenderer.material = toggleOffMaterial;
            toggleOnAnimation.StopFeedbacks();
            toggleOffAnimation.PlayFeedbacks();
        }
    }
}
