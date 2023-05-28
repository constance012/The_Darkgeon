using System.IO;
using UnityEngine;
using CSTGames.CommonEnums;

/// <summary>
/// A scriptable object for creating a set of keys use in keybinding.
/// </summary>
[CreateAssetMenu(fileName = "New Keyset", menuName = "Keybinding/Keyset")]
public class Keyset : ScriptableObject
{
	[System.Serializable]
	public struct Key
	{
		public KeybindingActions action;
		public KeyCode keyCode;
	}

	public Key[] keyList;

	/// <summary>
	///  Save the Keyset's data into a json file. Create one if the file doesn't already exist.
	/// </summary>
	/// <param name="fileName"></param>
	public void SaveKeysetToJson(string fileName)
	{
		string persistentPath = Path.Combine(Application.persistentDataPath, "Keyset Data" + Path.DirectorySeparatorChar);

		Debug.Log(persistentPath);

		// Create directories if they're not currently existed.
		if (!Directory.Exists(persistentPath))
		{
			Directory.CreateDirectory(persistentPath);
		}

		persistentPath += fileName + ".json";

		Debug.Log("Saving data to " + persistentPath);
		
		string json = JsonUtility.ToJson(this, true);

		File.WriteAllText(persistentPath, json);

		UserSettings.SelectedKeyset = fileName;
	}

	/// <summary>
	/// Load data to the Keyset from a .json file. Use the default file if the previously selected file is missing.
	/// </summary>
	/// <param name="fileName"></param>
	public void LoadKeysetFromJson(string fileName)
	{
		string persistentPath = Path.Combine(Application.persistentDataPath, "Keyset Data", fileName + ".json");

		Debug.Log("Reading data at " + persistentPath);
		
		// If the custom keyset file exists.
		if (File.Exists(persistentPath))
		{
			string json = File.ReadAllText(persistentPath);
			
			JsonUtility.FromJsonOverwrite(json, this);
		}

		// If not, use the default file.
		else
		{
			fileName = "Default";

			persistentPath = Path.Combine(Application.persistentDataPath, "Keyset Data", fileName + ".json");

			// If the default file doesn't exist, create one.
			if (!File.Exists(persistentPath))
				SaveKeysetToJson(fileName);

			string json = File.ReadAllText(persistentPath);
			
			JsonUtility.FromJsonOverwrite(json, this);
		}
	}
}
