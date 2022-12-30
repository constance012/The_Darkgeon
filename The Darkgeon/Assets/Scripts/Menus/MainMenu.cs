using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenu : MonoBehaviour
{
	[Header("References")]
	[Space]

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
		menuPanel.SetActive(false);
		starterText.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (Time.timeSinceLevelLoad > 2f && !starterText.gameObject.activeInHierarchy)
			starterText.gameObject.SetActive(true);

		if (Input.GetMouseButtonDown(0) && starterText.gameObject.activeInHierarchy)
			StartCoroutine(EnableUI());

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

	private IEnumerator EnableUI()
	{
		starterText.GetComponent<Animator>().SetTrigger("Fade Out");
		FindObjectOfType<MenuTorch>().LightUp();

		yield return new WaitForSeconds(2f);

		menuPanel.SetActive(true);
	}

	private IEnumerator LoadGameScene()
	{
		AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync("Scenes/Base Scene");
		loadSceneOp.allowSceneActivation = false;

		menuPanel.GetComponent<Animator>().SetTrigger("Fade Out");

		yield return new WaitUntil(() => menuPanel.GetComponent<CanvasGroup>().alpha == 0f);

		FindObjectOfType<MenuTorch>().Extinguish();

		yield return new WaitForSeconds(1f);

		fadeoutPanel.SetTrigger("Fading Out");

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
