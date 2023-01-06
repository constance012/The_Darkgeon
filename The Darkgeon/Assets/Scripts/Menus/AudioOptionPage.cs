using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

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
		masterSlider = transform.Find("Master Slider").GetComponent<Slider>();
		musicSlider = transform.Find("Music Slider").GetComponent<Slider>();
		soundsSlider = transform.Find("Sounds Slider").GetComponent<Slider>();

		masterAmountUI = transform.Find("Master Amount").GetComponent<TextMeshProUGUI>();
		musicAmountUI = transform.Find("Music Amount").GetComponent<TextMeshProUGUI>();
		soundsAmountUI = transform.Find("Sounds Amount").GetComponent<TextMeshProUGUI>();
	}

	private void Start()
	{
		Load();
	}

	public void SetMasterVolume(float amount)
	{
		mixer.SetFloat("masterVol", amount);

		masterAmountUI.text = GetVolumeAmount(amount);
		PlayerPrefs.SetFloat("MasterVolume", amount);
	}

	public void SetMusicVolume(float amount)
	{
		mixer.SetFloat("musicVol", amount);

		musicAmountUI.text = GetVolumeAmount(amount);
		PlayerPrefs.SetFloat("MusicVolume", amount);
	}

	public void SetSoundsVolume(float amount)
	{
		mixer.SetFloat("soundsVol", amount);

		soundsAmountUI.text = GetVolumeAmount(amount);
		PlayerPrefs.SetFloat("SoundsVolume", amount);
	}

	private string GetVolumeAmount(float amount)
	{
		float invertedPercent = Mathf.Abs(amount) / 80f;
		return ((1f - invertedPercent) * 100f).ToString("0");
	}

	private void Load()
	{
		float masterVol = PlayerPrefs.GetFloat("MasterVolume", 0f);
		float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0f);
		float soundsVol = PlayerPrefs.GetFloat("SoundsVolume", 0f);

		masterSlider.value = masterVol;
		musicSlider.value = musicVol;
		soundsSlider.value = soundsVol;

		masterAmountUI.text = GetVolumeAmount(masterVol);
		musicAmountUI.text = GetVolumeAmount(musicVol);
		soundsAmountUI.text = GetVolumeAmount(soundsVol);
	}
}
