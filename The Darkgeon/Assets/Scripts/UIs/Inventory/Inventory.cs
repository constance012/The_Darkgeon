using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CSTGames.DataPersistence;
using CSTGames.CommonEnums;

public class Inventory : MonoBehaviour, ISaveDataTransceiver
{
	public static Inventory instance { get; private set; }

	public UnityEvent onItemChanged { get; private set; } = new UnityEvent();

	[Header("Item Database")]
	[Space]
	public ItemDatabase database;

	[Header("Items List")]
	[Space]
	public List<Item> items = new List<Item>();
	public int coins = 0;
	public int space = 20;

	private InventorySlot[] slots;
	private CoinSlot coinSlot;
	private Image outsideZone;

	private void OnEnable()
	{
		onItemChanged?.Invoke();
		PlayerMovement.isModifierKeysOccupied = true;
		PlayerActions.canAttack = false;
	}

	private void OnDisable()
	{
		if (ChestStorage.instance.openedChest != null)
		{
			ChestStorage.instance.openedChest.Interact();
			ChestStorage.instance.gameObject.SetActive(false);
		}

		TooltipHandler.Hide();
		PlayerMovement.isModifierKeysOccupied = false;
		PlayerActions.canAttack = true;
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
		coinSlot = transform.Find("Coin Slot").GetComponent<CoinSlot>();
		outsideZone = transform.root.Find("Outside Zone").GetComponent<Image>();
		
		onItemChanged.AddListener(ReloadUI);
	}

	private void Update()
	{
		outsideZone.raycastTarget = ClickableObject.holdingItem;
	}

	public bool Add(Item target, bool forcedSplit = false)
	{
		if (target.itemName.Equals("Coin"))
		{
			coins += target.quantity;
			coinSlot.AddCoin(coins);

			onItemChanged?.Invoke();
			return true;
		}
		
		if (items.Count >= space)
		{
			Debug.LogWarning("Inventory Full.");
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

	public void Remove(string targetID, bool forced = false)
	{
		Item target = GetItem(targetID);

		if (!target.isFavorite || forced)
		{
			items.Remove(target);
			onItemChanged?.Invoke();
		}
	}

	public Item GetItem(string targetID) => items.Find(item => item.id.Equals(targetID));

	public List<Item> GetItemsByName(string name) => items.FindAll(item => item.itemName.Equals(name));

	public bool IsExisting(string targetID) => items.Exists(item => item.id == targetID);

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
			Remove(target, true);
			return;
		}

		onItemChanged?.Invoke();
	}

	public void LoadData(GameData gameData)
	{
		this.coins = gameData.playerData.coinsCollected;

		ContainerSaveData loadedInventory = gameData.playerData.inventoryData;

		if (loadedInventory.storedItem != null && loadedInventory.storedItem.Any())
		{
			foreach(ItemSaveData item in loadedInventory.storedItem)
			{
				switch (item.category)
				{
					case ItemCategory.Equipment:
						var equipment = database.GetItem(item) as Equipment;
						
						this.items.Add(equipment);
						break;

					case ItemCategory.Food:
						var food = database.GetItem(item) as Food;

						this.items.Add(food);
						break;

					default:
						var baseItem = database.GetItem(item);

						this.items.Add(baseItem);
						break;
				}
			}
		}
	}

	public void SaveData(GameData gameData)
	{
		PlayerData playerData = gameData.playerData;

		playerData.coinsCollected = this.coins;
		playerData.inventoryData = new ContainerSaveData(items);
	}

	private void ReloadUI()
	{
		coinSlot.AddCoin(coins);

		// Split the master list into 2 smaller lists.
		List<Item> unindexedItems = items.FindAll(item => item.slotIndex == -1);
		List<Item> indexedItems = items.FindAll(item => item.slotIndex != -1);

		//Debug.Log("Unindexed items count : " + unindexedItems.Count);
		//Debug.Log("Indexed items count : " + indexedItems.Count);

		// Clear all the slots.
		Array.ForEach(slots, (slot) => slot.ClearItem());

		// Load the indexed items first.
		if (indexedItems.Count != 0)
		{
			Action<Item> ReloadIndexedItems = (item) => slots[item.slotIndex].AddItem(item);
			indexedItems.ForEach(ReloadIndexedItems);
		}

		// Secondly, load the unindexes items to the leftover empty slots.
		if (unindexedItems.Count != 0)
		{
			int i = 0;

			foreach (InventorySlot slot in slots)
			{
				if (i == unindexedItems.Count)
					break;

				if (slot.currentItem == null)
				{
					unindexedItems[i].slotIndex = slot.transform.GetSiblingIndex();

					slot.AddItem(unindexedItems[i]);

					i++;
				}
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
