using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(AudioSource))]
public class SpatialUIButton : SpatialUI
{
    public Action<string, MeshRenderer> WasPressed;

    public string ButtonText => m_ButtonText;
    public MeshRenderer MeshRenderer => m_MeshRenderer;

    [SerializeField] string m_ButtonText;
    [SerializeField] AudioSource m_AudioSource;

    MeshRenderer m_MeshRenderer;

    private void Awake()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();
        m_AudioSource = GetComponent<AudioSource>();

    }

    public override void Press(Vector3 position)
    {
        base.Press(position);
        m_AudioSource.Play();
        if (WasPressed != null)
        {
            WasPressed.Invoke(m_ButtonText, m_MeshRenderer);
        }
    }
}
