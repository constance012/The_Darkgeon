using CSTGames.DataPersistence;
using System.Collections;
using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsManager : MonoBehaviour, ISaveDataTransceiver
{
	public static LevelsManager instance { get; private set; }

	public int currentLevelIndex { get; set; } = -1;

	private Scene currentScene;
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
		yield return new WaitUntil(() => currentLevelIndex != -1);
		LoadLevel(LevelNavigation.LastPlayed);
	}

	public void SaveData(GameData gameData)
	{
		gameData.levelData.levelIndex = this.currentLevelIndex;
		gameData.levelData.levelName = this.currentScene.name;

		gameData.playerData.lastPlayedLevel = this.currentLevelIndex;
	}

	public void LoadData(GameData gameData)
	{
		
	}

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
		switch (navigation)
		{
			case LevelNavigation.Next:
				SceneManager.UnloadSceneAsync(currentLevelIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
				currentLevelIndex++;
				break;

			case LevelNavigation.Previous:
				SceneManager.UnloadSceneAsync(currentLevelIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
				currentLevelIndex--;
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

			loadSceneOp.completed += OnSceneCompletedLoading;
		}
		else
			OnSceneCompletedLoading(null);
	}

	/// <summary>
	/// Callback method when the new level scene has been asynchronously loaded.
	/// </summary>
	/// <param name="obj"></param>
	private void OnSceneCompletedLoading(AsyncOperation obj)
	{
		currentScene = SceneManager.GetSceneByBuildIndex(currentLevelIndex);

		SceneManager.SetActiveScene(currentScene);

		if (!transitionFromMenu)
		{
			GameDataManager.instance.UpdateLevelSaveFile(currentLevelIndex);
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
