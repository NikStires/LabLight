using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModelStatusButtonViewController : MonoBehaviour
{
    public TextMeshProUGUI modelNameText;
    public GameObject lockedIcon;
    public GameObject unalignedIcon;
    public GameObject backplate;

    public void InitButton(ArDefinition ardef, ModelElementViewController modelView)
    {
        modelNameText.text = ((AnchorCondition)ardef.condition).filter;

        if(((WorldPositionController)modelView).positionLocked)
        {
            lockedIcon.SetActive(true);
        }
        else
        {
            lockedIcon.SetActive(false);
        }

        DeactivateBackplate();
    }

    public void ActivateLockIcon()
    {
        lockedIcon.SetActive(true);
    }

    public void DeactivateLockIcon()
    {
        lockedIcon.SetActive(false);
    }

    public void ActivateBackplate()
    {
        backplate.SetActive(true);
    }

    public void DeactivateBackplate()
    {
        backplate.SetActive(false);
    }
}
