using UnityEngine;

public class Attack2 : StateMachineBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private PlayerActions action;

	[Header("Fields.")]
	[Space]

	bool dmgDealt;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		action = animator.GetComponent<PlayerActions>();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!dmgDealt && stateInfo.normalizedTime > .5f)
		{
			Collider2D[] hitList = Physics2D.OverlapCircleAll(action.atkPoint.position, action.atkRange, action.enemyLayers);

			foreach (Collider2D enemy in hitList)
			{
				enemy.GetComponent<EnemyStat>().TakeDamage(12);
			}

			dmgDealt = true;
		}

		if (action.inputWaitTime != 0f)
			action.inputWaitTime = 0f;
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		dmgDealt = false;
		action.lastComboTime = Time.time;
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