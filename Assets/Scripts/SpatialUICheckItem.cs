using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialUICheckItem : SpatialUI
{
    public bool Active => m_Active;

    [SerializeField]
    protected Transform m_CheckMark;

    [SerializeField]
    MeshRenderer m_ToggleBackground;

    bool m_Active = false;
    protected Vector3 m_BubbleTargetPosition;
    protected Vector3 m_BubbleOnTargetPosition;
    protected Vector3 m_BubbleOffTargetPosition;

    float m_StartLerpTime;

    protected const float k_BubbleOnPosition = 0.0025f;
    protected const float k_BubbleOffPosition = 0f;    
    protected const float k_LerpSpeed = 3.0f;

    void Start()
    {
        m_ToggleBackground.material.color = UnselectedColor;    
        var bubblePosition = m_CheckMark.localPosition;
        m_BubbleOnTargetPosition = new Vector3(bubblePosition.x, k_BubbleOnPosition, bubblePosition.z);
        m_BubbleOffTargetPosition = new Vector3(bubblePosition.x, k_BubbleOffPosition, bubblePosition.z);
    }

    public override void Press(Vector3 position)
    {
        base.Press(position);
        m_Active = !m_Active;   
        m_StartLerpTime = Time.time;
        m_ToggleBackground.material.color = m_Active ? SelectedColor : UnselectedColor;
    }

    public void Update()
    {
        var coveredAmount = (Time.time - m_StartLerpTime) * k_LerpSpeed;
        var lerpPercentage = coveredAmount / (k_BubbleOffPosition * 2);
        m_CheckMark.localPosition = Vector3.Lerp(m_Active ? m_BubbleOffTargetPosition : m_BubbleOnTargetPosition, m_Active ? m_BubbleOnTargetPosition : m_BubbleOffTargetPosition, lerpPercentage);
        if(m_CheckMark.localPosition == m_BubbleOffTargetPosition)
        {
            m_CheckMark.gameObject.SetActive(false);
        }
        else
        {
            m_CheckMark.gameObject.SetActive(true);
        }
    }
}
