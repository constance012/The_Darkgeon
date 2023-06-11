using System;
using UnityEngine;

[Serializable]
public class Stat
{
	private enum StatType { Flat, Percentage }
	[SerializeField] private StatType modifierType;

	[field: SerializeField]
	public float BaseValue { get; set; }

	public float Value { get => GetValue(); }

	// Private fields.
	private float modifier = 0f;

	private float GetValue()
	{
		float finalValue = BaseValue;

		if (modifierType == StatType.Flat)
			return finalValue += modifier;
		else
			return finalValue *= (1 + modifier / 100f);
	}

	public void AddModifier(float target)
	{
		if (target != 0f)
			modifier += target;

		modifier = Mathf.Clamp(modifier, 0f, modifier);
	}

	public void RemoveModifier(float target)
	{
		if (target != 0f)
			modifier -= target;

		modifier = Mathf.Clamp(modifier, 0f, modifier);
	}
}
