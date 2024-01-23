using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(AudioSource))]
public class SpatialUIButton : SpatialUI
{
    public delegate void ClickEventHandler();
    public event ClickEventHandler OnClick;

    public string ButtonText => m_ButtonText;
    public MeshRenderer MeshRenderer => m_MeshRenderer;

    public bool IsEnabled = true;

    [SerializeField] string m_ButtonText;

    MeshRenderer m_MeshRenderer;

    private void Awake()
    {   
        m_MeshRenderer = GetComponent<MeshRenderer>();
    }

    public override void Press(Vector3 position)
    {
        if(IsEnabled)
        base.Press(position);
        OnClick();
    }
}
