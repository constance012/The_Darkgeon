using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CSTGames.CommonEnums;
using CSTGames.DataPersistence;

public class Chest : Interactable, ISaveDataTransceiver
{
	/// <summary>
	/// Represent every type of chest.
	/// </summary>
	public enum ChestTier { Wooden, Iron, Silver, Golden }

	public ChestTier tier;

	public List<Item> storedItem = new List<Item>();

	[Space]
	// List of pre-initialize items for each chest.
	[SerializeField] private List<DeathLoot> treasures = new List<DeathLoot>();

	// Private fields.
	/// <summary>
	/// The quantity of each item type for randomizing if these chests have never been opened for the first time.
	/// </summary>
	[Space]
	private readonly static Dictionary<ItemCategory, int> _treasureAmount = new Dictionary<ItemCategory, int>
	{
		[ItemCategory.Null] = 0,
		[ItemCategory.Coin] = 1,
		[ItemCategory.Equipment] = 2,
		[ItemCategory.Weapon] = 1,
		[ItemCategory.Food] = 3,
		[ItemCategory.Potion] = 2,
		[ItemCategory.Material] = 3,
		[ItemCategory.Mineral] = 3
	};

	private Animator animator;
	private bool firstTimeOpen;

	protected override void Awake()
	{
		base.Awake();
		animator = GetComponentInParent<Animator>();
	}

	private void Start()
	{
		if (!GameDataManager.instance.enableManager)
			InitializeTreasures();

		// Use this to check if the chest is opened or not. True if the chest is closed.
		hasInteracted = true;
	}

	protected override void TriggerInteraction(float playerDistance)
	{
		base.TriggerInteraction(playerDistance);

		if (Input.GetMouseButtonDown(1) && playerDistance <= interactDistance)
		{
			Chest target = ChestStorage.instance.openedChest;

			// Close any currently opening chest before opening the new one.
			if (target != null && target != this)
				target.Interact();

			Interact();
		}
	}

	protected override void CancelInteraction(float playerDistance)
	{
		base.CancelInteraction(playerDistance);

		// Close the chest when out of range.
		if (!hasInteracted && playerDistance > interactDistance)
		{
			hasInteracted = true;
			OpenAndClose();
		}
	}

	public override void Interact()
	{
		base.Interact();

		hasInteracted = !hasInteracted;

		OpenAndClose();
	}

	#region Save and Load Data.
	public void LoadData(GameData gameData)
	{
		bool hasData = gameData.levelData.chestsData.TryGetValue(ID, out ContainerSaveData loadedData);

		if (!hasData)
		{
			InitializeTreasures();
			return;
		}

		// If the saved list is not null and not empty.
		if (loadedData.storedItem != null && loadedData.storedItem.Any())
		{
			ItemDatabase database = Inventory.instance.database;

			foreach (ItemSaveData item in loadedData.storedItem)
			{
				switch (item.category)
				{
					case ItemCategory.Equipment:
						var equipment = database.GetItem(item) as Equipment;

						this.storedItem.Add(equipment);
						break;

					case ItemCategory.Food:
						var food = database.GetItem(item) as Food;

						this.storedItem.Add(food);
						break;

					default:
						var baseItem = database.GetItem(item);

						this.storedItem.Add(baseItem);
						break;
				}

			}
		}

		firstTimeOpen = loadedData.firstTimeOpen;

		if (!firstTimeOpen)
		{
			InitializeTreasures();
			return;
		}

		treasures.Clear();
	}

	public void SaveData(GameData gameData)
	{
		if (ID == null || ID.Equals(""))
		{
			Debug.LogWarning($"This {tier} Chest doesn't have an ID yet, its data will not be stored.", this);
			return;
		}

		LevelData levelData = gameData.levelData;

		if (levelData.chestsData.ContainsKey(ID))
		{
			levelData.chestsData.Remove(ID);
		}

		ContainerSaveData dataToSave = new ContainerSaveData(storedItem, firstTimeOpen);

		levelData.chestsData.Add(ID, dataToSave);
	}
	#endregion

	protected override void CreatePopupLabel()
	{
		base.CreatePopupLabel();

		clone.SetObjectName($"{tier} chest");
	}

	private void OpenAndClose()
	{
		if (!hasInteracted)
		{
			animator.SetTrigger("Open");
			firstTimeOpen = true;
			
			// Activate the Inventory canvas if it hasn't already open yet.
			if (!Inventory.instance.CanvasActive)
				Inventory.instance.CanvasActive = true;

			ChestStorage.instance.openedChest = this;
			ChestStorage.instance.gameObject.SetActive(true);

		}
		else
		{
			animator.SetTrigger("Close");

			ChestStorage.instance.openedChest = null;
			ChestStorage.instance.gameObject.SetActive(false);
		}
	}

	private void InitializeTreasures()
	{
		if (treasures.Count == 0)
			return;

		// Essential local variables.
		ItemCategory currentType = ItemCategory.Null;
		List<DeathLoot> itemsOfCurrentType = new List<DeathLoot>();

		int numberOfItemsRemaining = 0;

		// Explicit writing of int.CompareTo(int value) method.
		treasures.Sort((x, y) =>
		{
			return (int)x.loot.category - (int)y.loot.category;
		});

		foreach (DeathLoot target in treasures)
		{
			if (target.isGuaranteed)
			{
				Item guaranteedItem = Instantiate(target.loot);
				
				guaranteedItem.id = Guid.NewGuid().ToString();
				guaranteedItem.name = target.loot.name;
				guaranteedItem.quantity = target.quantity;

				storedItem.Add(guaranteedItem);
				continue;
			}

			// If the target treasure type is changed, than update these locals.
			else if (target.loot.category != currentType)
			{
				currentType = target.loot.category;
				itemsOfCurrentType = treasures.FindAll(loot => loot.loot.category == currentType);

				numberOfItemsRemaining = _treasureAmount[currentType];
			}

			// Skip this iteration if items of the current type are fully added.
			if (numberOfItemsRemaining <= 0 || itemsOfCurrentType.Count == 0)
				continue;

			DeathLoot selectedLoot = itemsOfCurrentType.GetRandomItem();

			Item otherItem = Instantiate(selectedLoot.loot);

			otherItem.id = Guid.NewGuid().ToString();
			otherItem.name = selectedLoot.loot.name;
			otherItem.quantity = selectedLoot.quantity;

			storedItem.Add(otherItem);
			itemsOfCurrentType.Remove(selectedLoot);

			numberOfItemsRemaining--;
		}

		treasures.Clear();
	}
}