using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChestStorage : ItemStorage
{
	public static ChestStorage instance { get; private set; }

	[Header("Target Chest")]
	[Space]
	public Chest openedChest;

	// Private fields.
	private ChestSlot[] _slots;
	private TextMeshProUGUI _uiTitle;

	protected override void OnEnable()
	{
		base.OnEnable();
		_uiTitle.text = openedChest?.tier.ToString().ToUpper() + " CHEST";

		Inventory.instance.InitializeOtherOpenedStorage();
	}

	protected override void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Chest Storage found!!");
			Destroy(gameObject);
			return;
		}

		_slots = transform.GetComponentsInChildren<ChestSlot>("Slots");
		_uiTitle = transform.GetComponentInChildren<TextMeshProUGUI>("Title/Text");

		gameObject.SetActive(false);
		base.Awake();
	}

	public override bool Add(Item target, bool forcedSplit = false)
	{
		List<Item> items = openedChest.storedItem;

		return base.AddToList(items, target, forcedSplit);
	}

	public override void Remove(Item target, bool forced = false)
	{
		openedChest.storedItem.Remove(target);
		onItemChanged?.Invoke();
	}

	public override void Remove(string targetID, bool forced = false)
	{
		openedChest.storedItem.Remove(GetItem(targetID));
		onItemChanged?.Invoke();
	}

	public override void RemoveWithoutNotify(Item target)
	{
		openedChest.storedItem.Remove(target);
	}

	public override Item GetItem(string targetID)
	{
		return openedChest.storedItem.Find(item => item.id.Equals(targetID));
	}

	public override bool IsExisting(string targetID)
	{
		return openedChest.storedItem.Exists(item => item.id == targetID);
	}

	public override void UpdateQuantity(string targetID, int amount, bool setExactAmount = false)
	{
		Item target = GetItem(targetID);

		base.SetQuantity(target, amount, setExactAmount);

		if (target.quantity <= 0)
		{
			Remove(target);
			return;
		}

		onItemChanged?.Invoke();
	}

	protected override void ReloadUI()
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
		Array.ForEach(_slots, ClearAllSlots);

		// Load the indexed items first.
		if (indexedItems.Count != 0)
		{
			Action<Item> ReloadIndexedItems = (item) => _slots[item.slotIndex].AddItem(item);
			indexedItems.ForEach(ReloadIndexedItems);
		}

		// Secondly, load the unindexes items to the leftover empty slots.
		if (unindexedItems.Count != 0)
		{
			for (int i = 0; i < unindexedItems.Count; i++)
				for (int j = 0; j < _slots.Length; j++)
					if (_slots[j].currentItem == null)
					{
						unindexedItems[i].slotIndex = _slots[j].transform.GetSiblingIndex();

						_slots[j].AddItem(unindexedItems[i]);

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
