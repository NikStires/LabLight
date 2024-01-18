using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class CameraPositionText : MonoBehaviour
{
    public TextMeshProUGUI camPositionText;
    public TextMeshProUGUI hmdPositionText;

    [SerializeField]
    InputActionReference m_HMDpos;

    private void OnEnable()
    {
        m_HMDpos.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
       Vector3 HMDpos = m_HMDpos.action.ReadValue<Vector3>();
       hmdPositionText.text = "HMD position: " + HMDpos;
       camPositionText.text = "Camera position: " + transform.position;
    }
}
