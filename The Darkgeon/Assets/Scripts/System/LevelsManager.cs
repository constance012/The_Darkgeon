using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsManager : MonoBehaviour
{
	public static LevelsManager instance { get; private set; }

	private int currentLevel = 3;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Levels Manager found!!");
			Destroy(gameObject);
			return;
		}
	}

	private void Start()
	{
		if (!SceneManager.GetSceneByBuildIndex(currentLevel).isLoaded)
		{
			AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync(currentLevel, LoadSceneMode.Additive);
			loadSceneOp.completed += LoadSceneOp_completed;
		}
		else
			SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(currentLevel));
	}

	public void RestartLevel()
	{
		Debug.Log("Restarting level...");
		SceneManager.UnloadSceneAsync(currentLevel, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync(currentLevel, LoadSceneMode.Additive);
		loadSceneOp.completed += LoadSceneOp_completed;
	}

	public void LoadNextLevel()
	{
		Debug.Log("Entering the next level...");
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	private void LoadSceneOp_completed(AsyncOperation obj)
	{
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(currentLevel));
	}
}
