using UnityEngine;
using UnityEngine.UI;

public class SpatialUISlider : SpatialUI
{
    [SerializeField]
    MeshRenderer m_FillRenderer;

    float m_BoxColliderSizeX;

    void Start()
    {
        m_BoxColliderSizeX = GetComponent<BoxCollider>().size.x;
    }

    public override void Press(Vector3 position)
    {
        base.Press(position);
        var localPosition = transform.InverseTransformPoint(position);
        Debug.LogWarning($"{position} {localPosition} {m_BoxColliderSizeX}");
        var percentage = localPosition.x / m_BoxColliderSizeX + 0.5f;
        m_FillRenderer.material.SetFloat("_Percentage", Mathf.Clamp(percentage, 0.0f, 1.0f));
    }
}