using UnityEngine;

[CreateAssetMenu(fileName = "New Debuff")]
public class Debuff : ScriptableObject
{
	public new string name;
	public string description;
	public float baseDuration;
	public float duration;
	public bool isActive;

	public Sprite icon;

	[Header("Health Related Debuff")]
	[Space]
	public int healthLossRate;
	public float healthLossDelay;
	public bool canRegenerate;

	[Header("Movement Related Debuff")]
	[Space]
	[Range(.2f, .9f)] public float speedReduceFactor;
	public bool isSpeedReduced;

	//public void Bleeding(DebuffManager manager, PlayerStats player, Animator playerAnim)
	//{
	//	duration -= Time.deltaTime;
	//	healthLossDelay -= Time.deltaTime;

	//	if (duration <= 0f)
	//	{
	//		manager.RemoveDebuff(name);
	//		player.canRegen = true;
	//		return;
	//	}

	//	player.canRegen = canRegenerate;


	//	// If the player moves, she'll lose health.
	//	if (playerAnim.GetFloat("Speed") > 0.01f && player.currentHP > 0 && healthLossDelay < 0f)
	//	{
	//		player.currentHP -= healthLossRate;
	//		player.currentHP = Mathf.Clamp(player.currentHP, 0, player.maxHP);
	//		player.hpBar.SetCurrentHealth(player.currentHP);

	//		healthLossDelay = manager.healthLossDelayOrigin;
	//	}
	//}
}
