using UnityEngine;

public class Attack1 : StateMachineBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private PlayerActions action;
	[SerializeField] private PlayerStats stats;

	[Header("Fields.")]
	[Space]

	bool dmgDealt, soundPlayed;
	bool isAtk2Triggered;
	float dmgMultiplier = .8f;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		action = animator.GetComponent<PlayerActions>();
		stats = animator.GetComponent<PlayerStats>();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!soundPlayed && stateInfo.normalizedTime > .5f)
		{
			FindObjectOfType<AudioManager>().Play("Normal Attack 1");
			soundPlayed = true;
		}

		if (!dmgDealt && stateInfo.normalizedTime > .7f)
		{
			Collider2D[] hitList = Physics2D.OverlapCircleAll(action.atkPoint.position, action.atkRange, action.enemyLayers);

			foreach (Collider2D enemy in hitList)
			{
				enemy.GetComponent<EnemyStat>().TakeDamage(stats.atkDamage * dmgMultiplier, stats.knockBackVal);
			}

			dmgDealt = true;
		}

		if (Input.GetMouseButtonDown(0))
		{
			animator.SetTrigger("Atk2");
			isAtk2Triggered = true;
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		// If the 2nd attack is not get triggered.
		if (!isAtk2Triggered)
		{
			action.isComboDone = true;
			action.lastComboTime = Time.time;
			animator.GetComponent<PlayerMovement>().enabled = true;  // The player can move again as soon as the animation is completed.
		}

		dmgDealt = isAtk2Triggered = soundPlayed = false;
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
