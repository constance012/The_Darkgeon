using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// A class manages all Graphics settings in the Options Menu.
/// </summary>
public class GraphicsOptionPage : MonoBehaviour
{
	[Header("References")]
	[Header("Graphics")]
	[Space]
	[SerializeField] private TMP_Dropdown qualityDropdown;

	[Header("Resolution")]
	[Space]
	[SerializeField] private TMP_Dropdown resolutionDropdown;
	[SerializeField] private Toggle fullscreenToggle;

	[Header("Framerate and Vsync")]
	[Space]
	[SerializeField] private Slider framerateSlider;
	[SerializeField] private Toggle vsyncToggle;
	[SerializeField] private TextMeshProUGUI framerateNumber;

	public static Resolution[] resoArr { get; set; }

	private int fullscreenIndex;
	private Color textDisableColor = new Color(.392f, .392f, .392f);

	private void Awake()
	{
		qualityDropdown = transform.Find("Quality Dropdown").GetComponent<TMP_Dropdown>();
		
		resolutionDropdown = transform.Find("Resolution Dropdown").GetComponent<TMP_Dropdown>();
		fullscreenToggle = transform.Find("Fullscreen Toggle").GetComponent<Toggle>();
		
		framerateSlider = transform.Find("Framerate Slider").GetComponent<Slider>();
		vsyncToggle = transform.Find("Vsync Toggle").GetComponent<Toggle>();
		framerateNumber = transform.Find("Framerate Number").GetComponent<TextMeshProUGUI>();
	}

	private void Start()
	{
		SetUpResoDropdown();

		ReloadUI();
	}

	public void SetQuality(int index)
	{
		QualitySettings.SetQualityLevel(index);
		UserSettings.QualityLevel = index;
	}

	public void SetFramerateLimit(float value)
	{
		int fps = Convert.ToInt32(value);

		Application.targetFrameRate = fps;
		framerateNumber.text = fps.ToString();

		UserSettings.TargetFramerate = value;
	}

	public void SetVsync(bool useVsync)
	{
		if (useVsync)
		{
			Application.targetFrameRate = -1;
			framerateSlider.SetValueWithoutNotify(Application.targetFrameRate);
			framerateNumber.text = framerateSlider.value.ToString();

			framerateSlider.interactable = false;
			framerateNumber.color = textDisableColor;
		}
		else
		{
			framerateSlider.value = UserSettings.TargetFramerate;
			framerateSlider.interactable = true;
			framerateNumber.color = Color.white;
		}

		QualitySettings.vSyncCount = useVsync ? 1 : 0;
		UserSettings.UseVsync = useVsync;
	}

	public void SetFullscreen(bool isFullsreen)
	{
		Screen.fullScreenMode = isFullsreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
		Debug.Log("Fullscreen: " + Screen.fullScreen);

		if (isFullsreen)
			resolutionDropdown.value = fullscreenIndex;

		else if (!resolutionDropdown.interactable)
			resolutionDropdown.value = UserSettings.ResolutionIndex;  // Return to previous resolution only once time.
		
		resolutionDropdown.interactable = !isFullsreen;  // The user can't change to other resolutions if the game is fullscreen.

		UserSettings.IsFullscreen = isFullsreen;  // If true then return 1, else 0.
	}

	public void SetResolution(int index)
	{
		Resolution selectedResolution = resoArr[index];
		Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);

		if (index != fullscreenIndex)
			UserSettings.ResolutionIndex = index;
	}

	public void ResetToDefault()
	{
		// Perform reset.
		UserSettings.ResetToDefault(UserSettings.SettingSection.Graphics);

		ReloadUI();
	}

	private void SetUpResoDropdown()
	{
		resolutionDropdown.ClearOptions();  // Clear all the placeholder options.

		List<string> options = new List<string>();

		for (int i = 0; i < resoArr.Length; i++)
		{
			string option = resoArr[i].width + " x " + resoArr[i].height;

			if (!options.Contains(option))
				options.Add(option);
		}

		// Update the dropdown each time the options scene is loaded.
		resolutionDropdown.AddOptions(options);
		fullscreenIndex = options.Count - 1;  // Last index.

		resolutionDropdown.RefreshShownValue();
	}

	private void ReloadUI()
	{
		// Set the values without notify.
		qualityDropdown.SetValueWithoutNotify(UserSettings.QualityLevel);

		resolutionDropdown.SetValueWithoutNotify(UserSettings.ResolutionIndex);

		fullscreenToggle.SetIsOnWithoutNotify(UserSettings.IsFullscreen);

		framerateSlider.SetValueWithoutNotify(UserSettings.ResolutionIndex);

		vsyncToggle.SetIsOnWithoutNotify(UserSettings.UseVsync);

		// Invoke the events manually.
		qualityDropdown.onValueChanged?.Invoke(qualityDropdown.value);
		resolutionDropdown.onValueChanged?.Invoke(resolutionDropdown.value);
		fullscreenToggle.onValueChanged?.Invoke(fullscreenToggle.isOn);
		framerateSlider.onValueChanged?.Invoke(framerateSlider.value);
		vsyncToggle.onValueChanged?.Invoke(vsyncToggle.isOn);
	}
}
