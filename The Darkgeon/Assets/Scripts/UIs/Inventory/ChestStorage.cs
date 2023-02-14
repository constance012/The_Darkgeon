using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChestStorage : MonoBehaviour
{
	public static ChestStorage instance { get; private set; }

	[Header("References")]
	[Space]

	public Chest openedChest;
	public int space = 10;

	// Delegate.
	public delegate void OnItemChanged();
	public OnItemChanged onItemChanged;

	private ChestSlot[] slots;
	private TextMeshProUGUI uiTitle;

	private void OnEnable()
	{
		ReloadUI();
		uiTitle.text = openedChest?.type.ToString().ToUpper() + " CHEST";
	}

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Chest Storage found!!");
			Destroy(gameObject);
			return;
		}

		slots = transform.Find("Slots").GetComponentsInChildren<ChestSlot>();
		uiTitle = transform.Find("Title/Text").GetComponent<TextMeshProUGUI>();

		gameObject.SetActive(false);
	}

	private void Start()
	{
		instance.onItemChanged += ReloadUI;
	}

	public bool Add(Item target, bool forcedSplit = false)
	{
		List<Item> items = openedChest.storedItem;

		if (items.Count >= space)
		{
			Debug.Log("This chest is full.");
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
				{
					items.Add(target);
					Debug.Log("This is empty slot.");
				}

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

	public void Remove(Item target)
	{
		openedChest.storedItem.Remove(target);
		onItemChanged?.Invoke();
	}

	public void Remove(string targetID)
	{
		openedChest.storedItem.Remove(GetItem(targetID));
		onItemChanged?.Invoke();
	}

	public Item GetItem(string targetID)
	{
		return openedChest.storedItem.Find(item => item.id.Equals(targetID));
	}

	public bool IsExisting(string targetID)
	{
		return openedChest.storedItem.Exists(item => item.id == targetID);
	}

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

	public void UpdateQuantity(string targetID, int amount, bool setExactAmount = false)
	{
		Item target = GetItem(targetID);

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
		if (openedChest == null)
			return;

		// Split the master list into 2 smaller lists.
		List<Item> unindexedItems = openedChest.storedItem.FindAll(item => item.slotIndex == -1);
		List<Item> indexedItems = openedChest.storedItem.FindAll(item => item.slotIndex != -1);

		//Debug.Log("Unindexed items in chest count : " + unindexedItems.Count);
		//Debug.Log("Indexed items in chest count : " + indexedItems.Count);

		// Clear all the slots.
		Action<ChestSlot> ClearAllSlots = (slot) => slot.ClearItem();
		Array.ForEach(slots, ClearAllSlots);

		// Load the indexed items first.
		if (indexedItems.Count != 0)
		{
			Action<Item> ReloadIndexedItems = (item) => slots[item.slotIndex].AddItem(item);
			indexedItems.ForEach(ReloadIndexedItems);
		}

		// Secondly, load the unindexes items to the leftover empty slots.
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
		openedChest.storedItem.Clear();
		openedChest.storedItem.AddRange(unindexedItems);
		openedChest.storedItem.AddRange(indexedItems);

		// Sort the list by slot indexes in ascending order.
		openedChest.storedItem.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));
	}
}
