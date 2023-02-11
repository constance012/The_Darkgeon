using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{
	[Header("Items List")]
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

		// Use this to check if the chest is opened or not. True if the chest is closed.
		hasInteracted = true;
	}

	// Use this Update instead of the parent's one.
	private new void Update()
	{
		float distance = Vector2.Distance(player.position, transform.position);

		if (distance <= radius)
		{
			mat.SetFloat("_Thickness", .002f);

			if (InputManager.instance.GetKeyDown(KeybindingActions.Interact))
				Interact();
		}

		else if (distance > radius)
		{
			mat.SetFloat("_Thickness", 0f);

			// Close the chest when out of range.
			if (!hasInteracted)
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

	private void OpenAndClose()
	{
		if (!hasInteracted)
		{
			animator.SetTrigger("Open");
			
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
}
