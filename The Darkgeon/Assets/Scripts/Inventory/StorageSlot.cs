using System;
using System.Collections.Generic;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class StorageSlot : MonoBehaviour
{
	[Header("Current Item")]
	[Space]
	public Item currentItem;

	// Protected fields.
	protected Image icon;
	protected TextMeshProUGUI quantity;

	protected TooltipTrigger tooltip;

	protected virtual void Awake()
	{
		icon = transform.GetComponentInChildren<Image>("Item Button/Icon");

		quantity = transform.GetComponentInChildren<TextMeshProUGUI>("Item Button/Quantity");
		tooltip = GetComponent<TooltipTrigger>();
	}

	public virtual void AddItem(Item newItem)
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

		tooltip.header = currentItem.itemName;
		tooltip.content = currentItem.ToString();
		tooltip.popupDelay = .5f;
	}

	public virtual void ClearItem()
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
		if (currentItem != null && currentItem.canBeUsed)
			currentItem.Use();
	}

	public abstract void OnDrop(GameObject shipper);

	protected void SwapSlotIndexes<TSlot>(ClickableObject cloneData) where TSlot : StorageSlot
	{
		Item droppedItem = cloneData.dragItem;
		ItemStorage currentStorage = cloneData.currentStorage;
		ItemStorage otherStorage = cloneData.otherStorage;

		int senderIndex = droppedItem.slotIndex;
		int destinationIndex = currentItem.slotIndex;

		// If swap within the current storage.
		if (cloneData.FromSameStorageSlot<TSlot>())
		{
			if (!currentStorage.IsExisting(droppedItem.id))
				currentStorage.Add(droppedItem, true);

			else if (ClickableObject.splittingItem)
				currentStorage.UpdateQuantity(droppedItem.id, droppedItem.quantity);

			// Swap their slot indexes.
			currentStorage.UpdateSlotIndex(currentItem.id, senderIndex);
			currentStorage.UpdateSlotIndex(droppedItem.id, destinationIndex);
		}

		// If swap between two storages.
		else
		{
			if (!otherStorage.IsExisting(droppedItem.id))
				otherStorage.Add(droppedItem, true);

			else if (ClickableObject.splittingItem)
			{
				otherStorage.UpdateQuantity(droppedItem.id, droppedItem.quantity);
				droppedItem.quantity = otherStorage.GetItem(droppedItem.id).quantity;
			}

			Item destinationItem = Instantiate(currentItem);
			destinationItem.name = currentItem.name;

			otherStorage.Remove(droppedItem.id);
			currentStorage.Remove(destinationItem.id);

			destinationItem.slotIndex = senderIndex;
			droppedItem.slotIndex = destinationIndex;

			otherStorage.Add(destinationItem, true);
			currentStorage.Add(droppedItem, true);
		}
	}
}
