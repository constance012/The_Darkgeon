using JetBrains.Annotations;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private GameObject ratPrefab;
	[SerializeField] private TextMeshProUGUI starterText;
	[SerializeField] private GameObject menuPanel;
	[SerializeField] private Transform spawnerPos;

	[Header("Cursors")]
	[Space]
	[SerializeField] private Texture2D defaultCursor;
	[SerializeField] private Texture2D swordCursor;

	private GameObject onScreenRat;
	private bool isRatAlive;

	private void Awake()
	{
		spawnerPos = GameObject.Find("Rat Spawner").transform;
		starterText = transform.Find("Starter Text").GetComponent<TextMeshProUGUI>();
		menuPanel = transform.Find("Menu Panel").gameObject;
	}

	private void Start()
	{
		Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.ForceSoftware);
		menuPanel.SetActive(false);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && !menuPanel.activeInHierarchy)
		{
			StartCoroutine(EnableUI());
		}
		
		if (!isRatAlive)
		{
			SpawnRat();
			isRatAlive = true;
		}

		if (IsTouchingMouse(onScreenRat))
		{
			Cursor.SetCursor(swordCursor, new Vector2(5, 2), CursorMode.Auto);
			
			if (Input.GetMouseButtonDown(0))
			{
				onScreenRat.GetComponent<MenuRatBehavior>().Die();
				isRatAlive = false;
			}
		}
		else
			Cursor.SetCursor(defaultCursor, new Vector2(10, 5), CursorMode.Auto);
	}

	private bool IsTouchingMouse(GameObject rat)
	{
		Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		return rat.GetComponent<CircleCollider2D>().OverlapPoint(cursorPos);
	}

	private void SpawnRat()
	{
		onScreenRat = Instantiate(ratPrefab, spawnerPos.position, Quaternion.identity);
	}

	public void NewGame()
	{
		StartCoroutine(DisableUI());
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

		if (starterText.alpha == 0)
			starterText.gameObject.SetActive(false);
	}

	private IEnumerator DisableUI()
	{
		AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync("Scenes/Base Scene");
		loadSceneOp.allowSceneActivation = false;

		menuPanel.GetComponent<Animator>().SetTrigger("Fade Out");

		yield return new WaitForSeconds(1f);

		FindObjectOfType<MenuTorch>().Extinguish();

		yield return new WaitForSeconds(1f);

		loadSceneOp.allowSceneActivation = true;
	}
}
