using UnityEngine;
using CSTGames.CommonEnums;
using TMPro;
using System.Collections;

[RequireComponent (typeof(SpriteRenderer))]
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
	private SpriteRenderer spriteRenderer;
	
	private TextMeshProUGUI doorText;
	private Animator levelClearedText;

	private ParticleSystem torches;
	private Flickering[] pointLights;

	private bool levelCleared;

	protected override void Awake()
	{
		base.Awake();
		enemiesContainer = GameObject.FindWithTag("Enemies Container").transform;
		
		spriteRenderer = GetComponent<SpriteRenderer>();
		mat = spriteRenderer.material;

		doorText = worldCanvas.transform.Find("Door Text").GetComponent<TextMeshProUGUI>();

		if (direction == DoorDirection.ToNextLevel)
		{
			levelClearedText = GameObject.FindWithTag("Level UI Canvas").transform.Find("Level Cleared Text").GetComponent<Animator>();
			torches = transform.Find("Torches").GetComponent<ParticleSystem>();
			pointLights = GetComponentsInChildren<Flickering>(true);
		}
	}

	protected override void Update()
	{
		float distance = Vector2.Distance(player.position, transform.position);

		if (distance <= radius)
		{
			mat.SetFloat("_Thickness", .002f);

			if (!hasInteracted && InputManager.instance.GetKeyDown(KeybindingActions.Interact))
				Interact();
		}

		else if (distance > radius)
		{
			mat.SetFloat("_Thickness", 0f);
			hasInteracted = false;
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

	private void OnTriggerEnter2D(Collider2D collision)
	{
		doorText.text = doorDescription;
		doorText.GetComponent<Animator>().Play("Increase Alpha");
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		// Keep the text above the door.
		doorText.transform.position = this.transform.position.AddOrSubstractComponent('y', .9f);
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		doorText.GetComponent<Animator>().Play("Decrease Alpha");
	}

	public override void Interact()
	{
		base.Interact();

		if (isOpened)
		{
			switch (direction)
			{
				case DoorDirection.ToNextLevel:
					if (levelCleared)
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
		else
			doorText.text = "LOCKED";
	}

	public override void ExecuteRemoteLogic(bool state)
	{
		isOpened = state;

		spriteRenderer.sprite = isOpened ? openSprite : closeSprite;
	}

	private IEnumerator DisplayClearedText()
	{
		levelClearedText.Play("Increase Alpha");

		yield return new WaitForSecondsRealtime(2f);

		levelClearedText.Play("Decrease Alpha");
	}
}
