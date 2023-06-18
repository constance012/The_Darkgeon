using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemPickup : Interactable
{
	[Header("Current Item Info")]
	[Space]

	public Item itemPrefab;
	public GameObject pickedItemUIPrefab;

	private Item currentItem;
	private SpriteRenderer spriteRenderer;

	protected override void Awake()
	{
		base.Awake();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Start()
	{
		currentItem = Instantiate(itemPrefab);
		currentItem.name = itemPrefab.name;

		spriteRenderer.sprite = currentItem.icon;
		
		mat = spriteRenderer.material;
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
		bool isCloneExisting = worldCanvas.transform.Find("Popup Label");

		int quantity = currentItem.quantity;
		TextMeshProUGUI labelText;

		// Create a clone if not already exists.
		if (!isCloneExisting)
		{
			base.CreatePopupLabel();
			labelText = clone.transform.Find("Names/Object Name").GetComponent<TextMeshProUGUI>();

			labelText.text = quantity > 1 ? currentItem.itemName.ToUpper() + " x" + currentItem.quantity
										: currentItem.itemName.ToUpper();
			labelText.color = currentItem.rarity.color;
		}
		// Otherwise, use the existing one.
		else
		{
			clone = worldCanvas.transform.Find("Popup Label").gameObject;
			labelText = clone.transform.Find("Names/Object Name").GetComponent<TextMeshProUGUI>();
			TextMeshProUGUI duplicatedLabel = Instantiate(labelText, labelText.transform.parent);

			duplicatedLabel.text = quantity > 1 ? currentItem.itemName.ToUpper() + " x" + currentItem.quantity
										: currentItem.itemName.ToUpper();
			duplicatedLabel.color = currentItem.rarity.color;

			clone.GetComponent<Animator>().SetTrigger("Restart");
		}
	}

	private void Pickup()
	{
		Debug.Log("You're picking up a(an) " + currentItem.itemName);

		// Destroy the popup label.
		Destroy(clone);

		if (Inventory.instance.Add(currentItem))
		{
			NewItemNotifier.Generate(pickedItemUIPrefab, itemPrefab);
			Destroy(gameObject);
		}
	}
}
