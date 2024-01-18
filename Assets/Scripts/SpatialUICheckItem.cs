using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialUICheckItem : SpatialUI
{
    public bool Active => m_Active;

    [SerializeField]
    protected Transform m_CheckMark;

    [SerializeField]
    MeshRenderer m_ToggleMesh;

    bool m_Active = false;
    protected Vector3 m_ToggleOnTargetPosition;
    protected Vector3 m_ToggleOffTargetPosition;

    float m_StartLerpTime;

    protected const float k_ToggleOnPosition = 0.015f;
    protected const float k_ToggleOffPosition = 0.06f;    
    protected const float k_LerpSpeed = 3.0f;

    void Start()
    {
        m_ToggleMesh.material.color = UnselectedColor;
        var meshPosition = m_ToggleMesh.transform.position;
        m_ToggleOnTargetPosition = new Vector3(1.5f, 0, k_ToggleOnPosition);
        m_ToggleOffTargetPosition = new Vector3(1.5f, 0, k_ToggleOffPosition);
    }

    public override void Press(Vector3 position)
    {
        base.Press(position);
        m_Active = !m_Active;   
        m_StartLerpTime = Time.time;
        m_ToggleMesh.material.color = m_Active ? SelectedColor : UnselectedColor;
    }

    public void Update()
    {
        var coveredAmount = (Time.time - m_StartLerpTime) * k_LerpSpeed;
        var lerpPercentage = coveredAmount / (k_ToggleOffPosition * 2);
        m_ToggleMesh.transform.localPosition = Vector3.Lerp(m_Active ? m_ToggleOffTargetPosition : m_ToggleOnTargetPosition, m_Active ? m_ToggleOnTargetPosition : m_ToggleOffTargetPosition, lerpPercentage);
        if(!m_Active)
        {
            m_CheckMark.gameObject.SetActive(false);
        }
        else
        {
            m_CheckMark.gameObject.SetActive(true);
        }
    }
}
