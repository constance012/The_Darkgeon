using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
	[SerializeField] private Animator fadeoutPanel;
	[SerializeField] private Animator mainCamera;

	private void Awake()
	{
		optionsPanel = transform.Find("Options Panel").gameObject;
		spawnerPos = GameObject.Find("Rat Spawner").transform;

		fadeoutPanel = transform.Find("Fade Out Panel").GetComponent<Animator>();
		mainCamera = GameObject.Find("Stationary Cam").GetComponent<Animator>();
	}

	private void Start()
	{
		if (MainMenu.isRatAlive)
			SpawnRat();
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
		StartCoroutine(LoadMainMenu());
	}

	private IEnumerator LoadMainMenu()
	{
		AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync("Scenes/Menu");
		loadSceneOp.allowSceneActivation = false;

		optionsPanel.GetComponent<Animator>().SetTrigger("Fade Out");

		yield return new WaitUntil(() => optionsPanel.GetComponent<CanvasGroup>().alpha == 0f);

		mainCamera.SetTrigger("Pan Up");
		fadeoutPanel.SetTrigger("Fade Out");

		// Activate the scene when the fading process is completed.
		yield return new WaitUntil(() => fadeoutPanel.GetComponent<Image>().color.a == 1f);

		loadSceneOp.allowSceneActivation = true;
	}
}
