using UnityEngine;

public class ItemPickup : Interactable
{
	public Item itemPrefab;

	private Item currentItem;
	private SpriteRenderer spriteRenderer;

	private void Start()
	{
		currentItem = Instantiate(itemPrefab);

		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = currentItem.icon;
		
		mat = spriteRenderer.material;
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
