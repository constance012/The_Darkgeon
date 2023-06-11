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
	[Header("Menu Navigation")]
	[Space]
	[SerializeField] private SaveSlotsMenu saveSlotsMenu;

	[Header("Audio Mixer")]
	[Space]
	[SerializeField] private AudioMixer mixer;

	[Header("Keyset")]
	[Space]
	[SerializeField] private Keyset keySet;

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

	public Action loadGameSceneEvent;

	public static bool isRatAlive { get; set; }
	public static bool isIntroDone { get; private set; }
	public static bool isThingsSet { get; private set; }

	private void Awake()
	{
		saveSlotsMenu = transform.root.GetComponentInChildren<SaveSlotsMenu>(true);

		menuPanel = transform.Find("Menu Panel").gameObject;
		spawnerPos = GameObject.Find("Rat Spawner").transform;
		
		fadeoutPanel = transform.root.Find("Fade Out Panel").GetComponent<Animator>();
		mainCamera = GameObject.Find("Stationary Cam").GetComponent<Animator>();
		
		starterText = transform.Find("Starter Text").GetComponent<TextMeshProUGUI>();

		loadGameSceneEvent = () => StartCoroutine(LoadGameScene());
	}

	private void Start()
	{
		if (!isThingsSet)
			SettingThingsUp();
		
		if (!isIntroDone)
			menuPanel.SetActive(false);

		if (isRatAlive)
			SpawnRat();
	}

	private void Update()
	{
		if (!isIntroDone)
		{
			if (Time.timeSinceLevelLoad > 2f && Time.timeSinceLevelLoad < 2.1f)
				starterText.GetComponent<Animator>().Play("Slide In");

			if (Input.GetMouseButtonDown(0) && Time.timeSinceLevelLoad > 2.5f)
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

	/// <summary>
	/// Callback method for the laod game button.
	/// </summary>
	public void LoadGame()
	{
		saveSlotsMenu.ActivateMenu();
		this.gameObject.SetActive(false);
	}

	/// <summary>
	/// Callback method for the continue button.
	/// </summary>
	public void Continue()
	{
		GameDataManager.instance.LoadGame(false);
		loadGameSceneEvent?.Invoke();
	}

	/// <summary>
	/// Callback method for the options button.
	/// </summary>
	public void Options()
	{
		StartCoroutine(LoadOptionsMenu());
	}

	/// <summary>
	/// Callback method for the quit button.
	/// </summary>
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
		float masterVol = UserSettings.MasterVolume;
		float musicVol = UserSettings.MusicVolume;
		float soundsVol = UserSettings.SoundsVolume;

		mixer.SetFloat("masterVol", masterVol);
		mixer.SetFloat("musicVol", musicVol);
		mixer.SetFloat("soundsVol", soundsVol);

		// Graphics.
		GraphicsOptionPage.resoArr = Screen.resolutions;
		QualitySettings.SetQualityLevel(UserSettings.QualityLevel);
		Application.targetFrameRate = Convert.ToInt32(UserSettings.TargetFramerate);
		QualitySettings.vSyncCount = UserSettings.UseVsync ? 1 : 0;

		// Controls
		string keysetFile = UserSettings.SelectedKeyset;
		Debug.Log("Load keyset from file: " + keysetFile);
		keySet.LoadKeysetFromJson(keysetFile);

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

	public IEnumerator LoadGameScene()
	{
		AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync("Scenes/Base Scene");
		loadSceneOp.allowSceneActivation = false;

		menuPanel.GetComponent<Animator>().SetTrigger("Fade Out");
		CanvasGroup menuCanvasGroup = menuPanel.GetComponent<CanvasGroup>();

		yield return new WaitUntil(() => menuCanvasGroup.alpha == 0f);

		FindObjectOfType<MenuTorch>().Extinguish();

		yield return new WaitForSeconds(1f);

		fadeoutPanel.SetTrigger("Fade Out");
		Image fadeoutImage = fadeoutPanel.GetComponent<Image>();

		// Activate the scene when the fading process is completed.
		yield return new WaitUntil(() => fadeoutImage.color.a == 1f);
		
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
