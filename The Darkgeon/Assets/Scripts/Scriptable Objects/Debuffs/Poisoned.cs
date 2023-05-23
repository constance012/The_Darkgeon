using UnityEngine;

[CreateAssetMenu(fileName = "New Poisoned Debuff", menuName = "Debuffs/Health Debuffs/Poisoned")]
public class Poisoned : HealthDebuff
{
	public override void TakeEffect()
	{
		base.TakeEffect();

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
			player.currentHP = Mathf.Clamp(player.currentHP, 0f, player.maxHP.Value);

			player.hpBar.SetCurrentHealth(player.currentHP);

			DamageText.Generate(player.dmgTextPrefab, player.dmgTextLoc.position, healthLossRate.ToString());

			hpLossDelay = baseHpLossDelay;
		}
		// Update the UI.
		durationUI.text = duration.ToString("0");
	}
}
