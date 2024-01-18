using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioClipsSettings
{
    public AudioClip clip;
    public float pitch;
}

/// <summary>
/// Central audio manager, nothing fancy, just a central place to specify an audio theme
/// Could be expanded to control flexible audio theming (swap dictionaries) in the future
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour, IAudio
{
    private AudioSource audioSource;

    public UDictionary<AudioEventEnum, AudioClipsSettings> clips;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioEventEnum key)
    {
        AudioClipsSettings settings;
        if (clips.TryGetValue(key, out settings))
        {
            if (settings.clip != null)
            {
                audioSource.pitch = settings.pitch;
                audioSource.PlayOneShot(settings.clip);
            }
            else
            {
                Debug.LogError("Audioevent '" + key + "' is missing an audioclip");
            }
        }
    }
}