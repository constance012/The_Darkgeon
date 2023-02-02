using UnityEngine;

public class ItemPickup : Interactable
{
	[SerializeField] private Item itemPrefab;
	private Item currentItem;

	private void Start()
	{
		currentItem = Instantiate(itemPrefab);
	}

	protected override void Interact()
	{
		base.Interact();

		Pickup();
	}

	private void Pickup()
	{
		currentItem.name = itemPrefab.name;
		Debug.Log("You're picking up a(an) " + currentItem.name);

		if (Inventory.instance.Add(currentItem))
			Destroy(gameObject);
	}
}
