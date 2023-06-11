using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using CSTGames.Utility;

public class PostProcessingController : MonoBehaviour
{
	[Header("General")]
	[Space]
	[SerializeField] private Volume ppVolume;
	public bool enable;

	[Header("Profiles")]
	[Space]
	[SerializeField] private VolumeProfile mainProfile;

	[Header("Post Processing Effects")]
	[SerializeField] private Vignette _vignette;

	// Health Effect.
	private Slider hpSlider;
	private float heartbeatInterval = 3f;
	private bool heartBeatSoundPlayed;

	private void Awake()
	{
		ppVolume = GetComponent<Volume>();
		hpSlider = GameObject.FindWithTag("Health Bar").transform.GetComponent<Slider>();
		
		ppVolume.profile.TryGet(out _vignette);
	}

	public void EnablePostProcess()
	{
		enable = !enable;
		ppVolume.enabled = enable;
	}

	/// <summary>
	/// Callback function whether the health value is updated.
	/// </summary>
	/// <param name="healthNormalized"></param>
	public void SetLowHealthEffect()
	{
		float healthNormalized = hpSlider.normalizedValue;

		// Invert the range so the oldMax will be relative to the newMin.
		_vignette.intensity.value = NumberManipulator.RangeConvert(healthNormalized, 0f, 1f, .55f, .3f);

		if (healthNormalized > .6f)
		{
			if (!heartBeatSoundPlayed)
				return;

			StopAllCoroutines();
			heartBeatSoundPlayed = false;

			AudioManager.instance.SetVolume("Heartbeat", .1f, true);
		}
		else
		{
			if (!heartBeatSoundPlayed)
				StartCoroutine(PlayHeartbeatSound());

			heartbeatInterval = NumberManipulator.RangeConvert(healthNormalized, 0f, .6f, 1f, 3f);
			float heartBeatVolume = NumberManipulator.RangeConvert(healthNormalized, 0f, .6f, .4f, .1f);

			AudioManager.instance.SetVolume("Heartbeat", heartBeatVolume);
		}
	}

	private IEnumerator PlayHeartbeatSound()
	{
		heartBeatSoundPlayed = true;

		while (hpSlider.normalizedValue <= .6f && !GameManager.isPlayerDeath)
		{
			yield return new WaitForSeconds(heartbeatInterval);
			AudioManager.instance.Play("Heartbeat");
		}

		heartBeatSoundPlayed = false;
	}
}
