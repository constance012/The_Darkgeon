using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CSTGames.DataPersistence;
using CSTGames.CommonEnums;

public class Inventory : ItemStorage, ISaveDataTransceiver
{
	public static Inventory instance { get; private set; }

	[Header("Item Database")]
	[Space]
	public ItemDatabase database;

	[Header("Items List")]
	[Space]
	public List<Item> items = new List<Item>();
	public int coins = 0;

	private InventorySlot[] _slots;
	private ClickableObject[] _clickables;
	private CoinSlot _coinSlot;
	private Image _outsideZone;

	protected override void OnEnable()
	{
		base.OnEnable();

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

	protected override void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Inventory found!!");
			Destroy(gameObject);
			return;
		}

		_slots = transform.GetComponentsInChildren<InventorySlot>("Slots");
		_coinSlot = transform.GetComponentInChildren<CoinSlot>("Coin Slot");
		_outsideZone = transform.parent.GetComponentInChildren<Image>("Outside Zone");
		_clickables = GetComponentsInChildren<ClickableObject>();

		base.Awake();
	}

	private void Update()
	{
		_outsideZone.raycastTarget = ClickableObject.holdingItem;
	}

	public void InitializeOtherOpenedStorage()
	{
		ItemStorage[] openedStorages = transform.parent.GetComponentsInChildren<ItemStorage>();
		ItemStorage selectedStorage = null;

		foreach (ItemStorage storage in openedStorages)
			if (storage.GetType() != typeof(Inventory))
			{
				selectedStorage = storage;
				break;
			}

		foreach (ClickableObject clickable in _clickables)
		{
			clickable.otherStorage = selectedStorage;
		}
	}

	public override bool Add(Item target, bool forcedSplit = false)
	{
		if (target.itemName.Equals("Coin"))
		{
			coins += target.quantity;
			_coinSlot.AddCoin(coins);

			onItemChanged?.Invoke();
			return true;
		}

		return base.AddToList(items, target, forcedSplit);
	}

	public override void Remove(Item target, bool forced = false)
	{
		if (!target.isFavorite || forced)
		{
			items.Remove(target);
			onItemChanged?.Invoke();
		}
	}

	public override void Remove(string targetID, bool forced = false)
	{
		Item target = GetItem(targetID);

		if (!target.isFavorite || forced)
		{
			items.Remove(target);
			onItemChanged?.Invoke();
		}
	}

	public override void RemoveWithoutNotify(Item target)
	{
		items.Remove(target);
	}

	public override Item GetItem(string targetID)
	{
		return items.Find(item => item.id.Equals(targetID));
	}

	public override bool IsExisting(string targetID)
	{
		return items.Exists(item => item.id == targetID);
	}

	public override void UpdateQuantity(string targetID, int amount, bool setExactAmount = false)
	{
		Item target = GetItem(targetID);

		base.SetQuantity(target, amount, setExactAmount);

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

	protected override void ReloadUI()
	{
		_coinSlot.AddCoin(coins);

		// Split the master list into 2 smaller lists.
		List<Item> unindexedItems = items.FindAll(item => item.slotIndex == -1);
		List<Item> indexedItems = items.FindAll(item => item.slotIndex != -1);

		//Debug.Log("Unindexed items count : " + unindexedItems.Count);
		//Debug.Log("Indexed items count : " + indexedItems.Count);

		// Clear all the _slots.
		Array.ForEach(_slots, (slot) => slot.ClearItem());

		// Load the indexed items first.
		if (indexedItems.Count != 0)
		{
			Action<Item> ReloadIndexedItems = (item) => _slots[item.slotIndex].AddItem(item);
			indexedItems.ForEach(ReloadIndexedItems);
		}

		// Secondly, load the unindexes items to the leftover empty slots.
		if (unindexedItems.Count != 0)
		{
			int i = 0;

			foreach (InventorySlot slot in _slots)
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
