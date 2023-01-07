using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

/// <summary>
/// A class contains all the functions for the Main Menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
	[Header("References")]
	[Space]

	[SerializeField] private AudioMixer mixer;

	[Header("Game Objects")]
	[Space]
	[SerializeField] private GameObject ratPrefab;
	[SerializeField] private GameObject menuPanel;
	[SerializeField] private Transform spawnerPos;

	[Header("Animators")]
	[Space]
	[SerializeField] private Animator fadeoutPanel;
	[SerializeField] private Animator mainCamera;

	[Header("UI Elements")]
	[Space]
	[SerializeField] private TextMeshProUGUI starterText;

	public static bool isRatAlive { get; set; }
	public static bool isIntroDone { get; private set; }
	public static bool isThingsSet { get; private set; }

	private void Awake()
	{
		menuPanel = transform.Find("Menu Panel").gameObject;
		spawnerPos = GameObject.Find("Rat Spawner").transform;
		
		fadeoutPanel = transform.Find("Fade Out Panel").GetComponent<Animator>();
		mainCamera = GameObject.Find("Stationary Cam").GetComponent<Animator>();
		
		starterText = transform.Find("Starter Text").GetComponent<TextMeshProUGUI>();
	}

	private void Start()
	{
		if (!isThingsSet)
			SettingThingsUp();
		
		if (!isIntroDone)
			menuPanel.SetActive(false);

		starterText.gameObject.SetActive(false);

		if (isRatAlive)
			SpawnRat();
	}

	private void Update()
	{
		if (!isIntroDone)
		{
			if (Time.timeSinceLevelLoad > 2f && !starterText.gameObject.activeInHierarchy)
				starterText.gameObject.SetActive(true);

			if (Input.GetMouseButtonDown(0) && starterText.gameObject.activeInHierarchy)
				StartCoroutine(EnableUI());
		}

		if (!isRatAlive)
		{
			SpawnRat();
			isRatAlive = true;
		}
	}

	private void SpawnRat()
	{
		Instantiate(ratPrefab, spawnerPos.position, Quaternion.identity);
	}

	public void NewGame()
	{
		StartCoroutine(LoadGameScene());
	}

	public void Options()
	{
		StartCoroutine(LoadOptionsMenu());
	}

	public void Quit()
	{
		Debug.Log("Quiting...");
		Application.Quit();
	}

	/// <summary>
	/// Setting up essential things once when the game first start.
	/// </summary>
	private void SettingThingsUp()
	{
		Time.timeScale = 1f;
		// Audio.
		float masterVol = PlayerPrefs.GetFloat("MasterVolume", 0f);
		float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0f);
		float soundsVol = PlayerPrefs.GetFloat("SoundsVolume", 0f);

		mixer.SetFloat("masterVol", masterVol);
		mixer.SetFloat("musicVol", musicVol);
		mixer.SetFloat("soundsVol", soundsVol);

		// Graphics.
		GraphicsOptionPage.resoArr = Screen.resolutions;
		QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QualityLevel", 3));
		Application.targetFrameRate = Convert.ToInt32(PlayerPrefs.GetFloat("TargetFramerate", 60f));
		QualitySettings.vSyncCount = PlayerPrefs.GetInt("UseVsync", 0);

		Debug.Log("All things successfully set up.");

		isThingsSet = true;
	}

	private IEnumerator EnableUI()
	{
		starterText.GetComponent<Animator>().SetTrigger("Fade Out");
		FindObjectOfType<MenuTorch>().LightUp();

		yield return new WaitForSeconds(2f);

		menuPanel.SetActive(true);

		isIntroDone = true;
	}

	private IEnumerator LoadGameScene()
	{
		AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync("Scenes/Base Scene");
		loadSceneOp.allowSceneActivation = false;

		menuPanel.GetComponent<Animator>().SetTrigger("Fade Out");

		yield return new WaitUntil(() => menuPanel.GetComponent<CanvasGroup>().alpha == 0f);

		FindObjectOfType<MenuTorch>().Extinguish();

		yield return new WaitForSeconds(1f);

		fadeoutPanel.SetTrigger("Fade Out");

		// Activate the scene when the fading process is completed.
		yield return new WaitUntil(() => fadeoutPanel.GetComponent<Image>().color.a == 1f);
		
		loadSceneOp.allowSceneActivation = true;
	}

	private IEnumerator LoadOptionsMenu()
	{
		AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync("Scenes/Options");
		loadSceneOp.allowSceneActivation = false;

		menuPanel.GetComponent<Animator>().SetTrigger("Fade Out");

		yield return new WaitUntil(() => menuPanel.GetComponent<CanvasGroup>().alpha == 0f);

		mainCamera.SetTrigger("Pan Down");
		fadeoutPanel.SetTrigger("Fade Out");

		// Activate the scene when the fading process is completed.
		yield return new WaitUntil(() => fadeoutPanel.GetComponent<Image>().color.a == 1f);

		loadSceneOp.allowSceneActivation = true;
	}
}
