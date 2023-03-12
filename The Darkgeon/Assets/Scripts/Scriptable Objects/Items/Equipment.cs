using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Item
{
	[Header("Type")]
	[Space]
	public EquipmentType type;

	[Header("Flat Stat Modifiers")]
	[Space]
	public int armor;
	public int maxHP;

	[Header("Percentage Stat Modifiers")]
	[Space]
	public float damage;
	public float criticalChance;
	public float criticalDamage;

	[Space]
	public float movementSpeed;
	public float knockback;
	public float knockbackResistance;

	[Space]
	public float regenerateDelay;
	public float invincibilityTime;

	public override void Use()
	{
		base.Use();

		// Equip code goes here.
	}
}

public enum EquipmentType
{
	Weapon,
	Shield,
	Head,
	Chestplate,
	Leggings,
	Boots,
	Ring,
	Amulet,
	Special,
}
