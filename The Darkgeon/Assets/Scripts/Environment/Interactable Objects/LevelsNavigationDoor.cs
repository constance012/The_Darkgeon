using UnityEngine;
using CSTGames.CommonEnums;
using TMPro;
using System.Collections;
using Ink.Runtime;

public class LevelsNavigationDoor : Interactable
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
	private Transform enemiesContainer;
	
	private Animator levelClearedText;

	private ParticleSystem torches;
	private Flickering[] pointLights;

	private bool levelCleared;

	protected override void Awake()
	{
		base.Awake();
		enemiesContainer = GameObject.FindWithTag("Enemies Container").transform;

		if (direction == DoorDirection.ToNextLevel)
		{
			levelClearedText = GameObject.FindWithTag("Level UI Canvas").transform.GetComponentInChildren<Animator>("Level Cleared Text");
			torches = transform.GetComponentInChildren<ParticleSystem>("Torches");
			pointLights = GetComponentsInChildren<Flickering>(true);
		}
	}

	private void LateUpdate()
	{
		if (levelCleared || direction == DoorDirection.ToPreviousLevel || direction == DoorDirection.ToMainMenu)
			return;

		bool isCleared = enemiesContainer.childCount == 0;

		if (levelCleared != isCleared)
		{
			levelCleared = isCleared;

			StartCoroutine(DisplayClearedText());

			torches.Play();
			
			foreach (var pointLight in pointLights)
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

		//if (isOpened)
		//{
		//	switch (direction)
		//	{
		//		case DoorDirection.ToNextLevel:
		//			if (levelCleared)
		//				LevelsManager.instance.LoadNextLevel();
		//			break;

		//		case DoorDirection.ToPreviousLevel:
		//			LevelsManager.instance.LoadPreviousLevel();
		//			break;

		//		case DoorDirection.ToMainMenu:
		//			GameManager.instance.ReturnToMenu();
		//			break;
		//	}

		//	hasInteracted = true;
		//}
	}

	public override void ExecuteRemoteLogic(bool state)
	{
		isOpened = state;

		DialogueManager.instance.SetVariable("is_door_opened", state);

		spriteRenderer.sprite = isOpened ? openSprite : closeSprite;
	}

	private IEnumerator DisplayClearedText()
	{
		levelClearedText.Play("Increase Alpha");

		yield return new WaitForSecondsRealtime(2f);

		levelClearedText.Play("Decrease Alpha");
	}
}
