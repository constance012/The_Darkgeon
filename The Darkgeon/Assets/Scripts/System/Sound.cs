using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// A custom class contains informations about an Audio Clip.
/// Used by the Audio Manager.
/// </summary>
[System.Serializable]
public class Sound
{
	public string name;
	
	[Space]
	public AudioClip[] clips;
	
	[Space]
	public AudioMixerGroup mixerGroup;
	
	[Space]
	[Range(0f, 1f)]  // Use range to create a slider for a public field.
	public float volume;

	[Range(-3f, 3f)]
	public float pitch;

	public bool loop;
	[HideInInspector] public AudioSource source;
}
