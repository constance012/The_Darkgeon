using System.Reflection;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "New Health Debuff", menuName = "Debuffs/Health Debuff")]
public class HealthDebuff : Debuff
{
	[Header("Health Related Debuff")]
	[Space]
	public int healthLossRate;
	
	[Space]
	public float baseHpLossDelay;
	public float hpLossDelay;

	[Space]
	public bool allowRegenerate;

	public override void TakeEffect()
	{
		base.TakeEffect();

		MethodInfo methodName = this.GetType().GetMethod(debuffName, BindingFlags.NonPublic | BindingFlags.Instance);
		methodName.Invoke(this, null);
	}

	private void Bleeding()
	{
		// Clone the particle system if the player doesn't contain its game object already and play it once.
		if (visualEffect != null && player.transform.Find(visualEffect.name) == null)
		{
			ParticleSystem currentEffect = Instantiate(visualEffect, player.transform).GetComponent<ParticleSystem>();
			currentEffect.name = visualEffect.name;
			currentEffect.Play();
		}

		duration -= Time.deltaTime;
		hpLossDelay -= Time.deltaTime;

		if (duration <= 0f || player.currentHP <= 0)
		{
			Destroy(player.transform.Find(visualEffect.name).gameObject);

			DebuffManager.instance.RemoveDebuff(this);
			player.canRegen = true;
			return;
		}

		player.canRegen = allowRegenerate;

		// If the player moves, she'll lose health.
		if (playerAnim.GetFloat("Speed") > .01f && player.currentHP > 0 && hpLossDelay <= 0f)
		{
			player.currentHP -= healthLossRate;
			player.currentHP = Mathf.Clamp(player.currentHP, 0, player.maxHP);

			player.hpBar.SetCurrentHealth(player.currentHP);

			DamageText.Generate(player.dmgTextPrefab, player.dmgTextLoc.position, healthLossRate.ToString());

			hpLossDelay = baseHpLossDelay;
		}

		// Update the UI.
		durationUI.text = duration.ToString("0");
	}

	private void Poisoned()
	{
		duration -= Time.deltaTime;
		hpLossDelay -= Time.deltaTime;

		if (duration <= 0f || player.currentHP <= 0)
		{
			DebuffManager.instance.RemoveDebuff(this);
			player.canRegen = true;
			return;
		}

		player.canRegen = allowRegenerate;

		// The player loses health over time.
		if (player.currentHP > 0 && hpLossDelay <= 0f)
		{
			player.currentHP -= healthLossRate;
			player.currentHP = Mathf.Clamp(player.currentHP, 0, player.maxHP);

			player.hpBar.SetCurrentHealth(player.currentHP);

			DamageText.Generate(player.dmgTextPrefab, player.dmgTextLoc.position, healthLossRate.ToString());

			hpLossDelay = baseHpLossDelay;
		}
		// Update the UI.
		durationUI.text = duration.ToString("0");
	}
}