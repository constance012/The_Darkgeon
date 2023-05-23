using UnityEngine;

public class MovementDebuff : Debuff
{
	[Header("Movement Related Debuff")]
	[Space]
	[Range(.2f, .9f)] public float speedReduceFactor;
	[HideInInspector] public bool isSpeedReduced;

	public override void TakeEffect()
	{
		base.TakeEffect();
	}
}