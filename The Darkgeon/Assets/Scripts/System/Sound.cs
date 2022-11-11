using UnityEngine;

/// <summary>
/// A custom class contains informations about an Audio Clip.
/// Used by the Audio Manager.
/// </summary>
[System.Serializable]
public class Sound
{
	public AudioClip clip;
	[HideInInspector] public AudioSource source;

	public string name;

	[Range(0f, 1f)]  // Use range to create a slider for a public field.
	public float volume;

	[Range(.1f, 3f)]
	public float pitch;

	public bool loop;
}
