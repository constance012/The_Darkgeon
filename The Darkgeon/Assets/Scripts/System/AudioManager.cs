using System;
using UnityEngine;

/// <summary>
/// A class that manages all the game's sounds and music.
/// The instance of this class remains through different game sessions.
/// </summary>
public class AudioManager : MonoBehaviour
{
	// Make a Singleton.
	public static AudioManager instance { get; private set; }
	
	// An array of audios.
	public Sound[] sounds;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Destroy(gameObject);
			return;
		}

		foreach (var sound in sounds)
		{
			sound.source = gameObject.AddComponent<AudioSource>();
			sound.source.clip = sound.clip;
			sound.source.outputAudioMixerGroup = sound.mixerGroup;
			sound.source.volume = sound.volume;
			sound.source.pitch = sound.pitch;
		}
	}

	public void Play(string name)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);

		if (s == null)
		{
			Debug.LogWarning("Audio Clip: " + name + " not found!!");
			return;
		}

		s.source.Play();
	}
}
