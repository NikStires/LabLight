using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RotateModelButton : MonoBehaviour
{
    private XRSimpleInteractable interactable;

    public GameObject positionRotationButton;

    public GameObject negativeRotationButton;

    public GameObject mainPrefab;

    public GameObject outlinePrefab;

    private GameObject duplicateOutline;

    private bool buttonsEnabled;

    private bool inRotation;

    public void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(_ => ToggleButtons());
        buttonsEnabled = false;
        inRotation = false;
    }

    private void ToggleButtons()
    {
        if(inRotation)
        {
            //outlinePrefab.transform.rotation = duplicateOutline.transform.rotation;
            mainPrefab.transform.rotation = duplicateOutline.transform.rotation;
            Destroy(duplicateOutline);
            inRotation = false;
        }
        buttonsEnabled = !buttonsEnabled;
        if(positionRotationButton != null && negativeRotationButton != null)
        {
            positionRotationButton.SetActive(buttonsEnabled);
            negativeRotationButton.SetActive(buttonsEnabled);
        }else
        {
            RotateModel(45); //if no buttons exist this button rotates the model by 90 degrees
        }
    }

    public void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(_ => ToggleButtons());
    }

    public void OnDisable()
    {
        if(inRotation)
        {
            //outlinePrefab.transform.rotation = duplicateOutline.transform.rotation;
            mainPrefab.transform.rotation = duplicateOutline.transform.rotation;
            Destroy(duplicateOutline);
            duplicateOutline = null;
            inRotation = false;
        }
        buttonsEnabled = false;
        if(positionRotationButton != null && negativeRotationButton != null)
        {
            positionRotationButton.SetActive(buttonsEnabled);
            negativeRotationButton.SetActive(buttonsEnabled);
        }
    }

    public void RotateModel(int degrees)
    {
        if(duplicateOutline == null)
        {
            inRotation = true;
            duplicateOutline = Instantiate(outlinePrefab, outlinePrefab.transform.position, outlinePrefab.transform.rotation);
            duplicateOutline.transform.parent = transform;
        }
        duplicateOutline.transform.RotateAround(duplicateOutline.transform.position, Vector3.up, degrees);
        Quaternion rotation = Quaternion.Euler(0f, degrees, 0f);
        duplicateOutline.transform.rotation *= rotation;
    }
}
