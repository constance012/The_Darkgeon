using UnityEngine;

public class RatNormalAtk : StateMachineBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private RatBehaviour behaviour;
	[SerializeField] private EnemyStat stats;
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Debuff bleeding;
	[SerializeField] private Debuff slowness;

	bool dmgDealt, hopped;
	float dmgMultiplier = .9f;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		behaviour = animator.GetComponent<RatBehaviour>();
		stats = animator.GetComponent<EnemyStat>();
		rb2d = animator.GetComponent<Rigidbody2D>();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!dmgDealt)
		{
			if (!hopped && stateInfo.normalizedTime > .3f)
			{
				float hopForce = behaviour.facingRight ? 3.5f : -3.5f;
				rb2d.AddForce(hopForce * Vector2.right, ForceMode2D.Impulse);
				hopped = true;
			}

			if (stateInfo.normalizedTime > .6f) {
				Collider2D hitObj = Physics2D.OverlapCircle(behaviour.attackPoint.position, behaviour.attackRange, behaviour.whatIsPlayer);
				int inflictChange = Random.Range(1, 6);

				if (hitObj != null)
				{
					hitObj.GetComponent<PlayerStats>().TakeDamage(stats.atkDamage * dmgMultiplier, stats.knockBackVal, animator.transform, KillSources.Rat);

					if (inflictChange == 1)
					{
						FindObjectOfType<DebuffManager>().ApplyDebuff(Instantiate(bleeding));
						FindObjectOfType<DebuffManager>().ApplyDebuff(Instantiate(slowness));
					}
				}

				dmgDealt = true;
			}
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		dmgDealt = hopped = false;
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
