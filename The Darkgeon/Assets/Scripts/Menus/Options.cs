using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// A class contains all the functions for the Options Menu.
/// </summary>
public class Options : MonoBehaviour
{
	[Header("References")]
	[Space]

	[Header("Game Objects")]
	[Space]
	[SerializeField] private GameObject ratPrefab;
	[SerializeField] private GameObject optionsPanel;
	[SerializeField] private Transform spawnerPos;

	[Header("Animators")]
	[Space]
	[SerializeField] private Animator mainCamera;

	private bool backToMenu;

	private void Awake()
	{
		optionsPanel = transform.Find("Options Panel").gameObject;
		spawnerPos = GameObject.Find("Rat Spawner").transform;

		mainCamera = GameObject.Find("Stationary Cam").GetComponent<Animator>();
	}

	private void Start()
	{
		if (MainMenu.isRatAlive)
			SpawnRat();

		FadeOutPanel.FadeIn();
	}

	private void Update()
	{
		if (!MainMenu.isRatAlive)
		{
			SpawnRat();
			MainMenu.isRatAlive = true;
		}
	}

	private void SpawnRat()
	{
		Instantiate(ratPrefab, spawnerPos.position, Quaternion.identity);
	}

	public void Back()
	{
		if (!backToMenu)
		{
			StartCoroutine(LoadMainMenu());
			backToMenu = true;
		}
	}

	private IEnumerator LoadMainMenu()
	{
		AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync("Scenes/Main Menu");
		loadSceneOp.allowSceneActivation = false;

		optionsPanel.GetComponent<Animator>().SetTrigger("Fade Out");

		CanvasGroup canvasGroup = optionsPanel.GetComponent<CanvasGroup>();

		yield return new WaitUntil(() => canvasGroup.alpha == 0f);

		mainCamera.SetTrigger("Pan Up");
		FadeOutPanel.FadeOut();

		// Activate the scene when the fading process is completed.
		yield return new WaitUntil(() => FadeOutPanel.alpha == 1f);

		loadSceneOp.allowSceneActivation = true;
	}
}
