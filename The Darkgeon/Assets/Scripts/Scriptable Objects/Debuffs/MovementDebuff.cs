using System.Reflection;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "New Movement Debuff", menuName = "Debuffs/Movement Debuff")]
public class MovementDebuff : Debuff
{
	[Header("Movement Related Debuff")]
	[Space]
	[Range(.2f, .9f)] public float speedReduceFactor;
	[HideInInspector] public bool isSpeedReduced;

	public override void TakeEffect()
	{
		base.TakeEffect();

		MethodInfo methodName = this.GetType().GetMethod(debuffName, BindingFlags.NonPublic | BindingFlags.Instance);
		methodName.Invoke(this, null);
	}

	private void Slowness()
	{
		duration -= Time.deltaTime;

		if (duration <= 0f || player.currentHP <= 0)
		{
			FindObjectOfType<DebuffManager>().RemoveDebuff(this);
			controller.m_MoveSpeed *= 1 / (1 - speedReduceFactor);  // Reverse the speed nerf.
			return;
		}

		// The player's movement speed is reduced.
		if (!isSpeedReduced)
		{
			controller.m_MoveSpeed *= (1 - speedReduceFactor);
			isSpeedReduced = true;
		}

		// Update the UI.
		durationUI.text = duration.ToString("0");
	}
}