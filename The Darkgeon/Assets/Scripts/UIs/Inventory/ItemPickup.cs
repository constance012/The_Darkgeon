using CSTGames.CommonEnums;
using TMPro;
using UnityEngine;

public class ItemPickup : Interactable
{
	[Header("Current Item Info")]
	[Space]

	[Tooltip("The scriptable object represents this item.")] public Item itemSO;
	public GameObject pickedItemUIPrefab;

	private Item currentItem;

	private void Start()
	{
		currentItem = Instantiate(itemSO);
		currentItem.name = itemSO.name;

		spriteRenderer.sprite = currentItem.icon;		
	}

	protected override void CheckForInteraction(float mouseDistance, float playerDistance)
	{
		if (mouseDistance <= radius || playerDistance <= interactDistance)
		{
			TriggerInteraction(playerDistance);
		}
		else
		{
			CancelInteraction(playerDistance);
		}

	}

	protected override void TriggerInteraction(float playerDistance)
	{
		base.TriggerInteraction(playerDistance);

		if (InputManager.instance.GetKeyDown(KeybindingActions.Interact) && playerDistance <= interactDistance)
			Interact();
	}

	public override void Interact()
	{
		base.Interact();

		Pickup();
	}

	public override void ExecuteRemoteLogic(bool state)
	{
		
	}

	protected override void CreatePopupLabel()
	{
		Transform foundLabel = worldCanvas.transform.Find("Popup Label");

		string itemName = currentItem.itemName;
		int quantity = currentItem.quantity;
		Color textColor = currentItem.rarity.color;

		// Create a clone if not already exists.
		if (foundLabel == null)
		{
			base.CreatePopupLabel();
			clone.SetObjectName(itemName, quantity, textColor);
		}

		// Otherwise, append to the existing one.
		else
		{
			clone = foundLabel.GetComponent<InteractionPopupLabel>();

			clone.SetObjectName(itemName, quantity, textColor, true);
			clone.RestartAnimation();
		}
	}

	private void Pickup()
	{
		Debug.Log("You're picking up a(n) " + currentItem.itemName);

		// Destroy the popup label.
		Destroy(clone.gameObject);

		if (Inventory.instance.Add(currentItem))
		{
			NewItemNotifier.Generate(pickedItemUIPrefab, itemSO);
			Destroy(gameObject);
		}
	}
}
