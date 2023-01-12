using System.IO;
using UnityEngine;

/// <summary>
/// A scriptable object for creating a set of keys use in keybinding.
/// </summary>
[CreateAssetMenu(fileName = "New Keyset", menuName = "Keybinding/Keyset")]
public class Keyset : ScriptableObject
{
	[System.Serializable]
	public class Key
	{
		public KeybindingActions action;
		public KeyCode keyCode;
	}

	public Key[] keyList;

	public void SaveKeysetToJson(string fileName)
	{
		string path = Application.streamingAssetsPath + "/Keyset Data/";
		string persistentPath = Application.persistentDataPath + "/Keyset Data/";

		// Create directories if they're not currently existed.
		if (!Directory.Exists(path) || !Directory.Exists(persistentPath))
		{
			Directory.CreateDirectory(path);
			Directory.CreateDirectory(persistentPath);
		}

		path += fileName + ".json";
		persistentPath += fileName + ".json";

		Debug.Log("Saving data at " + path);
		//Debug.Log("Saving persistent data at " + persistentPath);
		string json = JsonUtility.ToJson(this, true);
		//Debug.Log(json);

		File.WriteAllText(path, json);
		File.WriteAllText(persistentPath, json);

		PlayerPrefs.SetString("SelectedKeyset", fileName);
	}

	public void LoadKeysetFromJson(string fileName)
	{
		string path = Application.streamingAssetsPath + "/Keyset Data/" + fileName + ".json";
		string persistentPath = Application.persistentDataPath + "/Keyset Data/" + fileName + ".json";

		// If the custom keyset file exists.
		if (File.Exists(path) && File.Exists(persistentPath))
		{
			string json = File.ReadAllText(path);
			JsonUtility.FromJsonOverwrite(json, this);
		}

		// If not, use the default file.
		else
		{
			fileName = "Keyset_Default";

			path = Application.streamingAssetsPath + "/Keyset Data/" + fileName + ".json";
			persistentPath = Application.persistentDataPath + "/Keyset Data/" + fileName + ".json";

			// If the default file doesn't exist, create one.
			if (!File.Exists(path) || !File.Exists(persistentPath))
				SaveKeysetToJson(fileName);

			string json = File.ReadAllText(path);
			JsonUtility.FromJsonOverwrite(json, this);
		}
	}
}
