using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RotateButton : MonoBehaviour
{
    private XRSimpleInteractable interactable;

    public GameObject rotateParent;

    public int rotationDegrees;

    public void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(_ => RequestRotation());
    }

    public void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(_ => RequestRotation());
    }

    public void RequestRotation()
    {
        rotateParent.GetComponent<RotateModelButton>().RotateModel(rotationDegrees);
    }
}
