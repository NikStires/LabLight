using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DisableOutlineButton : MonoBehaviour
{
    public GameObject outline;

    [SerializeField] private Material toggleOffMaterial;
    [SerializeField] private Material toggleOnMaterial;
    private MeshRenderer meshRenderer;

    [SerializeField] Sprite toggleOnSprite;
    [SerializeField] Sprite toggleOffSprite;
    [SerializeField] Image image;

    public void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }


    // Update is called once per frame
    public void toggleOutline()
    {
        outline.SetActive(!outline.activeSelf);
        if(outline.activeSelf)
        {
            meshRenderer.material = toggleOnMaterial;
            image.sprite = toggleOnSprite;
        }
        else
        {
            meshRenderer.material = toggleOffMaterial;
            image.sprite = toggleOffSprite;
        }
    }
}
