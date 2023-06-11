using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// A serializable dictionary, can be used to stored complex data on files.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[Serializable]
public class BetterDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField] private List<TKey> keys = new List<TKey>();
	[SerializeField] private List<TValue> values = new List<TValue>();

	// Save the dictionary to lists.
	public void OnBeforeSerialize()
	{
		keys.Clear();
		values.Clear();

		foreach(KeyValuePair<TKey, TValue> pair in this)
		{
			keys.Add(pair.Key);
			values.Add(pair.Value);
		}
	}

	// Load the dictionary from lists.
	public void OnAfterDeserialize()
	{
		this.Clear();

		if (keys.Count != values.Count)
			Debug.LogError($"FATAL ERROR: Failed to deserialize a BetterDictionary object." +
						   $"The amount of keys {keys.Count} does not match the amount of values {values.Count}." +
						   $"Please ensure all the keys have their corresponding values in the save file.");

		for(int i = 0; i < keys.Count; i++)
		{
			this.Add(keys[i], values[i]);
		}
	}
}