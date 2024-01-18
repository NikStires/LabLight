using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckItemViewController : MonoBehaviour
{
    public GameObject backplate;
    public GameObject checkmarkGroup;

    public void ActivateBackplate()
    {
        backplate.SetActive(true);
    }

    public void DeactivateBackplate()
    {
        backplate.SetActive(false);
    }

    public void ActiveCheckmarkGroup()
    {
        checkmarkGroup.SetActive(true);
    }

    public void DeactivateCheckmarkGroup()
    {
        checkmarkGroup.SetActive(false);
    }
}
