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
			Debug.LogWarning("More than one instance of Audio Manager found!!");
			Destroy(gameObject);
			return;
		}

		GameObject audioSourceHolder = new GameObject("Audio Source Holder");
		audioSourceHolder.transform.parent = this.transform;

		foreach (var sound in sounds)
		{
			sound.source = audioSourceHolder.AddComponent<AudioSource>();
			sound.source.outputAudioMixerGroup = sound.mixerGroup;
			sound.source.volume = sound.volume;
			sound.source.pitch = sound.pitch;
		}
	}

	public void Play(string soundName)
	{
		Sound chosenSound = GetSound(soundName);

		if (chosenSound == null)
		{
			Debug.LogWarning("Audio Clip: " + soundName + " not found!!");
			return;
		}

		chosenSound.source.clip = GetRandomClip(chosenSound);

		chosenSound.source.Play();
	}

	public void SetVolume(string soundName, float newVolume, bool resetToDefault = false)
	{
		Sound chosenSound = GetSound(soundName);

		if (!resetToDefault)
			chosenSound.source.volume = newVolume;
		else
			chosenSound.source.volume = chosenSound.volume;
	}

	private Sound GetSound(string soundName)
	{
		soundName = soundName.ToLower().Trim();
		return Array.Find(sounds, sound => sound.name.ToLower().Equals(soundName));
	}

	private AudioClip GetRandomClip(Sound sound)
	{
		int index = UnityEngine.Random.Range(0, sound.clips.Length);
		return sound.clips[index];
	}
}
