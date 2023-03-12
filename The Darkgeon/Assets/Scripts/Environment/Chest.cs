using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chest : Interactable
{
	[Space]
	[Header("General Info")]
	public ChestType type;
	public float distanceBeforeClosed;

	[Space]
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

	// Use this Update instead of the parent's one.
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
		TreasureType currentType = TreasureType.Null;
		List<DeathLoot> itemsOfCurrentType = new List<DeathLoot>();

		int numberOfItemsRemaining = 0;

		// Explicit writing of int.CompareTo(int value) method.
		treasures.Sort((x, y) =>
		{
			if ((int)x.type < (int)y.type) return -1;
			else if ((int)x.type > (int)y.type) return 1;
			else return 0;
		});

		foreach (DeathLoot target in treasures)
		{
			if (target.isGuaranteed || target.type == TreasureType.Primary)
			{
				Item primaryItem = Instantiate(target.loot);
				primaryItem.quantity = target.quantity;
				primaryItem.id = Guid.NewGuid().ToString();

				storedItem.Add(primaryItem);
				continue;
			}

			// If the target treasure type is changed, than update these locals.
			else if (target.type != currentType) 
			{
				currentType = target.type;
				itemsOfCurrentType = treasures.FindAll(loot => loot.type == currentType);

				numberOfItemsRemaining = (int)currentType;
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

/// <summary>
/// Represent every type of chest.
/// </summary>
public enum ChestType { Wooden, Iron, Silver, Golden }

/// <summary>
/// Represents types of treasure and the thier maximum quantity when generated in a chest.
/// </summary>
public enum TreasureType
{
	Null = 0,
	Primary = 1,
	Food = 2,
	Potion = 3,
	Material = 4
}