using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CSTGames.DataPersistence;
using System.Linq;

public class EquipmentManager : Singleton<EquipmentManager>, ISaveDataTransceiver
{
	[Serializable]
	public class EquipmentEvent : UnityEvent<Equipment, Equipment> { }

	[Header("Current Equipments")]
	[Space]
	[SerializeField] private Equipment[] currentEquipments;

	public EquipmentEvent onEquipmentChanged { get; private set; } = new EquipmentEvent();

	// Private fields.
	private EquipmentSlot[] slots;
	private TextMeshProUGUI[] statTexts;

	private Image portrait;
	private SpriteRenderer playerSprite;
	private PlayerStats playerStats;

	private TextMeshProUGUI toggleText;
	private TextMeshProUGUI titleText;
	private GameObject statsPanel;
	private Button unequipAllButton;

	private void OnEnable()
	{
		ReloadUI();
	}

	protected override void Awake()
	{
		base.Awake();

		slots = transform.GetComponentsInChildren<EquipmentSlot>("Slots");
		statTexts = transform.GetComponentsInChildren<TextMeshProUGUI>("Stats Panel/Scroll View/Viewport/Content");

		Transform player = GameObject.FindWithTag("Player").transform;
		playerSprite = player.GetComponentInChildren<SpriteRenderer>("Graphic");
		playerStats = player.GetComponent<PlayerStats>();

		portrait = transform.GetComponentInChildren<Image>("Portrait/Player Sprite");

		toggleText = transform.GetComponentInChildren<TextMeshProUGUI>("Stats Toggle/Text");
		titleText = transform.GetComponentInChildren<TextMeshProUGUI>("Title/Text");
		statsPanel = transform.Find("Stats Panel").gameObject;
		unequipAllButton = transform.GetComponentInChildren<Button>("Unequip All Button");
		
		int numberOfSlots = Enum.GetNames(typeof(Equipment.EquipmentType)).Length;
		currentEquipments = new Equipment[numberOfSlots];
	}

	private void Update()
	{
		if (this.enabled)
			portrait.sprite = playerSprite.sprite;
	}

	public void Equip(Equipment target)
	{
		int index = (int)target.type;

		// If there's an equipment of the same type equiped, then swap it with the target.
		Equipment oldEquipment = null;

		if (currentEquipments[index] != null)
		{
			oldEquipment = currentEquipments[index];
			oldEquipment.slotIndex = -1;

			Inventory.instance.Add(oldEquipment, true);
		}

		currentEquipments[index] = target;

		onEquipmentChanged?.Invoke(target, oldEquipment);
		ReloadUI();
	}

	public void Unequip(Equipment target)
	{
		Debug.Log("Unequiping " + target.itemName);

		int index = (int)target.type;
		target.slotIndex = -1;

		Inventory.instance.Add(target, true);

		currentEquipments[index] = null;

		onEquipmentChanged?.Invoke(null, target);
		ReloadUI();
	}

	public void UnequipAll()
	{
		foreach (Equipment equipment in currentEquipments)
			if (equipment != null)
				Unequip(equipment);
	}

	/// <summary>
	/// Callback method for the display stat toggle's event.
	/// </summary>
	/// <param name="state"></param>
	public void OnStatsToggled(bool state)
	{
		statsPanel.SetActive(state);
		unequipAllButton.interactable = !state;

		if (!state)
		{
			titleText.text = "EQUIPMENTS";
			toggleText.text = "STATS";
		}
		else
		{
			titleText.text = "STATS";
			toggleText.text = "EQUIPMENTS";
		}
	}

	public void LoadData(GameData gameData)
	{
		ContainerSaveData loadedEquipments = gameData.playerData.equipmentData;

		if (loadedEquipments.storedItem != null && loadedEquipments.storedItem.Any())
		{
			ItemDatabase database = Inventory.instance.database;

			foreach (ItemSaveData equipmentData in loadedEquipments.storedItem)
			{
				Equipment equipment = database.GetItem(equipmentData) as Equipment;

				int index = (int)equipment.type;
				currentEquipments[index] = equipment;

				onEquipmentChanged?.Invoke(equipment, null);
			}
		}
	}

	public void SaveData(GameData gameData)
	{
		gameData.playerData.equipmentData = new ContainerSaveData(currentEquipments);
	}
	
	private void ReloadUI()
	{
		// Clear all the slots first.
		Array.ForEach(slots, slot => slot.ClearEquipment());

		for (int i = 0; i < currentEquipments.Length; i++)
			if (currentEquipments[i] != null)
			{
				int index = (int)currentEquipments[i].type;
				slots[index].AddEquipment(currentEquipments[i]);
			}

		statTexts[0].text = $"DAMAGE: {playerStats.atkDamage.Value}";
		statTexts[1].text = $"MAX HEALTH: {Mathf.Round(playerStats.maxHP.Value)}";
		statTexts[2].text = $"ARMOR: {Mathf.Round(playerStats.armor.Value)}";
		statTexts[3].text = $"CRITICAL CHANCE: {playerStats.criticalChance.Value}%";
		statTexts[4].text = $"CRITICAL DAMAGE: {playerStats.criticalDamage.Value}%";
		statTexts[5].text = $"KNOCKBACK: {playerStats.knockBackVal.Value}";
		statTexts[6].text = $"KNOCKBACK RESISTANCE: {Mathf.Clamp(playerStats.knockBackRes.Value, 0f, 1f) * 100f}%";
		statTexts[7].text = $"MOVEMENT SPEED: {playerStats.m_MoveSpeed.Value}";
		statTexts[8].text = $"REGENERATION RATE: {playerStats.m_RegenRate.Value} HP/s";
		statTexts[9].text = $"INVINCIBILITY TIME: {playerStats.m_InvincibilityTime.Value} secs";
	}
}
