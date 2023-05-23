using UnityEngine;

[CreateAssetMenu(fileName = "New Slowness Debuff", menuName = "Debuffs/Movement Debuffs/Slowness")]
public class Slowness : MovementDebuff
{
	public override void TakeEffect()
	{
		base.TakeEffect();

		duration -= Time.deltaTime;

		if (duration <= 0f || player.currentHP <= 0)
		{
			DebuffManager.instance.RemoveDebuff(this);
			player.m_MoveSpeed.BaseValue *= 1 / (1 - speedReduceFactor);  // Reverse the speed nerf.
			return;
		}

		// The player's movement speed is reduced.
		if (!isSpeedReduced)
		{
			player.m_MoveSpeed.BaseValue *= (1 - speedReduceFactor);
			isSpeedReduced = true;
		}

		// Update the UI.
		durationUI.text = duration.ToString("0");
	}
}