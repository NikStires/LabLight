using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class SpatialUI : MonoBehaviour
{
    public Color SelectedColor = new Color(.1254f, .5882f, .9529f);
    public Color UnselectedColor = new Color(.1764f, .1764f, .1764f);

    public AudioSource m_AudioSource;

    void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    // TODO Hover component?

    public virtual void Press(Vector3 position)
    {
        if(m_AudioSource.clip != null)
        {
            m_AudioSource.Play();
        }
    }
}
    