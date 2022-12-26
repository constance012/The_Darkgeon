using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private GameObject ratPrefab;
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
	}

	private void Start()
	{
		Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.ForceSoftware);
	}

	private void Update()
	{
		if (!isRatAlive)
		{
			SpawnRat();
			isRatAlive = true;
		}

		if (IsTouchingMouse(onScreenRat))
		{
			Cursor.SetCursor(swordCursor, Vector2.zero, CursorMode.Auto);
			
			if (Input.GetMouseButtonDown(0))
			{
				onScreenRat.GetComponent<MenuRatBehavior>().Die();
				isRatAlive = false;
			}
		}
		else
			Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
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
		SceneManager.LoadSceneAsync("Scenes/Base Scene");
	}

	public void Quit()
	{
		Debug.Log("Quiting...");
		Application.Quit();
	}
}
