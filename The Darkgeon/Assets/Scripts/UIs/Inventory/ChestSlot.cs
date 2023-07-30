using UnityEngine;

public class ChestSlot : StorageSlot
{
	/// <summary>
	/// This method used to catch the dragged item from another slot.
	/// </summary>
	/// <param name="eventData"></param>
	public override void OnDrop(GameObject shipper)
	{
		if (shipper == null)
			return;
		
		ClickableObject cloneData = shipper.GetComponent<ClickableObject>();
		
		Item droppedItem = cloneData.dragItem;

		// If this is an empty slot.
		if (currentItem == null)
		{
			// Add the item if it doesn't already exist, otherwise move the item to this slot.
			if (!ChestStorage.instance.IsExisting(droppedItem.id) || ClickableObject.splittingItem)
			{
				ChestStorage.instance.Add(droppedItem, true);
			}
				
			ChestStorage.instance.UpdateSlotIndex(droppedItem.id, transform.GetSiblingIndex());

			if (!cloneData.FromSameStorageSlot<ChestSlot>() && !ClickableObject.splittingItem)
				cloneData.currentStorage.Remove(droppedItem, true);
				
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
						cloneData.currentStorage.UpdateQuantity(droppedItem.id, residue, true);
					}

					else
					{
						Item splitItem = ClickableObject.sender.dragItem;

						if (!cloneData.currentStorage.IsExisting(splitItem.id))
						{
							cloneData.currentStorage.Add(splitItem, true);
							cloneData.currentStorage.UpdateQuantity(splitItem.id, residue, true);
						}

						else
							cloneData.currentStorage.UpdateQuantity(splitItem.id, residue);
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
				cloneData.currentStorage.Remove(droppedItem);
			}

			// If it can not be stacked or its stack is currently full, swap the slot indexes between them.
			else if (!currentItem.stackable || currentItem.quantity == currentItem.maxPerStack)
				SwapSlotIndexes<ChestSlot>(cloneData);

			return;
		}

		// If they're two different items.
		SwapSlotIndexes<ChestSlot>(cloneData);
	}
}
