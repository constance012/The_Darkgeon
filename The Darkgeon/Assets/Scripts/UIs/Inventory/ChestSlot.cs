using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestSlot : MonoBehaviour
{
	[Header("Current Item")]
	[Space]

	public Item currentItem;

	// Private fields.
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

		tooltip.header = currentItem.itemName;
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

			// Local function.
			void SwapSlotIndexes()
			{
				int senderIndex = droppedItem.slotIndex;
				int destinationIndex = currentItem.slotIndex;

				// If swap within the chest.
				if (cloneData.isChestSlot)
				{
					if (!ChestStorage.instance.IsExisting(droppedItem.id))
						ChestStorage.instance.Add(droppedItem, true);

					else if (ClickableObject.splittingItem)
						ChestStorage.instance.UpdateQuantity(droppedItem.id, droppedItem.quantity);

					// Swap their slot indexes.
					ChestStorage.instance.UpdateSlotIndex(currentItem.id, senderIndex);
					ChestStorage.instance.UpdateSlotIndex(droppedItem.id, destinationIndex);
				}

				// If swap within the chest and other storages.
				else
				{
					if (!Inventory.instance.IsExisting(droppedItem.id))
						Inventory.instance.Add(droppedItem, true);

					else if (ClickableObject.splittingItem)
					{
						Inventory.instance.UpdateQuantity(droppedItem.id, droppedItem.quantity);
						droppedItem.quantity = Inventory.instance.GetItem(droppedItem.id).quantity;
					}

					Item destinationItem = Instantiate(currentItem);
					destinationItem.name = currentItem.name;

					Inventory.instance.Remove(droppedItem.id);
					ChestStorage.instance.Remove(destinationItem.id);

					destinationItem.slotIndex = senderIndex;
					droppedItem.slotIndex = destinationIndex;

					Inventory.instance.Add(destinationItem, true);
					ChestStorage.instance.Add(droppedItem, true);
				}		

				Debug.Log("Dragged item index " + droppedItem.slotIndex);
			}

			// If this is an empty slot.
			if (currentItem == null)
			{
				// Add the item if it doesn't already exist, otherwise move the item to this slot.
				if (!ChestStorage.instance.IsExisting(droppedItem.id) || ClickableObject.splittingItem)
				{
					droppedItem.slotIndex = transform.GetSiblingIndex();
					ChestStorage.instance.Add(droppedItem, true);
				}
				
				ChestStorage.instance.UpdateSlotIndex(droppedItem.id, transform.GetSiblingIndex());

				if (!cloneData.isChestSlot && !ClickableObject.splittingItem)
					Inventory.instance.Remove(droppedItem, true);
				
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

						ChestStorage.instance.UpdateQuantity(currentItem.id, currentItem.maxPerStack, true);

						if (!ClickableObject.splittingItem)
						{
							if (!cloneData.isChestSlot)
								Inventory.instance.UpdateQuantity(droppedItem.id, residue, true);
							else
								ChestStorage.instance.UpdateQuantity(droppedItem.id, residue, true);
						}

						else
						{
							Item splitItem = ClickableObject.sender.dragItem;

							if (cloneData.isChestSlot)
							{
								if (!ChestStorage.instance.IsExisting(splitItem.id))
								{
									ChestStorage.instance.Add(splitItem, true);
									ChestStorage.instance.UpdateQuantity(splitItem.id, residue, true);
								}

								else
									ChestStorage.instance.UpdateQuantity(splitItem.id, residue);
							}
							else
							{
								if (!Inventory.instance.IsExisting(splitItem.id))
								{
									Inventory.instance.Add(splitItem, true);
									Inventory.instance.UpdateQuantity(splitItem.id, residue, true);
								}

								else 
									Inventory.instance.UpdateQuantity(splitItem.id, residue);
							}
						}

						return;
					}

					else if (totalQuantity == currentItem.maxPerStack)
						ChestStorage.instance.UpdateQuantity(currentItem.id, totalQuantity, true);

					// Otherwise, just increase the quantity of the current one.
					else
						ChestStorage.instance.UpdateQuantity(currentItem.id, droppedItem.quantity);


					// Set as favorite when needed.
					if (!currentItem.isFavorite && droppedItem.isFavorite)
						ChestStorage.instance.SetFavorite(currentItem.id, true);


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
