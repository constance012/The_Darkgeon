using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
	public static EquipmentManager instance { get; private set; }

	[Header("Current Equipments")]
	[Space]
	[SerializeField] private Equipment[] currentEquipments;

	public delegate void OnEquipmentChanged(Equipment newEquipment, Equipment oldEquipment);
	public OnEquipmentChanged onEquipmentChanged;

	// Private fields.
	private EquipmentSlot[] slots;
	private TextMeshProUGUI[] statTexts;

	private Image portrait;
	private SpriteRenderer playerSprite;
	private PlayerStats playerStats;

	private TextMeshProUGUI toggleText;
	private TextMeshProUGUI title;
	private GameObject statsPanel;
	private Button unequipAllButton;

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
			Debug.LogWarning("More than one instance of Equipment Manager found!!");
			instance = null;
			this.enabled = false;
			return;
		}

		slots = transform.Find("Slots").GetComponentsInChildren<EquipmentSlot>();
		statTexts = transform.Find("Stats Panel/Scroll View/Viewport/Content").GetComponentsInChildren<TextMeshProUGUI>();

		playerSprite = GameObject.FindWithTag("Player").GetComponent<SpriteRenderer>();
		playerStats = playerSprite.GetComponent<PlayerStats>();
		portrait = transform.Find("Portrait/Player Sprite").GetComponent<Image>();

		toggleText = transform.Find("Stats Toggle/Text").GetComponent<TextMeshProUGUI>();
		title = transform.Find("Title/Text").GetComponent<TextMeshProUGUI>();
		statsPanel = transform.Find("Stats Panel").gameObject;
		unequipAllButton = transform.Find("Unequip All Button").GetComponent<Button>();
	}

	private void Start()
	{
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

	// Method for the stats toggle.
	public void OnStatsToggled(bool state)
	{
		statsPanel.SetActive(state);
		unequipAllButton.interactable = !state;

		if (!state)
		{
			title.text = "EQUIPMENTS";
			toggleText.text = "STATS";
		}
		else
		{
			title.text = "STATS";
			toggleText.text = "EQUIPMENTS";
		}
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
