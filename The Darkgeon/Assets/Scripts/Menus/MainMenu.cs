using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenu : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private GameObject ratPrefab;
	[SerializeField] private TextMeshProUGUI starterText;
	[SerializeField] private GameObject menuPanel;
	[SerializeField] private Animator fadeoutPanel;
	[SerializeField] private Transform spawnerPos;

	public static bool isRatAlive { get; set; }

	private void Awake()
	{
		spawnerPos = GameObject.Find("Rat Spawner").transform;
		starterText = transform.Find("Starter Text").GetComponent<TextMeshProUGUI>();
		menuPanel = transform.Find("Menu Panel").gameObject;
		fadeoutPanel = transform.Find("Fade Out Panel").GetComponent<Animator>();
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
		StartCoroutine(LoadGameScene(" Scenes/Base Scene  "));
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

	private IEnumerator LoadGameScene(string sceneName)
	{
		AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync(sceneName.Trim());
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
}
