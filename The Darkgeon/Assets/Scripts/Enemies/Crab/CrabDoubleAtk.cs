using UnityEngine;
using CSTGames.CommonEnums;

public class CrabDoubleAtk : StateMachineBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private CrabBehaviour behaviour;
	[SerializeField] private EnemyStat stats;

	bool firstHitLanded;
	bool secondHitLanded;
	float dmgScale = .85f;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		behaviour = animator.GetComponent<CrabBehaviour>();
		stats = animator.GetComponent<EnemyStat>();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
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

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		firstHitLanded = secondHitLanded = false;
	}

	// OnStateMove is called right after Animator.OnAnimatorMove()
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that processes and affects root motion
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK()
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that sets up animation IK (inverse kinematics)
	//}
}
