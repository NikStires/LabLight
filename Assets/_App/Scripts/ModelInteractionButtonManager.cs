using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ModelInteractionButtonManager : MonoBehaviour
{
    [SerializeField]
    public PlaneInteractionManagerScriptableObject planeInteractionManagerSO;
    private XRSimpleInteractable interactable;

    public GameObject relockButton;

    public GameObject rotationButton;

    public GameObject toggleOutlineButton;

    private bool currentState;

    public void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(_ => ToggleButtons());
        currentState = false;
    }

    public void OnDestroy()
    {
        if(interactable != null)
        {
            interactable.selectEntered.RemoveListener(_ => ToggleButtons());
        }
    }

    public void ToggleButtons()
    {
        Debug.Log("Toggling buttons");
        currentState = !currentState;
        relockButton.SetActive(currentState);
        rotationButton.SetActive(currentState);
        toggleOutlineButton.SetActive(currentState);
    }
}
