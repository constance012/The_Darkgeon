using UnityEngine;

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
