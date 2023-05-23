using UnityEngine;

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
	}
}