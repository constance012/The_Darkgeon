using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CSTGames.DataPersistence;
using System;

public class GameDataManager : Singleton<GameDataManager>
{
	[Header("Debugging (Development Only)")]
	[Space]
	[ReadOnly] public bool enableManager;
	public bool initializeDataIfNull;
	public bool saveDataOnExit;
	[field: SerializeField, ReadOnly]
	public string SelectedSaveSlotID { get; set; } = "";

	[Header("Save files configuration")]

	[Header("Player Data")]
	[Space]
	[SerializeField] private string playerSubFolders;
	[SerializeField] private string playerSaveFileName;

	[Header("Levels Data")]
	[Space]
	[SerializeField] private string levelSubFolders;
	[SerializeField, ReadOnly] private string levelSaveFileName = "";

	[HideInInspector] public bool useEncryption;
	public static bool ContainsAnyData { get; set; }

	// Private fields.
	private DateTime _dataLoadDateTime;
	private GameData _currentGameData;
	private SaveFileHandler<PlayerData> _playerSaveHandler;
	private SaveFileHandler<LevelData> _levelSaveHandler;

	/// <summary>
	/// A list of all the scripts that implemented the IDataPersistence interface.
	/// </summary>
	private List<ISaveDataTransceiver> _dataTransceivers;

	protected override void Awake()
	{
		base.Awake();

		DontDestroyOnLoad(this.gameObject);

		if (!enableManager)
			Debug.LogWarning("DEBUGGING: Game Data Manager is currently disabled, game data will not be persisted between sessions.");

		this._playerSaveHandler = new SaveFileHandler<PlayerData>(
			Application.persistentDataPath, playerSubFolders, playerSaveFileName, useEncryption);

		this._levelSaveHandler = new SaveFileHandler<LevelData>(
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
		if (!enableManager)
			return;

		Debug.Log($"Loaded scene: {scene.name}", this);
		this._dataTransceivers = GetAllTransceivers();

		// TODO - player data has to be loaded first.
		if (scene.name.Equals("Base Scene"))
		{
			LevelsManager.instance.currentLevelIndex = this._currentGameData.playerData.lastPlayedLevel;
		}
	}

	#region Game Data Management.
	public void UpdateLevelSaveFile(int levelIndex)
	{
		string[] sceneSplitPath = SceneUtility.GetScenePathByBuildIndex(levelIndex).Split('\\', '/', '.');
		levelSaveFileName = sceneSplitPath[sceneSplitPath.Length - 2].ToLower() + ".cst";

		_levelSaveHandler.fileName = string.Copy(levelSaveFileName);
	}

	public void NewGame()
	{
		// Store the time when this new data is loaded.
		_dataLoadDateTime = DateTime.Now;

		_currentGameData = new GameData();

		UpdateLevelSaveFile(_currentGameData.playerData.lastPlayedLevel);
	}

	public void LoadGame(bool distributeData = true)
	{
		if (!enableManager)
			return;

		_dataLoadDateTime = DateTime.Now;

		// TODO: Load data from the json file, push it to the _currentGameData object.
		this._currentGameData = new GameData(true);

		Debug.Log($"Loading data for slot {SelectedSaveSlotID}", this);
		this._currentGameData.playerData = _playerSaveHandler.LoadDataFromFile(SelectedSaveSlotID);

		Debug.Log($"Player data loaded successfully: {_currentGameData.HasPlayerData}", this);

		if (!this._currentGameData.HasPlayerData)
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

		UpdateLevelSaveFile(_currentGameData.playerData.lastPlayedLevel);
		this._currentGameData.levelData = _levelSaveHandler.LoadDataFromFile(SelectedSaveSlotID);

		// If this level's file does not exist in the system to load, means this is a brand new level, then initiates it's default values.
		if (this._currentGameData.levelData == null)
		{
			int index = LevelsManager.instance.currentLevelIndex;
			string levelName = LevelsManager.instance.currentScene.name;
			this._currentGameData.levelData = new LevelData(index, levelName);
		}

		if (distributeData)
			DistributeDataToScripts();
	}

	public void SaveGame()
	{
		if (!enableManager)
			return;

		if (!this._currentGameData.AllDataLoadedSuccessfully)
		{
			Debug.LogError("CAN NOT SAVE because of missing data or data corruption.");
			return;
		}

		// TODO: Notify all the scripts to write their data into the game data object.
		foreach (ISaveDataTransceiver obj in _dataTransceivers)
		{
			obj.SaveData(_currentGameData);
		}

		TimestampData();

		// TODO: Save all that data into a file on the local machine.
		_playerSaveHandler.SaveDataToFile(_currentGameData.playerData, SelectedSaveSlotID);
		_levelSaveHandler.SaveDataToFile(_currentGameData.levelData, SelectedSaveSlotID);
		
		DialogueManager.instance?.SaveGlobalVariables();
	}

	public void DistributeDataToScripts()
	{
		// TODO: Distribute the game data to all the scripts that need it.
		foreach (ISaveDataTransceiver obj in _dataTransceivers)
			obj.LoadData(_currentGameData);
	}

	public void TimestampData()
	{
		// Timestamp the data so we know when it was last saved.
		_currentGameData.playerData.lastUpdated = DateTime.Now.ToBinary();

		// Update the total played time by increasing the old newTotalPlayedTime.
		TimeSpan currentPlayedTime = DateTime.Now - _dataLoadDateTime;

		int totalHours = Mathf.FloorToInt((float)currentPlayedTime.TotalHours);
		_currentGameData.playerData.TotalPlayedTime += new Vector3Int(totalHours, currentPlayedTime.Minutes, currentPlayedTime.Seconds);
	}

	private List<ISaveDataTransceiver> GetAllTransceivers()
	{
		IEnumerable<ISaveDataTransceiver> _dataTransceivers = FindObjectsOfType<MonoBehaviour>(true).
																OfType<ISaveDataTransceiver>();

		return new List<ISaveDataTransceiver>(_dataTransceivers);
	}
	#endregion

	#region Save Slots Management.
	public void DeleteSaveSlot(string saveSlotID)
	{
		if (saveSlotID == null || saveSlotID.Equals(""))
			return;

		string playerFullPath = Path.Combine(Application.persistentDataPath, saveSlotID, playerSubFolders, playerSaveFileName);
		string slotDirectory = Path.Combine(Application.persistentDataPath, saveSlotID);
		bool success = false;

		try
		{
			if (File.Exists(playerFullPath))
			{
				Directory.Delete(slotDirectory, true);
				success = true;
			}
			else
				Debug.LogWarning($"Tried to delete data at: {playerFullPath} but the data was not found.");

			if (success)
				SelectedSaveSlotID = GetMostRecentlyUpdatedSaveSlot();
		}
		catch (Exception ex)
		{
			Debug.LogError($"ERROR: Failed to delete slot {saveSlotID}.\n" +
						   $"Reason: {ex.Message}");
		}
	}

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
			data.playerData = _playerSaveHandler.LoadDataFromFile(saveSlotID);

			UpdateLevelSaveFile(data.playerData.lastPlayedLevel);
			data.levelData = _levelSaveHandler.LoadDataFromFile(saveSlotID);

			if (data.AllDataLoadedSuccessfully)
				saveSlots.Add(saveSlotID, data);
			else
				Debug.LogError($"CRITICAL: Loading data failed for slot {saveSlotID}, the data might have been modified or corrupted.");
		}

		levelSaveFileName = "";
		ContainsAnyData = saveSlots.Count > 0;

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
				DateTime currentTime = DateTime.FromBinary(data.playerData.lastUpdated);

				if (currentTime > mostRecentTime)
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

		GameData data = new GameData(true);
		
		SaveFileHandler<PlayerData> playerHandler = new SaveFileHandler<PlayerData>(
			Application.persistentDataPath, playerSubFolders, playerSaveFileName, false);		

		SaveFileHandler<LevelData> levelHandler = new SaveFileHandler<LevelData>(
			Application.persistentDataPath, levelSubFolders, levelSaveFileName, false);

		IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(Application.persistentDataPath).EnumerateDirectories();

		// Loop through each save slot.
		foreach (DirectoryInfo dirInfo in dirInfos)
		{
			string saveSlotID = dirInfo.Name;
			string playerFullPath = Path.Combine(Application.persistentDataPath, saveSlotID, playerSubFolders, playerSaveFileName);
			string levelsDataFolder = Path.Combine(Application.persistentDataPath, saveSlotID, levelSubFolders);

			if (!File.Exists(playerFullPath))
			{
				Debug.LogWarning($"Folder {saveSlotID} does not contain data, ignoring this folder.");
				continue;
			}

			playerHandler.useEncryption = false;
			data.playerData = playerHandler.LoadDataFromFile(saveSlotID);
		
			playerHandler.useEncryption = true;
			playerHandler.SaveDataToFile(data.playerData, saveSlotID);

			// Encrypt all level data files of the current save slot.
			string[] levelFiles = Directory.GetFiles(levelsDataFolder, "*.cst");
			
			foreach (string levelFile in levelFiles)
			{
				string[] splitNames = levelFile.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				levelHandler.fileName = splitNames[splitNames.Length - 1];

				levelHandler.useEncryption = false;
				data.levelData = levelHandler.LoadDataFromFile(saveSlotID);
				
				levelHandler.useEncryption = true;
				levelHandler.SaveDataToFile(data.levelData, saveSlotID);
			}
		}

		useEncryption = true;
	}

	public void DecryptManually()
	{
		// Only decrypt once.
		if (!useEncryption)
			return;

		GameData data = new GameData(true);

		SaveFileHandler<PlayerData> playerHandler = new SaveFileHandler<PlayerData>(
			Application.persistentDataPath, playerSubFolders, playerSaveFileName, true);
		
		SaveFileHandler<LevelData> levelHandler = new SaveFileHandler<LevelData>(
			Application.persistentDataPath, levelSubFolders, levelSaveFileName, true);

		IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(Application.persistentDataPath).EnumerateDirectories();

		foreach (DirectoryInfo dirInfo in dirInfos)
		{
			string saveSlotID = dirInfo.Name;
			string playerFullPath = Path.Combine(Application.persistentDataPath, saveSlotID, playerSubFolders, playerSaveFileName);
			string levelsDataFolder = Path.Combine(Application.persistentDataPath, saveSlotID, levelSubFolders);

			if (!File.Exists(playerFullPath))
			{
				Debug.LogWarning($"Folder {saveSlotID} does not contain data, ignoring this folder.");
				continue;
			}

			playerHandler.useEncryption = true;
			data.playerData = playerHandler.LoadDataFromFile(saveSlotID);

			playerHandler.useEncryption = false;
			playerHandler.SaveDataToFile(data.playerData, saveSlotID);

			// Encrypt all level data files of the current save slot.
			string[] levelFiles = Directory.GetFiles(levelsDataFolder, "*.cst");

			foreach (string levelFile in levelFiles)
			{
				string[] splitNames = levelFile.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				levelHandler.fileName = splitNames[splitNames.Length - 1];

				levelHandler.useEncryption = true;
				data.levelData = levelHandler.LoadDataFromFile(saveSlotID);

				levelHandler.useEncryption = false;
				levelHandler.SaveDataToFile(data.levelData, saveSlotID);
			}
		}

		useEncryption = false;
	}
	#endregion
}
