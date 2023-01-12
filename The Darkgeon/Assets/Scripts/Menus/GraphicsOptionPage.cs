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
		resolutionDropdown.RefreshShownValue();

		ReloadUI();
	}

	public void SetQuality(int index)
	{
		QualitySettings.SetQualityLevel(index);
		PlayerPrefs.SetInt("QualityLevel", index);
	}

	public void SetFramerateLimit(float value)
	{
		int fps = Convert.ToInt32(value);

		Application.targetFrameRate = fps;
		framerateNumber.text = fps.ToString();

		PlayerPrefs.SetFloat("TargetFramerate", value);
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
			framerateSlider.value = PlayerPrefs.GetFloat("TargetFramerate", 120f);
			framerateSlider.interactable = true;
			framerateNumber.color = Color.white;
		}

		QualitySettings.vSyncCount = useVsync ? 1 : 0;
		PlayerPrefs.SetInt("UseVsync", useVsync ? 1 : 0);
	}

	public void SetFullscreen(bool isFullsreen)
	{
		if (isFullsreen)
			resolutionDropdown.value = fullscreenIndex;

		else if (!resolutionDropdown.interactable)
			resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 7); ;  // Return to previous resolution only once time.

		Screen.fullScreen = isFullsreen;
		
		resolutionDropdown.interactable = !isFullsreen;  // The user can't change to other resolutions if the game is fullscreen.

		PlayerPrefs.SetInt("IsFullscreen", isFullsreen ? 1 : 0);  // If true then return 1, else 0.
	}

	public void SetResolution(int index)
	{
		Resolution selectedResolution = resoArr[index];
		Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);

		if (index != fullscreenIndex)
			PlayerPrefs.SetInt("ResolutionIndex", index);
	}

	public void ResetToDefault()
	{
		// Perform reset.
		PlayerPrefs.SetInt("QualityLevel", 3);
		PlayerPrefs.SetInt("ResolutionIndex", 7);
		PlayerPrefs.SetInt("IsFullscreen", 0);
		PlayerPrefs.SetFloat("TargetFramerate", 120f);
		PlayerPrefs.SetInt("UseVsync", 0);

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
	}

	private void ReloadUI()
	{
		qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", 3);

		resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 7);

		fullscreenToggle.isOn = PlayerPrefs.GetInt("IsFullscreen", 0) == 1;

		framerateSlider.value = PlayerPrefs.GetFloat("TargetFramerate", 120f);

		vsyncToggle.isOn = PlayerPrefs.GetInt("UseVsync", 0) == 1;
	}
}
