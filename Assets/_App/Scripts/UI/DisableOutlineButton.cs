using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DisableOutlineButton : MonoBehaviour
{
    [SerializeField]
    public PlaneInteractionManagerScriptableObject planeInteractionManagerSO;
    private XRSimpleInteractable interactable;

    public GameObject outline;


    public void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(_ => toggleOutline());
    }

    public void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(_ => toggleOutline());
    }


    // Update is called once per frame
    public void toggleOutline()
    {
        outline.SetActive(!outline.activeSelf);
    }
}
