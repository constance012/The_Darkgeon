using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ClickableObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
	public Item dragItem;
	
	// Private fields.
	private InventorySlot currentSlot;
	private Image icon;

	private bool isLeftAltHeld;
	private bool isLeftShiftHeld;

	// Static fields.

	// A clone that "ships" the current item in this slot.
	public static GameObject clone { get; set; }
	// Singleton reference to the sender script.
	public static ClickableObject sender { get; private set; }
	public static bool holdingItem { get; set; }
	public static bool splittingItem { get; set; }

	private void Awake()
	{
		currentSlot = transform.GetComponentInParent<InventorySlot>();
		icon = transform.Find("Icon").GetComponent<Image>();
	}

	private void Update()
	{
		isLeftAltHeld = Input.GetKey(KeyCode.LeftAlt);
		isLeftShiftHeld = Input.GetKey(KeyCode.LeftShift);

		if (clone != null)
			clone.transform.position = Input.mousePosition;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			dragItem = currentSlot.currentItem;

			if (isLeftShiftHeld && dragItem != null)
			{
				SplitItem();
				return;
			}
			
			if (isLeftAltHeld && dragItem != null)
			{
				bool favorite = !dragItem.isFavorite;
				Inventory.instance.SetFavorite(dragItem.id, favorite);
				Debug.Log(dragItem + " is " + (favorite ? "favorite" : "not favorite"));
				return;
			}

			// Pick item up.
			if (!holdingItem && dragItem != null)
			{
				BeginDragItem();
				return;
			}

			// Drop item down.
			if (holdingItem)
			{
				// Drop if the other slot's item is null or has different id.
				if (dragItem == null || (dragItem != null && !sender.dragItem.id.Equals(dragItem.id)))
				{
					if (splittingItem)
						Inventory.instance.Add(clone.GetComponent<ClickableObject>().dragItem, true);

					currentSlot.OnDrop(clone);
				}

				// If the split item is being put back to the original slot.
				else if (splittingItem && dragItem != null && sender.dragItem.id.Equals(dragItem.id))
					currentSlot.OnDrop(clone);

				ClearSingleton();
				return;
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
			currentSlot.UseItem();
	}

	public static void ClearSingleton()
	{
		sender.icon.color = Color.white;

		sender.dragItem = null;

		sender = null;
		holdingItem = false;
		splittingItem = false;

		Destroy(clone);
	}

	private void BeginDragItem()
	{
		clone = Instantiate(gameObject, transform.root);
		clone.GetComponent<Image>().enabled = false;
		clone.transform.Find("Icon").GetComponent<Image>().raycastTarget = false;
		clone.transform.Find("Favorite Border").GetComponent<Image>().enabled = false;

		icon.color = new Color(.51f, .51f, .51f);

		holdingItem = true;
		sender = this;

		Debug.Log("You're dragging the " + dragItem.itemName);
	}

	private void SplitItem()
	{
		if (dragItem != null)
		{
			if (dragItem.quantity == 1)
			{
				BeginDragItem();
				return;
			}

			int halfQuantity = dragItem.quantity / 2;
			
			Item split = Instantiate(dragItem);
			split.quantity = halfQuantity;
			split.name = dragItem.name;

			clone = Instantiate(gameObject, transform.root);
			clone.GetComponent<ClickableObject>().dragItem = split;

			clone.GetComponent<Image>().enabled = false;
			clone.transform.Find("Icon").GetComponent<Image>().raycastTarget = false;
			clone.transform.Find("Favorite Border").GetComponent<Image>().enabled = false;
			clone.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = halfQuantity.ToString();

			holdingItem = true;
			splittingItem = true;
			sender = this;

			Inventory.instance.UpdateQuantity(dragItem.id, -halfQuantity);

			Debug.Log("Original item quantity: " + sender.dragItem.quantity);
		}
	}
}
