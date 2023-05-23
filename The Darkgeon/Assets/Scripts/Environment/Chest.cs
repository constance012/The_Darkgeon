using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CSTGames.CommonEnums;

public class Chest : Interactable
{
	/// <summary>
	/// Represent every type of chest.
	/// </summary>
	public enum ChestType { Wooden, Iron, Silver, Golden }

	[Space]
	[Header("General Info")]
	public ChestType type;
	public float distanceBeforeClosed;

	[Space]
	[SerializeField] private static Dictionary<ItemCategory, int> treasureAmount = new Dictionary<ItemCategory, int>
	{
		[ItemCategory.Null] = 0,
		[ItemCategory.Coin] = 1,
		[ItemCategory.Equipment] = 2,
		[ItemCategory.Weapon] = 1,
		[ItemCategory.Food] = 3,
		[ItemCategory.Potion] = 2,
		[ItemCategory.Material] = 3,
		[ItemCategory.Mineral] = 3
	};

	[SerializeField] private List<DeathLoot> treasures = new List<DeathLoot>();

	[Space]
	// Special items list for each chest.
	public List<Item> storedItem = new List<Item>();

	[Header("References")]
	[Space]
	[SerializeField] private Animator animator;

	// Private fields.

	private void Start()
	{
		animator = GetComponent<Animator>();
		mat = GetComponentInChildren<SpriteRenderer>().material;

		InitializeTreasures();
		// Use this to check if the chest is opened or not. True if the chest is closed.
		hasInteracted = true;
	}

	protected override void Update()
	{
		Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		float interactDistance = Vector2.Distance(worldMousePos, transform.position);
		float forcedCloseDistance = Vector2.Distance(player.position, transform.position);

		if (interactDistance <= radius)
		{
			if (clone == null)
				CreatePopupLabel();
			else
				clone.transform.position = transform.position;

			mat.SetFloat("_Thickness", .002f);

			if (Input.GetMouseButtonDown(1))
			{
				Chest target = ChestStorage.instance.openedChest;

				// Close any currently opening chest before opening the other one.
				if (target != null && target != this)
					target.Interact();
				
				Interact();
			}
		}

		else if (interactDistance > radius)
		{
			Destroy(clone);

			mat.SetFloat("_Thickness", 0f);

			// Close the chest when out of range.
			if (!hasInteracted && forcedCloseDistance > distanceBeforeClosed)
			{
				hasInteracted = true;
				OpenAndClose();
			}
		}
	}

	public override void Interact()
	{
		base.Interact();

		hasInteracted = !hasInteracted;

		OpenAndClose();
	}

	protected override void CreatePopupLabel()
	{
		base.CreatePopupLabel();

		clone.transform.Find("Names/Object Name").GetComponent<TextMeshProUGUI>().text = type.ToString().ToUpper() + " CHEST";
	}

	private void OpenAndClose()
	{
		if (!hasInteracted)
		{
			animator.SetTrigger("Open");
			
			// Activate the Inventory canvas if it hasn't already open yet.
			if (!Inventory.instance.transform.parent.gameObject.activeInHierarchy)
				Inventory.instance.transform.parent.gameObject.SetActive(true);

			ChestStorage.instance.openedChest = this;
			ChestStorage.instance.gameObject.SetActive(true);

		}
		else
		{
			animator.SetTrigger("Close");

			ChestStorage.instance.openedChest = null;
			ChestStorage.instance.gameObject.SetActive(false);
		}
	}

	private void InitializeTreasures()
	{
		if (treasures.Count == 0)
			return;

		// Essential local variables.
		ItemCategory currentType = ItemCategory.Null;
		List<DeathLoot> itemsOfCurrentType = new List<DeathLoot>();

		int numberOfItemsRemaining = 0;

		// Explicit writing of int.CompareTo(int value) method.
		treasures.Sort((x, y) =>
		{
			if ((int)x.loot.category < (int)y.loot.category) return -1;
			else if ((int)x.loot.category > (int)y.loot.category) return 1;
			else return 0;
		});

		foreach (DeathLoot target in treasures)
		{
			if (target.isGuaranteed)
			{
				Item guaranteedItem = Instantiate(target.loot);
				guaranteedItem.quantity = target.quantity;
				guaranteedItem.id = Guid.NewGuid().ToString();

				storedItem.Add(guaranteedItem);
				continue;
			}

			// If the target treasure type is changed, than update these locals.
			else if (target.loot.category != currentType)
			{
				currentType = target.loot.category;
				itemsOfCurrentType = treasures.FindAll(loot => loot.loot.category == currentType);

				numberOfItemsRemaining = treasureAmount[currentType];
			}

			// Skip this iteration if items of the current type are fully added.
			if (numberOfItemsRemaining <= 0 || itemsOfCurrentType.Count == 0)
				continue;

			int randomIndex = UnityEngine.Random.Range(0, itemsOfCurrentType.Count);

			DeathLoot selectedLoot = itemsOfCurrentType[randomIndex];

			Item otherItem = Instantiate(selectedLoot.loot);
			otherItem.quantity = selectedLoot.quantity;
			otherItem.id = Guid.NewGuid().ToString();

			storedItem.Add(otherItem);
			itemsOfCurrentType.Remove(itemsOfCurrentType[randomIndex]);

			numberOfItemsRemaining--;
		}

		treasures.Clear();
	}
}