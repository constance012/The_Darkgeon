using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ItemStorage : MonoBehaviour
{
	public UnityEvent onItemChanged { get; private set; } = new UnityEvent();

	[Header("Storage Space")]
	[Space]
	public int space;

	protected virtual void Awake()
	{
		onItemChanged.AddListener(ReloadUI);
	}

	protected virtual void OnEnable()
	{
		onItemChanged?.Invoke();
	}

	protected bool AddToList(List<Item> items, Item target, bool forcedSplit)
	{
		if (items.Count >= space)
		{
			Debug.LogWarning("This storage ran out of space.", this);
			return false;
		}

		// Add to the list if it's not a default item.
		if (!target.isDefaultItem)
		{
			// Generate a unique id for the target.
			target.id = Guid.NewGuid().ToString();

			if (!forcedSplit)
			{
				// Check for stackable items.
				for (int i = 0; i < items.Count; i++)
				{
					if (!items[i].itemName.Equals(target.itemName))
						continue;

					if (items[i].quantity == items[i].maxPerStack || !items[i].stackable)
						continue;

					// If the item is stackable and hasn't reached its max per stack yet.
					if (items[i].quantity < items[i].maxPerStack)
					{
						int totalQuantity = items[i].quantity + target.quantity;

						// If the new total quantity exceeds the maximum amount.
						// Then set the current one's quantity to max, set the new one's quantity to the residue amount and add to the next slot.
						if (totalQuantity > items[i].maxPerStack)
						{
							int residue = totalQuantity - items[i].maxPerStack;

							items[i].quantity = items[i].maxPerStack;
							target.quantity = residue;
						}

						else if (totalQuantity == items[i].maxPerStack)
						{
							items[i].quantity = totalQuantity;
							target.quantity = 0;
						}

						// Otherwise, just increase the quantity of the current one.
						else
						{
							items[i].quantity += target.quantity;
							target.quantity = 0;
						}
					}

					if (target.quantity <= 0)
						break;
				}

				// If there's a residue or this is a completely different item. Then add it to the list.
				if (target.quantity > 0)
					items.Add(target);

				onItemChanged?.Invoke();
				return true;
			}

			// If it's a completely new item or forced to split the same item, then just add it into the list.
			if (target.quantity > 0)
				items.Add(target);

			onItemChanged?.Invoke();
			return true;
		}

		return false;
	}

	public abstract bool Add(Item target, bool forcedSplit = false);

	public abstract void Remove(Item target, bool forced = false);

	public abstract void Remove(string targetID, bool forced = false);

	public abstract void RemoveWithoutNotify(Item target);

	public abstract Item GetItem(string targetID);

	public abstract bool IsExisting(string targetID);

	public void SetFavorite(string targetID, bool state)
	{
		GetItem(targetID).isFavorite = state;
		onItemChanged?.Invoke();
	}

	public void UpdateSlotIndex(string targetID, int index)
	{
		index = Mathf.Clamp(index, 0, space - 1);
		GetItem(targetID).slotIndex = index;
		onItemChanged?.Invoke();
	}
	
	public abstract void UpdateQuantity(string targetID, int amount, bool setExactAmount = false);

	protected void SetQuantity(Item target, int amount, bool setExactAmount = false)
	{
		if (setExactAmount)
			target.quantity = amount;
		else
			target.quantity += amount;

		target.quantity = Mathf.Clamp(target.quantity, 0, target.maxPerStack);
	}

	protected abstract void ReloadUI();
}
