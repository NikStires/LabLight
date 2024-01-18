using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelTransformBehavior : MonoBehaviour
{
    [SerializeField]
    MeshRenderer m_MeshRenderer;

    [SerializeField]
    Material m_DefaultMat;

    [SerializeField]
    Material m_SelectedMat;

    [SerializeField]
    private Transform parentPanel;

    GameObject m_TempParent;

    void Start()
    {
        m_TempParent = new GameObject("tempParent_" + gameObject.name);
        m_TempParent.transform.position = transform.position;
    }   

    public void Select()
    {
        m_MeshRenderer.material = m_SelectedMat;
        transform.SetParent(m_TempParent.transform);
    }

    public void Drag(Vector3 interactionPosition, Vector3 hmdPos)
    {
        m_TempParent.transform.LookAt(hmdPos);
        parentPanel.LookAt(hmdPos);

        m_TempParent.transform.position = interactionPosition;
        Vector3 targetPanelPosition = new Vector3(interactionPosition.x, interactionPosition.y, interactionPosition.z) + parentPanel.up * 0.2f;
        parentPanel.position = targetPanelPosition;
    }

    public void Deselect()
    {
        m_MeshRenderer.material = m_DefaultMat;
        transform.SetParent(parentPanel);
    }
}
