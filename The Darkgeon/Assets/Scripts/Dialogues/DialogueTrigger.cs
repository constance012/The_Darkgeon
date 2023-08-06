using System;
using UnityEngine;
using CSTGames.DataPersistence;
using UnityEditor.SceneManagement;

[RequireComponent(typeof(Interactable))]
public class DialogueTrigger : MonoBehaviour, ISaveDataTransceiver
{
	[Header("General Info")]
	[SerializeField, ReadOnly] private string ID;
	
	[ContextMenu("Generate Trigger ID")]
	private void GenerateTriggerID()
	{
		ID = Guid.NewGuid().ToString();
		EditorSceneManager.MarkSceneDirty(gameObject.scene);
	}
	
	[ContextMenu("Clear Trigger ID")]
	private void ClearTriggerID()
	{
		ID = "";
		EditorSceneManager.MarkSceneDirty(gameObject.scene);
	}

	[Header("Dialogue JSON file")]
	[Space]
	[SerializeField] private TextAsset inkJson;

	[Header("Configurations")]
	[Space]
	[Tooltip("Should this dialogue be triggered one time only?")]
	public bool triggerOneTime;

	private Interactable _interactable;
	private bool _wasTriggered;

	private void Awake()
	{
		_interactable = GetComponent<Interactable>();
	}

	private void Start()
	{
		if (triggerOneTime && _wasTriggered)
		{
			SelfDestruct();
		}
	}

	public void TriggerDialogue()
	{
		if (inkJson == null)
		{
			Debug.LogWarning("The Ink Json file is not found, please assign it in the Inspector before triggering the Dialogue.");
			return;
		}

		if (!DialogueManager.DialogueIsPlaying)
		{
			DialogueManager.instance.TriggerDialogue(inkJson, _interactable.InkExternalFunction);

			if (triggerOneTime)
			{
				_wasTriggered = true;
				SelfDestruct();
			}
		}
	}

	#region Save and Load Data.
	public void LoadData(GameData gameData)
	{
		gameData.levelData.dialogueTriggersData.TryGetValue(ID, out _wasTriggered);
	}

	public void SaveData(GameData gameData)
	{
		if (ID == null || ID.Equals(""))
		{
			Debug.LogWarning("This Dialogue Trigger doesn't have an ID yet, its data will not be stored.");
			return;
		}

		LevelData levelData = gameData.levelData;

		if (levelData.dialogueTriggersData.ContainsKey(ID))
		{
			levelData.dialogueTriggersData.Remove(ID);
		}

		levelData.dialogueTriggersData.Add(ID, _wasTriggered);
	}
	#endregion

	private void SelfDestruct()
	{
		_interactable.dialogueTrigger = null;
		_interactable.oneTimeDialogueTriggered = true;

		Destroy(this);
	}
}
