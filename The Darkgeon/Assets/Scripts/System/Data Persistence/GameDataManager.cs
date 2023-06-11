using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CSTGames.DataPersistence;
using Unity.VisualScripting;
using System.Security.Cryptography;
using System;
using static UnityEngine.Rendering.DebugUI;

public class GameDataManager : MonoBehaviour
{
	public static GameDataManager instance { get; private set; }

	[Header("Debugging")]
	[Space]
	public bool initializeDataIfNull;
	public bool saveDataOnExit;
	[field: SerializeField]
	public string SelectedSaveSlotID { get; set; } = "";

	[Header("Save files configuration")]

	[Header("Player Data")]
	[Space]
	[SerializeField] private string playerSubFolders;
	[SerializeField] private string playerSaveFileName;

	[Header("Levels Data")]
	[Space]
	[SerializeField] private string levelSubFolders;
	private string levelSaveFileName = "";

	[HideInInspector] public bool useEncryption;

	private DateTime dataLoadDateTime;
	private GameData gameData;
	private SaveFileHandler<PlayerData> playerSaveHandler;
	private SaveFileHandler<LevelData> levelSaveHandler;

	/// <summary>
	/// A list of all the scripts that implemented the IDataPersistence interface.
	/// </summary>
	private List<ISaveDataTransceiver> dataTransceiverObjects;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogError("More than 1 instance of Game Data Manager found!! Destroy the newest one.");
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(this.gameObject);

		this.playerSaveHandler = new SaveFileHandler<PlayerData>(
			Application.persistentDataPath, playerSubFolders, playerSaveFileName, useEncryption);

		this.levelSaveHandler = new SaveFileHandler<LevelData>(
			Application.persistentDataPath, levelSubFolders, levelSaveFileName, useEncryption);

		SelectedSaveSlotID = GetMostRecentlyUpdatedSaveSlot();
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	// TODO: This method is used in development only, remove it before build the game.
	private void OnApplicationQuit()
	{
		if (saveDataOnExit)
			SaveGame();
	}

	public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Debug.Log($"Loaded scene: {scene.name}", this);
		this.dataTransceiverObjects = GetAllTransceivers();

		// TODO - player data has to be loaded first.
		if (scene.name.Equals("Base Scene"))
		{
			LevelsManager.instance.currentLevelIndex = this.gameData.playerData.lastPlayedLevel;
		}
	}

	#region Game Data Management.
	public void UpdateLevelSaveFile(int levelIndex)
	{
		string[] sceneSplitPath = SceneUtility.GetScenePathByBuildIndex(levelIndex).Split('\\', '/', '.');
		levelSaveFileName = sceneSplitPath[sceneSplitPath.Length - 2].ToLower() + ".cst";

		levelSaveHandler.fileName = string.Copy(levelSaveFileName);
	}

	// Initiate a new game with all default newTotalPlayedTimes.
	public void NewGame()
	{
		// Store the time when this new data is loaded.
		dataLoadDateTime = DateTime.Now;

		gameData = new GameData();

		UpdateLevelSaveFile(gameData.playerData.lastPlayedLevel);
	}

	public void LoadGame(bool distributeData = true)
	{
		dataLoadDateTime = DateTime.Now;

		// TODO: Load data from the json file, push it to the gameData object.
		this.gameData = new GameData(true);

		Debug.Log($"Loading data for slot {SelectedSaveSlotID}", this);
		this.gameData.playerData = playerSaveHandler.LoadDataFromFile(SelectedSaveSlotID);

		Debug.Log("Player data loaded successfully: " + gameData.hasPlayerData, this);

		if (!this.gameData.hasPlayerData)
		{
			if (initializeDataIfNull)
			{
				Debug.Log("DEBUGGING: No player data was found. Starting a new game.");
				NewGame();
			}
			else
			{
				Debug.LogError($"CRITICAL: Loading data failed for slot {SelectedSaveSlotID}, the data might have been modified or corrupted.\n");
				return;
			}
		}

		UpdateLevelSaveFile(gameData.playerData.lastPlayedLevel);
		this.gameData.levelData = levelSaveHandler.LoadDataFromFile(SelectedSaveSlotID);

		if (distributeData)
			DistributeDataToScripts();
	}

	public void SaveGame()
	{
		if (!this.gameData.allDataLoadedSuccessfully)
		{
			Debug.LogError("CAN NOT SAVE because of missing data or data corruption.");
			return;
		}

		// TODO: Notify all the scripts to write their data into the game data object.
		foreach (ISaveDataTransceiver obj in dataTransceiverObjects)
		{
			obj.SaveData(gameData);
		}

		TimestampData();

		// TODO: Save all that data into a file on the local machine.
		playerSaveHandler.SaveDataToFile(gameData.playerData, SelectedSaveSlotID);
		levelSaveHandler.SaveDataToFile(gameData.levelData, SelectedSaveSlotID);
	}

	public void DistributeDataToScripts()
	{
		// TODO: Distribute the game data to all the scripts that need it.
		foreach (ISaveDataTransceiver obj in dataTransceiverObjects)
			obj.LoadData(gameData);
	}

	public void TimestampData()
	{
		// Timestamp the data so we know when it was last saved.
		gameData.playerData.lastUpdated = DateTime.Now.ToBinary();

		// Update the total played time by increasing the old newTotalPlayedTime.
		TimeSpan currentPlayedTime = DateTime.Now - dataLoadDateTime;

		int totalHours = Mathf.FloorToInt((float)currentPlayedTime.TotalHours);
		Vector3Int newTotalPlayedTime = gameData.playerData.TotalPlayedTime + new Vector3Int(totalHours, currentPlayedTime.Minutes, currentPlayedTime.Seconds);

		newTotalPlayedTime.y += newTotalPlayedTime.z / 60;
		newTotalPlayedTime.z -= 60 * (newTotalPlayedTime.z / 60);

		newTotalPlayedTime.x += newTotalPlayedTime.y / 60;
		newTotalPlayedTime.y -= 60 * (newTotalPlayedTime.y / 60);

		gameData.playerData.TotalPlayedTime = newTotalPlayedTime;
	}
	#endregion

	#region Save Slots Management.
	public Dictionary<string, GameData> LoadAllSaveSlotsData()
	{
		Dictionary<string, GameData> saveSlots = new Dictionary<string, GameData>();

		IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(Application.persistentDataPath).EnumerateDirectories();

		foreach(DirectoryInfo dirInfo in dirInfos)
		{
			string saveSlotID = dirInfo.Name;
			string playerFullPath = Path.Combine(Application.persistentDataPath, saveSlotID, playerSubFolders, playerSaveFileName);
			
			if (!File.Exists(playerFullPath))
			{
				Debug.LogWarning($"Folder {saveSlotID} does not contain data, ignoring this folder.");
				continue;
			}

			GameData data = new GameData(true); 
			data.playerData = playerSaveHandler.LoadDataFromFile(saveSlotID);

			UpdateLevelSaveFile(data.playerData.lastPlayedLevel);
			data.levelData = levelSaveHandler.LoadDataFromFile(saveSlotID);

			if (data.allDataLoadedSuccessfully)
				saveSlots.Add(saveSlotID, data);
			else
				Debug.LogError($"CRITICAL: Loading data failed for slot {saveSlotID}, the data might have been modified or corrupted.");
		}

		levelSaveFileName = "";

		return saveSlots;
	}

	public string GetMostRecentlyUpdatedSaveSlot()
	{
		string mostRecentID = null;

		Dictionary<string, GameData> saveSlotsData = LoadAllSaveSlotsData();

		foreach(KeyValuePair<string, GameData> pair in saveSlotsData)
		{
			string slotID = pair.Key;
			GameData data = pair.Value;

			// Defensive check - skip if this data is somehow null.
			if (data == null)
				continue;

			// If this is the first data that exists, then it's the most recent so far.
			if (mostRecentID == null)
				mostRecentID = slotID;
			
			// Otherwise, compare to get the most recent date.
			else
			{
				DateTime mostRecentTime = DateTime.FromBinary(saveSlotsData[mostRecentID].playerData.lastUpdated);
				DateTime newTime = DateTime.FromBinary(data.playerData.lastUpdated);

				if (newTime > mostRecentTime)
					mostRecentID = slotID;
			}
		}

		return mostRecentID;
	}
	#endregion

	#region Custom Editor Methods.
	public void EncryptManually()
	{
		// Only encrypt once.
		if (useEncryption)
			return;

		useEncryption = true;
		GameData data = new GameData(true);

		SaveFileHandler<PlayerData> playerHandler = new SaveFileHandler<PlayerData>(
			Application.persistentDataPath, playerSubFolders, playerSaveFileName, false);		
		
		data.playerData = playerHandler.LoadDataFromFile(SelectedSaveSlotID);
		UpdateLevelSaveFile(data.playerData.lastPlayedLevel);
		
		SaveFileHandler<LevelData> levelHandler = new SaveFileHandler<LevelData>(
			Application.persistentDataPath, levelSubFolders, levelSaveFileName, false);

		data.levelData = levelHandler.LoadDataFromFile(SelectedSaveSlotID);

		playerHandler.useEncryption = this.useEncryption;
		levelHandler.useEncryption = this.useEncryption;

		playerHandler.SaveDataToFile(data.playerData, SelectedSaveSlotID);
		levelHandler.SaveDataToFile(data.levelData, SelectedSaveSlotID);
	}

	public void DecryptManually()
	{
		// Only decrypt once.
		if (!useEncryption)
			return;

		useEncryption = false;
		GameData data = new GameData(true);

		SaveFileHandler<PlayerData> playerHandler = new SaveFileHandler<PlayerData>(
			Application.persistentDataPath, playerSubFolders, playerSaveFileName, true);

		data.playerData = playerHandler.LoadDataFromFile(SelectedSaveSlotID);
		UpdateLevelSaveFile(data.playerData.lastPlayedLevel);
		
		SaveFileHandler<LevelData> levelHandler = new SaveFileHandler<LevelData>(
			Application.persistentDataPath, levelSubFolders, levelSaveFileName, true);
		
		data.levelData = levelHandler.LoadDataFromFile(SelectedSaveSlotID);

		playerHandler.useEncryption = this.useEncryption;
		levelHandler.useEncryption = this.useEncryption;

		playerHandler.SaveDataToFile(data.playerData, SelectedSaveSlotID);
		levelHandler.SaveDataToFile(data.levelData, SelectedSaveSlotID);
	}
	#endregion

	private List<ISaveDataTransceiver> GetAllTransceivers()
	{
		IEnumerable<ISaveDataTransceiver> dataTransceiverObjects = FindObjectsOfType<MonoBehaviour>(true).
																OfType<ISaveDataTransceiver>();

		return new List<ISaveDataTransceiver>(dataTransceiverObjects);
	}
}
