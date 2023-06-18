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

	[Space]
	[Header("General Info")]
	[SerializeField, ReadOnly] private string id;

	[ContextMenu("Generate Chest ID")]
	private void GenerateChestID()
	{
		id = Guid.NewGuid().ToString();
	}

	public ChestTier tier;
	public float distanceBeforeClosed;

	/// <summary>
	/// The quantity of each item type for randomizing if these chests have never been opened for the first time.
	/// </summary>
	[Space]
	private static Dictionary<ItemCategory, int> treasureAmount = new Dictionary<ItemCategory, int>
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

	public List<Item> storedItem = new List<Item>();

	[Space]
	// Special items list for each chest.
	[SerializeField] private List<DeathLoot> treasures = new List<DeathLoot>();

	// Private fields.
	private Animator animator;
	private bool firstTimeOpen;

	protected override void Awake()
	{
		base.Awake();
		animator = GetComponent<Animator>();
		mat = GetComponentInChildren<SpriteRenderer>().material;
	}

	private void Start()
	{
		if (!GameDataManager.instance.enableManager)
			InitializeTreasures();

		// Use this to check if the chest is opened or not. True if the chest is closed.
		hasInteracted = true;
	}

	protected override void Update()
	{
		Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		float mouseDistance = Vector2.Distance(worldMousePos, transform.position);
		float playerDistance = Vector2.Distance(player.position, transform.position);

		if (mouseDistance <= radius)
		{
			if (clone == null)
				CreatePopupLabel();
			else
				clone.transform.position = transform.position;

			mat.SetFloat("_Thickness", .002f);

			if (Input.GetMouseButtonDown(1) && playerDistance <= distanceBeforeClosed)
			{
				Chest target = ChestStorage.instance.openedChest;

				// Close any currently opening chest before opening the new one.
				if (target != null && target != this)
					target.Interact();
				
				Interact();
			}
		}

		else
		{
			Destroy(clone);

			mat.SetFloat("_Thickness", 0f);

			// Close the chest when out of range.
			if (!hasInteracted && playerDistance > distanceBeforeClosed)
			{
				hasInteracted = true;
				OpenAndClose();
			}
		}
	}

	public override void Interact()
	{
		base.Interact();

		hasInteracted = !hasInteracted;

		OpenAndClose();
	}

	public override void ExecuteRemoteLogic(bool state)
	{
		
	}

	public void LoadData(GameData gameData)
	{
		ContainerSaveData loadedData;

		gameData.levelData.chestsData.TryGetValue(id, out loadedData);

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
		LevelData levelData = gameData.levelData;

		if (levelData.chestsData.ContainsKey(id))
		{
			levelData.chestsData.Remove(id);
		}

		ContainerSaveData dataToSave = new ContainerSaveData(storedItem, firstTimeOpen);

		levelData.chestsData.Add(id, dataToSave);
	}

	protected override void CreatePopupLabel()
	{
		base.CreatePopupLabel();

		clone.transform.Find("Names/Object Name").GetComponent<TextMeshProUGUI>().text = type.ToString().ToUpper() + " CHEST";
	}

	private void OpenAndClose()
	{
		if (!hasInteracted)
		{
			animator.SetTrigger("Open");
			firstTimeOpen = true;
			
			// Activate the Inventory canvas if it hasn't already open yet.
			if (!Inventory.instance.transform.parent.gameObject.activeInHierarchy)
				Inventory.instance.transform.parent.gameObject.SetActive(true);

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
			if ((int)x.loot.category < (int)y.loot.category) return -1;
			else if ((int)x.loot.category > (int)y.loot.category) return 1;
			else return 0;
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

				numberOfItemsRemaining = treasureAmount[currentType];
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

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, distanceBeforeClosed);
	}
}