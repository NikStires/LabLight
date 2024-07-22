using UnityEngine;

public class SpatialUIToggle : SpatialUI
{
    public bool Active => m_Active;

    [SerializeField]
    protected Transform m_ToggleBubble;

    [SerializeField]
    MeshRenderer m_ToggleBackground;

    bool m_Active = true;
    protected Vector3 m_BubbleTargetPosition;
    protected Vector3 m_BubbleOnTargetPosition;
    protected Vector3 m_BubbleOffTargetPosition;

    float m_StartLerpTime;

    protected const float k_BubbleOnPosition = -0.00533f;
    protected const float k_BubbleOffPosition = 0.0055f;
    protected const float k_LerpSpeed = 3.0f;

    void Start()
    {
        var bubblePosition = m_ToggleBubble.localPosition;
        m_BubbleOnTargetPosition = new Vector3(k_BubbleOnPosition, bubblePosition.y, bubblePosition.z);
        m_BubbleOffTargetPosition = new Vector3(k_BubbleOffPosition, bubblePosition.y, bubblePosition.z);
    }

    public override void Press(Vector3 position)
    {
        base.Press(position);
        m_Active = !m_Active;
        Debug.Log("Check Item Pressed");
        m_StartLerpTime = Time.time;
        m_ToggleBackground.material.color = m_Active ? SelectedColor : UnselectedColor;
    }

    public void Update()
    {
        var coveredAmount = (Time.time - m_StartLerpTime) * k_LerpSpeed;
        var lerpPercentage = coveredAmount / (k_BubbleOffPosition * 2);
        m_ToggleBubble.localPosition = Vector3.Lerp(m_Active ? m_BubbleOffTargetPosition : m_BubbleOnTargetPosition,
            m_Active ? m_BubbleOnTargetPosition : m_BubbleOffTargetPosition, lerpPercentage);
    }
}
