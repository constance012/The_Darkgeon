using UnityEngine;

[CreateAssetMenu(fileName = "New Bleeding Debuff", menuName = "Debuffs/Health Debuffs/Bleeding")]
public class Bleeding : HealthDebuff
{
	public override void TakeEffect()
	{
		base.TakeEffect();

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
			player.currentHP = Mathf.Clamp(player.currentHP, 0f, player.maxHP.Value);

			player.hpBar.SetCurrentHealth(player.currentHP);

			DamageText.Generate(player.dmgTextPrefab, player.dmgTextLoc.position, healthLossRate.ToString());

			hpLossDelay = baseHpLossDelay;
		}

		// Update the UI.
		durationUI.text = duration.ToString("0");
	}
}
