using UnityEngine;
using CSTGames.CommonEnums;

public class CrabNormalAtk : StateMachineBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private CrabBehaviour behaviour;
	[SerializeField] private EnemyStat stats;

	private bool dmgDealt;
	private float dmgScale = .7f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		behaviour = animator.GetComponent<CrabBehaviour>();
		stats = animator.GetComponent<EnemyStat>();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!dmgDealt && stateInfo.normalizedTime > .5f)
		{
			Collider2D hitObj = Physics2D.OverlapCircle(behaviour.attackPoint.position, behaviour.attackRange, behaviour.whatIsPlayer);

			if (hitObj != null)
				hitObj.GetComponent<PlayerStats>().TakeDamage(stats.atkDamage * dmgScale, stats.knockBackVal, animator.transform, KillSources.Crab);

			dmgDealt = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		dmgDealt = false;
	}
}
