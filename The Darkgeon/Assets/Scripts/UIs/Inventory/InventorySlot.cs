using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour, IDropHandler
{
	[Header("Current Item")]
	[Space]

	public Item currentItem;

	// Private fields.
	private Image icon;
	private Image favoriteBorder;
	private TextMeshProUGUI quantity;

	private TooltipTrigger tooltip;

	private void Awake()
	{
		favoriteBorder = transform.Find("Item Button/Favorite Border").GetComponent<Image>();
		icon = transform.Find("Item Button/Icon").GetComponent<Image>();
		
		quantity = transform.Find("Item Button/Quantity").GetComponent<TextMeshProUGUI>();
		tooltip = GetComponent<TooltipTrigger>();
	}

	public void AddItem(Item newItem)
	{
		currentItem = newItem;

		favoriteBorder.enabled = currentItem.isFavorite;

		icon.sprite = currentItem.icon;
		icon.enabled = true;

		if (currentItem.stackable)
		{
			quantity.text = currentItem.quantity.ToString();
			quantity.enabled = true;
		}
		else
		{
			quantity.text = "1";
			quantity.enabled = false;
		}

		tooltip.header = currentItem.name;
		tooltip.content = currentItem.description;
		tooltip.popupDelay = .5f;
	}

	public void ClearItem()
	{
		currentItem = null;

		favoriteBorder.enabled = false;

		icon.sprite = null;
		icon.enabled = false;

		quantity.text = "";
		quantity.enabled = false;

		tooltip.header = "";
		tooltip.content = "";
		tooltip.popupDelay = 0f;
	}

	public void UseItem()
	{
		// Use the item if it's not null and be used.
		if (currentItem != null && currentItem.canbeUsed)
		{
			currentItem.Use();
			Inventory.instance.UpdateQuantity(currentItem.id, -1);
			tooltip.HideTooltip();
		}
	}

	/// <summary>
	/// This method used to catch the dragged item from another slot.
	/// </summary>
	/// <param name="eventData"></param>
	public void OnDrop(PointerEventData eventData)
	{
		if (eventData.pointerDrag.GetComponent<DraggableItem>() != null)
		{
			GameObject droppedObj = eventData.pointerDrag.GetComponent<DraggableItem>().clone;
			Item droppedItem = droppedObj.GetComponent<DraggableItem>().dragItem;

			// Local function.
			void SwapSlotIndexes()
			{
				int dropOnIndex = currentItem.slotIndex;
				int draggedFromIndex = droppedItem.slotIndex;

				// Swap their slot indexes.
				Inventory.instance.UpdateSlotIndex(currentItem.id, draggedFromIndex);
				Inventory.instance.UpdateSlotIndex(droppedItem.id, dropOnIndex);

				//Debug.Log("Destination item index " + currentItem.slotIndex);
				Debug.Log("Dragged item index " + droppedItem.slotIndex);
			}

			// If this is a empty slot, then move the item to this slot.
			if (currentItem == null)
			{
				Inventory.instance.UpdateSlotIndex(droppedItem.id, transform.GetSiblingIndex());
				return;
			}

			// If there's an item of the same type, check if it can be stacked together.
			else if (currentItem.name.Equals(droppedItem.name))
			{
				if (currentItem.stackable && currentItem.quantity < currentItem.maxPerStack)
				{
					int totalQuantity = currentItem.quantity + droppedItem.quantity;

					if (totalQuantity > currentItem.maxPerStack)
					{
						int residue = totalQuantity - currentItem.maxPerStack;

						Inventory.instance.UpdateQuantity(currentItem.id, currentItem.maxPerStack, true);
						Inventory.instance.UpdateQuantity(droppedItem.id, residue, true);

						return;
					}

					else if (totalQuantity == currentItem.maxPerStack)
						Inventory.instance.UpdateQuantity(currentItem.id, totalQuantity, true);

					// Otherwise, just increase the quantity of the current one.
					else
						Inventory.instance.UpdateQuantity(currentItem.id, droppedItem.quantity);
					
					// Set as favorite when needed.
					if (!currentItem.isFavorite && droppedItem.isFavorite)
						Inventory.instance.SetFavorite(currentItem.id, true);
					
					// Finally, destroy the dragged one if there's no residue was created.
					Inventory.instance.Remove(droppedItem, true);
				}

				// If it can not be stacked or its stack is currently full, swap the slot indexes between them.
				else if (!currentItem.stackable || currentItem.quantity == currentItem.maxPerStack)
					SwapSlotIndexes();

				return;
			}

			// If they're two different items.
			SwapSlotIndexes();
		}
	}
}