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

		foreach (DeathLoot target in treasures)
		{
			Item item = Instantiate(target.loot);
			item.quantity = target.quantity;
			item.id = Guid.NewGuid().ToString();

			if (target.isGuaranteed)
			{
				storedItem.Add(item);
				continue;
			}

			float rand = UnityEngine.Random.Range(0f, 100f);
			if (rand <= target.dropChance)
				storedItem.Add(item);
		}

		treasures.Clear();
	}
}

public enum ChestType { Wooden, Iron, Silver, Golden }
