using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SystemTrayButtonController : MonoBehaviour
{
     public TextMeshPro text;
     public Transform IconAndText;

     public Transform backplate;

     void Start()
     {
        IconAndText.localPosition = new Vector3(0, 0, 0);
     }

     public void SetPanelName(string name)
     {
        text.text = name;
     }

     public void ToggleBackplate(bool maximized)
     {
        if(maximized)
        {
            backplate.gameObject.SetActive(false);
        }
        else
        {
            backplate.gameObject.SetActive(true);
        }
     }
}
