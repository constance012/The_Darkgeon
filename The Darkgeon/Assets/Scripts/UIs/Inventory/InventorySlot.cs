using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
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

		tooltip.header = currentItem.itemName;
		tooltip.content = currentItem.ToString();
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
		}
	}

	/// <summary>
	/// This method used to catch the dragged item from another slot.
	/// </summary>
	/// <param name="eventData"></param>
	public void OnDrop(GameObject shipper)
	{
		if (shipper != null)
		{
			ClickableObject cloneData = shipper.GetComponent<ClickableObject>();
			Item droppedItem = cloneData.dragItem;

			if (droppedItem.itemName.Equals("Coin"))
			{
				Inventory.instance.Add(droppedItem, true);
				ChestStorage.instance.Remove(droppedItem);
				return;
			}

			// Local function.
			void SwapSlotIndexes()
			{
				int senderIndex = droppedItem.slotIndex;
				int destinationIndex = currentItem.slotIndex;

				// If swap within the inventory.
				if (!cloneData.isChestSlot)
				{
					//Debug.Log("Within the inventoy.");
					//Debug.Log("Sender index: " + senderIndex + ", Destination index: " + destinationIndex);
					//Debug.Log("Sender id: " + droppedItem.id + ", Destination id: " + currentItem.id);

					if (!Inventory.instance.IsExisting(droppedItem.id))
						Inventory.instance.Add(droppedItem, true);

					else if (ClickableObject.splittingItem)
						Inventory.instance.UpdateQuantity(droppedItem.id, droppedItem.quantity);

					Inventory.instance.UpdateSlotIndex(currentItem.id, senderIndex);
					Inventory.instance.UpdateSlotIndex(droppedItem.id, destinationIndex);
				}

				// If swap between the inventory and other storages.
				else
				{
					if (!ChestStorage.instance.IsExisting(droppedItem.id))
						ChestStorage.instance.Add(droppedItem, true);

					else if (ClickableObject.splittingItem)
					{
						ChestStorage.instance.UpdateQuantity(droppedItem.id, droppedItem.quantity);
						droppedItem.quantity = ChestStorage.instance.GetItem(droppedItem.id).quantity;
					}
					
					Item destinationItem = Instantiate(currentItem);
					destinationItem.name = currentItem.name;

					ChestStorage.instance.Remove(droppedItem.id);
					Inventory.instance.Remove(destinationItem.id);

					destinationItem.slotIndex = senderIndex;
					droppedItem.slotIndex = destinationIndex;

					ChestStorage.instance.Add(destinationItem, true);
					Inventory.instance.Add(droppedItem, true);
				}

				Debug.Log("Dragged item index " + droppedItem.slotIndex);
			}

			// If this is an empty slot.
			if (currentItem == null)
			{
				// Add the item if it doesn't already exist, otherwise move the item to this slot.
				if (!Inventory.instance.IsExisting(droppedItem.id) || ClickableObject.splittingItem)
					Inventory.instance.Add(droppedItem, true);

				Inventory.instance.UpdateSlotIndex(droppedItem.id, transform.GetSiblingIndex());
				
				if (cloneData.isChestSlot && !ClickableObject.splittingItem)
					ChestStorage.instance.Remove(droppedItem);
				
				return;
			}

			// If this is the original item that we had dragged.
			else if (currentItem.id.Equals(droppedItem.id) && !ClickableObject.splittingItem)
				return;

			// If this is an item with the same name, check if it can be stacked together.
			else if (currentItem.itemName.Equals(droppedItem.itemName))
			{
				if (currentItem.stackable && currentItem.quantity < currentItem.maxPerStack)
				{
					int totalQuantity = currentItem.quantity + droppedItem.quantity;

					if (totalQuantity > currentItem.maxPerStack)
					{
						int residue = totalQuantity - currentItem.maxPerStack;

						Inventory.instance.UpdateQuantity(currentItem.id, currentItem.maxPerStack, true);

						if (!ClickableObject.splittingItem)
						{
							if (cloneData.isChestSlot)
								ChestStorage.instance.UpdateQuantity(droppedItem.id, residue, true);
							else
								Inventory.instance.UpdateQuantity(droppedItem.id, residue, true);
						}

						else
						{
							Item splitItem = ClickableObject.sender.dragItem;

							if (!cloneData.isChestSlot)
							{
								if (!Inventory.instance.IsExisting(splitItem.id))
								{
									Inventory.instance.Add(splitItem, true);
									Inventory.instance.UpdateQuantity(splitItem.id, residue, true);
								}

								else
									Inventory.instance.UpdateQuantity(splitItem.id, residue);
							}
							else
							{
								if (!ChestStorage.instance.IsExisting(splitItem.id))
								{
									ChestStorage.instance.Add(splitItem, true);
									ChestStorage.instance.UpdateQuantity(splitItem.id, residue, true);
								}

								else
									ChestStorage.instance.UpdateQuantity(splitItem.id, residue);
							}
						}

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
					if (cloneData.isChestSlot)
						ChestStorage.instance.Remove(droppedItem);
					else
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
