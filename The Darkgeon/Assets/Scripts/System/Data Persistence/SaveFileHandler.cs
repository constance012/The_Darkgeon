﻿using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveFileHandler<TData>
{
	public string directory { get; set; } = ""; 
	public string subFolders { get; set; } = "";
	public string fileName { get; set; } = "";

	public bool useEncryption { get; set; }

	private const string ENCRYPTION_CODE = "hypoxia";

	public SaveFileHandler(string directory, string subFolders, string fileName, bool useEncryption)
	{
		this.directory = directory;
		this.subFolders = subFolders;
		this.fileName = fileName;

		this.useEncryption = useEncryption;
	}

	/// <summary>
	/// Load the data from a save slot using Newtonsoft's JSON.Net.
	/// </summary>
	/// <param name="saveSlotID"></param>
	/// <returns>The loaded TData</returns>
	public TData LoadDataFromFile(string saveSlotID)
	{
		if (saveSlotID == null)
			return default;

		string fullPath = Path.Combine(directory, saveSlotID, subFolders, fileName);

		TData loadedData = default;

		if (File.Exists(fullPath))
		{
			try
			{
				string serializedData = "";

				// Read the serialized data.
				using (FileStream file = new FileStream(fullPath, FileMode.Open))
				{
					using (StreamReader reader = new StreamReader(file))
					{
						serializedData = reader.ReadToEnd();
					}
				}

				// Decrypt the data (Optional).
				if (useEncryption)
					serializedData = EncryptOrDecrypt(serializedData);
				
				// Deserialize that data from a json formatted string back into object's data.
				loadedData = JsonConvert.DeserializeObject<TData>(serializedData);
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error occured when trying to load data from file.\n" +
						   $"At full path: {fullPath}.\n" +
						   $"Reason: {ex.Message}.");
			}
		}

		return loadedData;
	}

	/// <summary>
	/// Load the data from a standalone file using Unity's JsonUtility.
	/// </summary>
	/// <returns>The loaded TData</returns>
	public TData LoadDataFromFile()
	{
		string fullPath = Path.Combine(directory, subFolders, fileName);
		TData loadedData = default;

		if (File.Exists(fullPath))
		{
			try
			{
				string json = "";

				// Read the serialized data.
				using (FileStream file = new FileStream(fullPath, FileMode.Open))
				{
					using (StreamReader reader = new StreamReader(file))
					{
						json = reader.ReadToEnd();
					}
				}

				// Decrypt the data (Optional).
				if (useEncryption)
					json = EncryptOrDecrypt(json);

				// Deserialize data.
				loadedData = JsonUtility.FromJson<TData>(json);
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error occured when trying to load json from file.\n" +
						   $"At full path: {fullPath}.\n" +
						   $"Reason: {ex.Message}.");
			}
		}

		return loadedData;
	}

	/// <summary>
	/// Save the data of a save slot using Newtonsoft's JSON.Net.
	/// </summary>
	/// <param name="dataToSave"></param>
	/// <param name="saveSlotID"></param>
	public void SaveDataToFile(TData dataToSave, string saveSlotID)
	{
		if (saveSlotID == null)
			return;

		string fullPath = Path.Combine(directory, saveSlotID, subFolders, fileName);
		string parentDirectory = Path.GetDirectoryName(fullPath);
		try
		{
			if (!Directory.Exists(parentDirectory))
				Directory.CreateDirectory(parentDirectory);

			// Serialize the object's data into a json formatted string.
			string serializedData = JsonConvert.SerializeObject(dataToSave, Formatting.Indented);

			// Encrypt the data (Optional).
			if (useEncryption)
				serializedData = EncryptOrDecrypt(serializedData);

			// Write the serialized data to file.
			using (FileStream file = new FileStream(fullPath, FileMode.Create))
			{
				using StreamWriter writer = new StreamWriter(file);
				{
					writer.Write(serializedData);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"Error occured when trying to save data to file.\n" +
						   $"At full path: {fullPath}.\n" +
						   $"Reason: {ex.Message}.");
		}
	}

	/// <summary>
	/// Save the data of type TData using Unity's JsonUtility.
	/// </summary>
	/// <param name="dataToSave"></param>
	public void SaveDataToFile(TData dataToSave)
	{
		string fullPath = Path.Combine(directory, subFolders, fileName);
		string parentDirectory = Path.GetDirectoryName(fullPath);

		try
		{
			if (!Directory.Exists(parentDirectory))
				Directory.CreateDirectory(parentDirectory);

			// Serialize data.
			string json = JsonUtility.ToJson(dataToSave, true);

			// Encrypt the data (Optional).
			if (useEncryption)
				json = EncryptOrDecrypt(json);

			// Write the serialized data to file.
			using (FileStream file = new FileStream(fullPath, FileMode.Create))
			{
				using StreamWriter writer = new StreamWriter(file);
				{
					writer.Write(json);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"Error occured when trying to save json to file.\n" +
						   $"At full path: {fullPath}.\n" +
						   $"Reason: {ex.Message}.");
		}
	}

	private string EncryptOrDecrypt(string data)
	{
		string modifiedData = "";
		int codeLength = ENCRYPTION_CODE.Length;

		for (int i = 0; i < data.Length; i++)
		{
			// This will cycle back and forth the encryption code.
			modifiedData += (char) (data[i] ^ ENCRYPTION_CODE[i % codeLength]);
		}

		return modifiedData;
	}
}
