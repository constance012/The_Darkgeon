using CSTGames.CommonEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CSTGames.DataPersistence
{
	[Serializable]
	public struct ItemSaveData
	{
		public string id;
		public string itemName;
		public ItemCategory category;

		public int slotIndex;
		public int quantity;
		public bool isFavorite;

		public ItemSaveData(Item itemToSave)
		{
			this.id = itemToSave.id;
			this.itemName = itemToSave.itemName;
			this.category = itemToSave.category;

			this.slotIndex = itemToSave.slotIndex;
			this.quantity = itemToSave.quantity;
			this.isFavorite = itemToSave.isFavorite;
		}
	}

	[Serializable]
	public struct ContainerSaveData
	{
		public List<ItemSaveData> storedItem;
		public bool firstTimeOpen;

		public ContainerSaveData(IEnumerable<Item> items, bool firstTimeOpen = false)
		{
			this.storedItem = new List<ItemSaveData>();

			if (items != null && items.Any())
				foreach (Item item in items)
				{
					if (item != null)
					{
						ItemSaveData itemData = new ItemSaveData(item);
						this.storedItem.Add(itemData);
					}
				}

			this.firstTimeOpen = firstTimeOpen;
		}
	}

	[Serializable]
	public class LevelData
	{
		public int levelIndex;
		public string levelName;

		/// <summary>
		/// The dictionary contains the data of all the chests in the level. Each pair has an ID and a ContainerSaveData object.
		/// </summary>
		public Dictionary<string, ContainerSaveData> chestsData;

		public Dictionary<string, bool> dialogueTriggersData;
		public Dictionary<string, bool> navigationDoorsData;
		public Dictionary<string, bool> enemiesDeathData;

		/// <summary>
		/// The values defined within this constructor will be set to the first level for a new game.
		/// <para />
		/// Or when the game has no data to load.
		/// </summary>
		public LevelData()
		{
			this.levelIndex = 3;
			this.levelName = "Tutorial";

			this.chestsData = new Dictionary<string, ContainerSaveData>();
			this.dialogueTriggersData = new Dictionary<string, bool>();
			this.navigationDoorsData = new Dictionary<string, bool>();
			this.enemiesDeathData = new Dictionary<string, bool>();
		}

		public LevelData(int levelIndex, string levelName)
		{
			this.levelIndex = levelIndex;
			this.levelName = levelName;

			this.chestsData = new Dictionary<string, ContainerSaveData>();
			this.dialogueTriggersData = new Dictionary<string, bool>();
			this.navigationDoorsData = new Dictionary<string, bool>();
			this.enemiesDeathData = new Dictionary<string, bool>();
		}
	}

	[Serializable]
	public class PlayerData
	{
		/// <summary>
		/// Store the total played time of this data (totalHours, minutes, seconds). 
		/// </summary>
		private Vector3Int totalPlayedTime = Vector3Int.zero;
		public Vector3Int TotalPlayedTime
		{
			get { return totalPlayedTime; }
			set
			{
				value.y += value.z / 60;
				value.z -= 60 * (value.z / 60);

				value.x += value.y / 60;
				value.y -= 60 * (value.y / 60);

				totalPlayedTime = value;
			}
		}

		/// <summary>
		/// Store the last updated time as a 64-bit signed integer.
		/// </summary
		public long lastUpdated;

		public int lastPlayedLevel;

		public Vector3 playerPosition;

		public int coinsCollected;
		public ContainerSaveData inventoryData;
		public ContainerSaveData equipmentData;

		public PlayerData()
		{
			lastPlayedLevel = 3;
			playerPosition = Vector3.zero;
			coinsCollected = 0;
			inventoryData = default;
			equipmentData = default;
		}
	}

	[Serializable]
	public class GameData
	{
		public PlayerData playerData { get; set; }
		public LevelData levelData { get; set; }

		public bool HasPlayerData => this.playerData != null;

		public bool AllDataLoadedSuccessfully => this.playerData != null && this.levelData != null;

		public GameData(bool isNull = false)
		{
			if (isNull)
			{
				this.playerData = null;
				this.levelData = null;
			}
			else
			{
				this.playerData = new PlayerData();
				this.levelData = new LevelData();
			}
		}
	}
}