using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
	[Header("Current Item")]
	[Space]

	public Item currentItem;
	
	private Image icon;
	private TextMeshProUGUI quantity;
	private TooltipTrigger tooltip;

	private void Awake()
	{
		icon = transform.Find("Item Button/Icon").GetComponent<Image>();
		quantity = transform.Find("Item Button/Quantity").GetComponent<TextMeshProUGUI>();
		tooltip = GetComponent<TooltipTrigger>();
	}

	public void AddItem(Item newItem)
	{
		currentItem = newItem;

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
			Inventory.instance.UpdateQuantity(currentItem.name, -1);
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
			DraggableItem droppedItem = droppedObj.GetComponent<DraggableItem>();

			// Swap the slot index between them.
			if (currentItem == null)
			{
				Inventory.instance.UpdateSlotIndex(droppedItem.dragItem.name, transform.GetSiblingIndex());
			}

			else
			{
				// Store the slot indexes for reversing back when swapping the items.
				int dropOnIndex = currentItem.slotIndex;
				int draggedFromIndex = droppedItem.dragItem.slotIndex;

				// Swap their slot indexes.
				Inventory.instance.UpdateSlotIndex(currentItem.name, draggedFromIndex);
				Inventory.instance.UpdateSlotIndex(droppedItem.dragItem.name, dropOnIndex);

				//Debug.Log("Destination item index " + currentItem.slotIndex);
				Debug.Log("Dragged item index " + droppedItem.dragItem.slotIndex);
			}
		}
	}
}
