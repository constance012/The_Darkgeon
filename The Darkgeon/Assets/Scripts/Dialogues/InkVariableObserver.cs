using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using System;

/// <summary>
/// A listener class responsibles for maintaining variables state between Ink stories.
/// </summary>
public class InkVariableObserver
{
	private Dictionary<string, Ink.Runtime.Object> _variables;
	
	private Story _globalVarManager;
	private SaveFileHandler<string> _saveHandler;

	public InkVariableObserver(TextAsset varManagerJson)
	{
		_globalVarManager = new Story(varManagerJson.text);
		_saveHandler = new SaveFileHandler<string>(Application.persistentDataPath, "Global Variables", "globals.cst", false);

		// Load the saved state to the manager.
		LoadStoryState();

		// Populate the dictionary by reading the variables in the Ink manager file.
		_variables = new Dictionary<string, Ink.Runtime.Object>();

		foreach(string name in _globalVarManager.variablesState)
		{
			Ink.Runtime.Object value = _globalVarManager.variablesState.GetVariableWithName(name);
			
			_variables.Add(name, value);

			Debug.Log($"Initialized variable: {name} = {value}");
		}
	}

	public void SetVariable(string name, Ink.Runtime.Object value)
	{
		if (_variables.ContainsKey(name))
		{
			_variables[name] = value;
		}
	}

	public Ink.Runtime.Object GetVariable(string name)
	{
		_variables.TryGetValue(name, out Ink.Runtime.Object value);

		if (value == null)
			Debug.LogWarning($"Variable {name} does not exist. Returning null");

		return value;
	}
	
	public void Register(Story story)
	{
		SetVariableInStory(story);
		story.variablesState.variableChangedEvent += OnVariableChanged;
	}

	public void Unregister(Story story)
	{
		story.variablesState.variableChangedEvent -= OnVariableChanged;
	}

	public void SaveStoryState()
	{
		if (_globalVarManager != null)
		{
			SetVariableInStory(_globalVarManager);

			string json = _globalVarManager.state.ToJson();

			_saveHandler.SaveDataToFile(json, GameDataManager.instance.SelectedSaveSlotID);
		}
	}

	public void LoadStoryState()
	{
		if (_globalVarManager != null)
		{
			string json = _saveHandler.LoadDataFromFile(GameDataManager.instance.SelectedSaveSlotID);

			if (json != null && !json.Equals(""))
				_globalVarManager.state.LoadJson(json);
		}
	}

	private void OnVariableChanged(string name, Ink.Runtime.Object value)
	{
		Debug.Log($"Variable {name} changed to {value}");

		// Maintain the variables in the dictionary.
		if (_variables.ContainsKey(name))
		{
			_variables.Remove(name);
			_variables.Add(name, value);
		}
	}

	private void SetVariableInStory(Story story)
	{
		foreach(KeyValuePair<string, Ink.Runtime.Object> variable in _variables)
		{
			story.variablesState.SetGlobal(variable.Key, variable.Value);
		}
	}
}
