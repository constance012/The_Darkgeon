using TMPro;
using UnityEngine;

public class ItemPickup : Interactable
{
	[Header("Current Item Info")]
	[Space]

	public Item itemPrefab;
	public GameObject pickedItemUIPrefab;

	private Item currentItem;
	private SpriteRenderer spriteRenderer;

	private void Start()
	{
		currentItem = Instantiate(itemPrefab);

		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = currentItem.icon;
		
		mat = spriteRenderer.material;
	}

	public override void Interact()
	{
		base.Interact();

		Pickup();
	}

	protected override void CreatePopupLabel()
	{
		bool isCloneExisting = worldCanvas.transform.Find("Popup Label");

		int quantity = currentItem.quantity;
		TextMeshProUGUI labelText;

		// Create a clone if not already exists.
		if (!isCloneExisting)
		{
			base.CreatePopupLabel();
			labelText = clone.transform.Find("Object Name").GetComponent<TextMeshProUGUI>();

			labelText.text = quantity > 1 ? currentItem.itemName.ToUpper() + " x" + currentItem.quantity
										: currentItem.itemName.ToUpper();
		}
		// Otherwise, use the existing one.
		else
		{
			clone = worldCanvas.transform.Find("Popup Label").gameObject;
			labelText = clone.transform.Find("Object Name").GetComponent<TextMeshProUGUI>();

			labelText.text += quantity > 1 ? "\n" + currentItem.itemName.ToUpper() + " x" + currentItem.quantity
										: "\n" + currentItem.itemName.ToUpper();
		}
	}

	private void Pickup()
	{
		currentItem.name = itemPrefab.name;
		Debug.Log("You're picking up a(an) " + currentItem.name);

		// Destroy the popup label.
		Destroy(clone);

		if (Inventory.instance.Add(currentItem))
		{
			NewItemUI.Generate(pickedItemUIPrefab, itemPrefab);
			Destroy(gameObject);
		}
	}
}
