using CSTGames.DataPersistence;
using System.Collections;
using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsManager : MonoBehaviour, ISaveDataTransceiver
{
	public static LevelsManager instance { get; private set; }

	[Header("Debugging (Development Only)")]
	[Space]
	[ReadOnly] public bool developmentMode;

	public int currentLevelIndex { get; set; } = -1;
	public Scene currentScene { get; private set; }

	private LevelNavigation navigation;
	private bool transitionFromMenu;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Levels Manager found!! Destroy the newest one.");
			Destroy(gameObject);
			return;
		}
	}

	private IEnumerator Start()
	{
		developmentMode = !GameDataManager.instance.enableManager;

		if (developmentMode)
		{
			Debug.LogWarning("Levels Manager is currently in development mode.", this);
			currentScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
			SceneManager.SetActiveScene(currentScene);

			currentLevelIndex = currentScene.buildIndex;

			yield break;
		}

		yield return new WaitUntil(() => currentLevelIndex != -1);
		LoadLevel(LevelNavigation.LastPlayed);
	}

	public void SaveData(GameData gameData)
	{
		gameData.levelData.levelIndex = this.currentLevelIndex;
		gameData.levelData.levelName = this.currentScene.name;

		switch (navigation)
		{
			case LevelNavigation.Next:
				gameData.playerData.lastPlayedLevel = this.currentLevelIndex + 1;
				break;
			
			case LevelNavigation.Previous:
				gameData.playerData.lastPlayedLevel = this.currentLevelIndex - 1;
				break;
		}	
	}

	public void LoadData(GameData gameData) { }

	public void RestartLevel()
	{
		Debug.Log("Restarting level...");
		LoadLevel(LevelNavigation.Restart);
	}

	public void LoadNextLevel()
	{
		Debug.Log("Entering the next level...");
		LoadLevel(LevelNavigation.Next);
	}

	public void LoadPreviousLevel()
	{
		Debug.Log("Entering the previous level...");
		LoadLevel(LevelNavigation.Previous);
	}

	private void LoadLevel(LevelNavigation navigation)
	{
		this.navigation = navigation;

		switch (navigation)
		{
			case LevelNavigation.Next:
				GameDataManager.instance.SaveGame();
				
				currentLevelIndex++;
				SceneManager.UnloadSceneAsync(currentLevelIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
				break;

			case LevelNavigation.Previous:
				GameDataManager.instance.SaveGame();
				
				currentLevelIndex--;
				SceneManager.UnloadSceneAsync(currentLevelIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
				break;

			case LevelNavigation.Restart:
				SceneManager.UnloadSceneAsync(currentLevelIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
				break;

			case LevelNavigation.LastPlayed:
				transitionFromMenu = true;
				break;
		}

		currentScene = SceneManager.GetSceneByBuildIndex(currentLevelIndex);

		if (!currentScene.isLoaded)
		{
			AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync(currentLevelIndex, LoadSceneMode.Additive);

			loadSceneOp.completed += OnLevelCompletedLoading;
		}
		else
			OnLevelCompletedLoading(null);
	}

	/// <summary>
	/// Callback method when the new level level has been asynchronously loaded.
	/// </summary>
	/// <param name="obj"></param>
	private void OnLevelCompletedLoading(AsyncOperation obj)
	{
		currentScene = SceneManager.GetSceneByBuildIndex(currentLevelIndex);
		SceneManager.SetActiveScene(currentScene);

		// Set the event camera for the level's world canvas.
		Canvas enemiesWorldCanvas = GameObject.FindWithTag("Enemies World Canvas").GetComponent<Canvas>();
		enemiesWorldCanvas.worldCamera = Camera.main;

		if (!transitionFromMenu)
		{
			GameDataManager.instance.LoadGame();
		}
		else
		{
			GameDataManager.instance.DistributeDataToScripts();
			transitionFromMenu = false;
		}
	}

	private enum LevelNavigation { Previous, Next, LastPlayed, Restart }
}
