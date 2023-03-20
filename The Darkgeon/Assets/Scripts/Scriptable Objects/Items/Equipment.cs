using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/New Equipment")]
public class Equipment : Item
{
	public enum EquipmentType
	{
		Weapon,
		Shield,
		Head,
		Chestplate,
		Legs,
		Boots,
		Amulet,
		Ring,
		Belt,
		Special,
	}

	[Header("Type")]
	[Space]
	public EquipmentType type;

	[Header("Flat Stat Modifiers")]
	[Space]
	public int armor;
	public int maxHP;
	
	[Space]
	public float regenerateRate;
	public float invincibilityTime;

	[Header("Percentage Stat Modifiers (Actual Percent %)")]
	[Space]
	public float damagePerc;
	public float criticalChancePerc;
	public float criticalDamagePerc;

	[Space]
	public float movementSpeedPerc;
	public float knockbackPerc;
	public float knockbackResistancePerc;

	public override void Use()
	{
		base.Use();
		
		EquipmentManager.instance.Equip(this);
		Inventory.instance.Remove(this, true);
	}

	public override string ToString()
	{
		string str = $"Right Click to equip/unequip.\n" +
				$"Type: {type}.\n\n" +
				$"Rarity: {rarity.title}.\n\n";

		str += damagePerc != 0 ? $"+{damagePerc}% Damage.\n" : "";
		str += criticalChancePerc != 0 ? $"+{criticalChancePerc}% Critical Chance.\n" : "";
		str += criticalDamagePerc != 0 ? $"+{criticalDamagePerc}% Critical Damage.\n" : "";
		str += "\n";
		str += movementSpeedPerc != 0 ? $"+{movementSpeedPerc}% Movement Speed.\n" : "";
		str += knockbackPerc != 0 ? $"+{knockbackPerc}% Knockback.\n" : "";
		str += knockbackResistancePerc != 0 ? $"+{knockbackResistancePerc * 100f}% Knockback Resistance.\n" : "";
		str += "\n";
		str += regenerateRate != 0 ? $"+{regenerateRate} HP/s Regeneration Rate.\n" : "";
		str += invincibilityTime != 0 ? $"+{invincibilityTime} secs Invincibility Time.\n" : "";
		str += "\n";
		str += armor != 0 ? $"+{armor} Armor.\n" : "";
		str += maxHP != 0 ? $"+{maxHP} Max Health.\n" : "";
		str += "\n";
		str += description;

		return str;
	}
}
