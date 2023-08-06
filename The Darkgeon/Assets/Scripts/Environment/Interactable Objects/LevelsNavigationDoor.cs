using UnityEngine;
using CSTGames.CommonEnums;
using TMPro;
using System.Collections;
using Ink.Runtime;
using CSTGames.DataPersistence;

public class LevelsNavigationDoor : Interactable, ISaveDataTransceiver
{
	public enum DoorDirection { ToNextLevel, ToPreviousLevel, ToMainMenu }

	[Header("Status")]
	[Space]
	[SerializeField, ReadOnly] private bool isOpened;

	[Header("Door Direction")]
	[Space]
	public DoorDirection direction;
	public string doorDescription;

	[Header("Sprites")]
	[Space]
	[SerializeField] private Sprite openSprite;
	[SerializeField] private Sprite closeSprite;

	// Private fields.
	private Transform _enemiesContainer;
	private TextMeshProUGUI _levelNameText;

	private ParticleSystem _torches;
	private Flickering[] _pointLights;
	private IEnumerator _displayTextRoutine;

	private bool _levelCleared;

	protected override void Awake()
	{
		base.Awake();
		_enemiesContainer = GameObject.FindWithTag("Enemies Container").transform;

		if (direction == DoorDirection.ToNextLevel)
		{
			_levelNameText = GameObject.FindWithTag("Level UI Canvas").transform.GetComponentInChildren<TextMeshProUGUI>("Level Name Text");
			_torches = transform.GetComponentInChildren<ParticleSystem>("Torches");
			_pointLights = GetComponentsInChildren<Flickering>(true);
		}
	}

	private void Start()
	{
		if (direction == DoorDirection.ToNextLevel)
		{
			_levelNameText.text = LevelsManager.instance.currentScene.name.ToUpper();

			_displayTextRoutine = DisplayLevelText();
			StartCoroutine(_displayTextRoutine);
		}
	}

	private void LateUpdate()
	{
		if (_levelCleared || direction != DoorDirection.ToNextLevel)
			return;

		bool isCleared = _enemiesContainer.childCount == 0;

		if (_levelCleared != isCleared)
		{
			_levelCleared = isCleared;
			_levelNameText.text = "LEVEL CLEARED!";

			StopCoroutine(_displayTextRoutine);
			StartCoroutine(_displayTextRoutine);

			_torches.Play();
			
			foreach (var pointLight in _pointLights)
				pointLight.gameObject.SetActive(true);
		}
	}

	protected override void TriggerInteraction(float playerDistance)
	{
		base.TriggerInteraction(playerDistance);

		if (Input.GetMouseButtonDown(1) && !hasInteracted && playerDistance <= interactDistance)
			Interact();
	}
	
	protected override void CreatePopupLabel()
	{
		base.CreatePopupLabel();

		clone.SetObjectName(doorDescription);
	}

	public override void Interact()
	{
		base.Interact();

		if (oneTimeDialogueTriggered && !DialogueManager.DialogueIsPlaying)
			Enter();
	}

	public override void InkExternalFunction()
	{
		base.InkExternalFunction();
		Enter();
	}

	public override void ExecuteRemoteLogic(bool state)
	{
		isOpened = state;

		DialogueManager.instance.SetVariable("is_door_opened", state);

		spriteRenderer.sprite = isOpened ? openSprite : closeSprite;
	}

	#region Save and Load Data.
	public void LoadData(GameData gameData)
	{
		gameData.levelData.navigationDoorsData.TryGetValue(ID, out _levelCleared);
	}

	public void SaveData(GameData gameData)
	{
		if (ID == null || ID.Equals("") || direction != DoorDirection.ToNextLevel)
		{
			Debug.LogWarning("This Navigation Door doesn't have an ID yet, its data will not be stored.\n" +
							 "Or this door is not leading to the next Level", this);
			return;
		}

		LevelData levelData = gameData.levelData;

		if (levelData.navigationDoorsData.ContainsKey(ID))
		{
			levelData.navigationDoorsData.Remove(ID);
		}

		levelData.navigationDoorsData.Add(ID, _levelCleared);
	}
	#endregion

	private void Enter()
	{
		if (isOpened)
		{
			switch (direction)
			{
				case DoorDirection.ToNextLevel:
					if (_levelCleared)
						LevelsManager.instance.LoadNextLevel();
					break;

				case DoorDirection.ToPreviousLevel:
					LevelsManager.instance.LoadPreviousLevel();
					break;

				case DoorDirection.ToMainMenu:
					GameManager.instance.ReturnToMenu();
					break;
			}

			hasInteracted = true;
		}
	}

	private IEnumerator DisplayLevelText()
	{
		Animator textAnim = _levelNameText.GetComponent<Animator>();

		textAnim.Play("Increase Alpha");

		yield return new WaitForSecondsRealtime(2f);

		textAnim.Play("Decrease Alpha");

		yield return new WaitForSecondsRealtime(.75f);

		_levelNameText.text = LevelsManager.instance.currentScene.name.ToUpper();
	}
}
