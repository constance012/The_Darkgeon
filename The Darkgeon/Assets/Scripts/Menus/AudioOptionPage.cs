using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// A class manages all Audio settings in the Options Menu.
/// </summary>
public class AudioOptionPage : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private AudioMixer mixer;

	// Sliders.
	private Slider masterSlider;
	private Slider musicSlider;
	private Slider soundsSlider;

	// Text UIs.
	private TextMeshProUGUI masterAmountUI;
	private TextMeshProUGUI musicAmountUI;
	private TextMeshProUGUI soundsAmountUI;

	private void Awake()
	{
		Debug.Log("Audio Page awoke.");
		masterSlider = transform.GetComponentInChildren<Slider>("Master Slider");
		musicSlider = transform.GetComponentInChildren<Slider>("Music Slider");
		soundsSlider = transform.GetComponentInChildren<Slider>("Sounds Slider");

		masterAmountUI = transform.GetComponentInChildren<TextMeshProUGUI>("Master Amount");
		musicAmountUI = transform.GetComponentInChildren<TextMeshProUGUI>("Music Amount");
		soundsAmountUI = transform.GetComponentInChildren<TextMeshProUGUI>("Sounds Amount");
	}

	private void Start()
	{
		ReloadUI();
	}

	public void SetMasterVolume(float amount)
	{
		mixer.SetFloat("masterVol", amount);

		masterAmountUI.text = GetVolumeAmount(amount);
		UserSettings.MasterVolume = amount;
	}

	public void SetMusicVolume(float amount)
	{
		mixer.SetFloat("musicVol", amount);

		musicAmountUI.text = GetVolumeAmount(amount);
		UserSettings.MusicVolume = amount;
	}

	public void SetSoundsVolume(float amount)
	{
		mixer.SetFloat("soundsVol", amount);

		soundsAmountUI.text = GetVolumeAmount(amount);
		UserSettings.SoundsVolume = amount;
	}

	public void ResetToDefault()
	{
		UserSettings.ResetToDefault(UserSettings.SettingSection.Audio);

		ReloadUI();
	}

	private string GetVolumeAmount(float amount)
	{
		float percent01 = 1f - (Mathf.Abs(amount) / 80f);
		return (percent01 * 100f).ToString("0");
	}

	private void ReloadUI()
	{
		float masterVol = UserSettings.MasterVolume;
		float musicVol = UserSettings.MusicVolume;
		float soundsVol = UserSettings.SoundsVolume;

		masterSlider.value = masterVol;
		musicSlider.value = musicVol;
		soundsSlider.value = soundsVol;
	}
}
