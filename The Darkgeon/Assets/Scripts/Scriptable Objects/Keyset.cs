using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using CSTGames.CommonEnums;
using System.Threading;
using System.Dynamic;

/// <summary>
/// A scriptable object for creating a set of keys use in keybinding.
/// </summary>
[CreateAssetMenu(fileName = "New Keyset", menuName = "Keybinding/Keyset")]
public class Keyset : ScriptableObject
{
	[Serializable]
	public struct Key
	{
		public KeybindingActions action;
		public KeyCode keyCode;
	}

	[Serializable]
	public class KeysList
	{
		public Key[] list;

		public int TotalKeys => list.Length;
		public int LastIndex => TotalKeys - 1;
		
		public Key this[int index]
		{
			get { return list[index]; }
			set { list[index] = value; }
		}
	}

	[Header("List of keys")]
	[Space]
	public KeysList keys;

	[Header("Data Saving Configuration")]
	[Space]
	public string subFolders = "";
	public string extension = "";

	private SaveFileHandler<KeysList> _saveHandler;
	private string _fullPath;

	public void SetKeycodeAt(int index, KeyCode keyCode)
	{
		keys.list[index].keyCode = keyCode;
	}

	/// <summary>
	///  Save the Keyset's data into a json file. Create one if the file doesn't already exist.
	/// </summary>
	/// <param name="fileName"></param>
	public void SaveKeysetToJson(string fileName)
	{
		ManageSaveHandler(fileName);

		Debug.Log("Saving data to " + _fullPath);

		_saveHandler.SaveDataToFile(keys);

		UserSettings.SelectedKeyset = fileName;
	}

	/// <summary>
	/// Load data to the Keyset from a .json file. Use the default file if the previously selected file is missing.
	/// </summary>
	/// <param name="fileName"></param>
	public void LoadKeysetFromJson(string fileName)
	{
		ManageSaveHandler(fileName);

		Debug.Log("Reading data at " + _fullPath);
		
		// If the custom keyset file exists.
		if (File.Exists(_fullPath))
		{
			keys = _saveHandler.LoadDataFromFile();
		}

		// If not, use the default file.
		else
		{
			fileName = "Default";

			ManageSaveHandler(fileName, true);

			// If the default file doesn't exist, create one.
			if (!File.Exists(_fullPath))
				SaveKeysetToJson(fileName);

			keys = _saveHandler.LoadDataFromFile();
		}
	}

	private void ManageSaveHandler(string fileName, bool update = false)
	{
		string directory = Application.persistentDataPath;

		if (_saveHandler == null || update)
			_saveHandler = new SaveFileHandler<KeysList>(directory, subFolders, fileName + extension, false);

		_fullPath = Path.Combine(directory, subFolders, fileName + extension);
	}
}
