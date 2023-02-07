using UnityEngine;

public class ItemPickup : Interactable
{
	public Item itemPrefab;

	private Item currentItem;
	private SpriteRenderer spriteRenderer;

	private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		currentItem = Instantiate(itemPrefab);
		spriteRenderer.sprite = currentItem.icon;
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
