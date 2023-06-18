using UnityEngine;
using CSTGames.CommonEnums;

public class CrabDoubleAtk : StateMachineBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private CrabBehaviour behaviour;
	[SerializeField] private EnemyStat stats;

	private bool firstHitLanded;
	private bool secondHitLanded;
	private float dmgScale = .85f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		behaviour = animator.GetComponent<CrabBehaviour>();
		stats = animator.GetComponent<EnemyStat>();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!secondHitLanded)
		{
			Collider2D hitObj = Physics2D.OverlapCircle(behaviour.attackPoint.position, behaviour.attackRange, behaviour.whatIsPlayer);

			if (hitObj == null)
				return;

			// First hit is weaker than second hit and doesn't knock the player back.
			if (stateInfo.normalizedTime > .3f && !firstHitLanded)
			{
				hitObj.GetComponent<PlayerStats>().TakeDamage(stats.atkDamage * (dmgScale - .25f), 0, null, KillSources.Crab);
				firstHitLanded = true;
			}

			// Second hit is stronger than first hit and knocks the player back.
			if (stateInfo.normalizedTime > .7f)
			{
				hitObj.GetComponent<PlayerStats>().TakeDamage(stats.atkDamage * dmgScale, stats.knockBackVal , animator.transform, KillSources.Crab);
				secondHitLanded = true;
			}

		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		firstHitLanded = secondHitLanded = false;
	}
}
