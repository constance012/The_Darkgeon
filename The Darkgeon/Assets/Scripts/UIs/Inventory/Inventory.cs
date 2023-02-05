using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	public static Inventory instance { get; private set; }

	public delegate void OnItemChanged();
	public OnItemChanged onItemChanged;

	// Fields
	public List<Item> items = new List<Item>();
	private InventorySlot[] slots;

	public int space = 20;

	private void OnEnable()
	{
		ReloadUI();
	}

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Inventory found!!");
			Destroy(gameObject);
			return;
		}

		slots = transform.Find("Slots").GetComponentsInChildren<InventorySlot>();
	}

	private void Start()
	{
		instance.onItemChanged += ReloadUI;
	}

	public bool Add(Item target, bool forcedSplit = false)
	{
		if (items.Count >= space)
		{
			Debug.Log("Inventory Full.");
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

	public void Remove(Item target, bool forced = false)
	{
		if (!target.isFavorite || forced)
		{
			items.Remove(target);
			onItemChanged?.Invoke();
		}
	}

	public void SetFavorite(string targetID, bool state)
	{
		items.Find(item => item.id.Equals(targetID)).isFavorite = state;
		onItemChanged?.Invoke();
	}

	public void UpdateSlotIndex(string targetID, int index)
	{
		items.Find(item => item.id.Equals(targetID)).slotIndex = index;
		onItemChanged?.Invoke();
	}

	public void UpdateQuantity(string targetID, int amount, bool setExactAmount = false)
	{
		Item target = items.Find(item => item.id.Equals(targetID));

		if (setExactAmount)
			target.quantity = amount;
		else
			target.quantity += amount;

		target.quantity = Mathf.Clamp(target.quantity, 0, target.maxPerStack);

		if (target.quantity <= 0)
		{
			Remove(target);
			return;
		}

		onItemChanged?.Invoke();
	}

	private void ReloadUI()
	{
		// Split the master list into 2 smaller lists.
		List<Item> unindexedItems = items.FindAll(item => item.slotIndex == -1);
		List<Item> indexedItems = items.FindAll(item => item.slotIndex != -1);

		Debug.Log("Unindexed items count : " + unindexedItems.Count);
		Debug.Log("Indexed items count : " + indexedItems.Count);

		// Clear all the slots.
		Action<InventorySlot> ClearAllSlots = (slot) => slot.ClearItem();
		Array.ForEach(slots, ClearAllSlots);

		if (indexedItems.Count != 0)
		{
			Action<Item> ReloadIndexedItems = (item) => slots[item.slotIndex].AddItem(item);
			indexedItems.ForEach(ReloadIndexedItems);
		}

		if (unindexedItems.Count != 0)
		{
			for (int i = 0; i < unindexedItems.Count; i++)
				for (int j = 0; j < slots.Length; j++)
					if (slots[j].currentItem == null)
					{
						unindexedItems[i].slotIndex = slots[j].transform.GetSiblingIndex();

						slots[j].AddItem(unindexedItems[i]);

						break;
					}
		}

		// Update the master list.
		items.Clear();
		items.AddRange(unindexedItems);
		items.AddRange(indexedItems);

		// Sort the list by slot indexes in ascending order.
		items.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));
	}
}
