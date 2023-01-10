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
	float dmgScale = .9f;

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
		if (!hopped && stateInfo.normalizedTime > .27f)
		{
			float hopForce = behaviour.facingRight ? 3.5f : -3.5f;
			rb2d.AddForce(hopForce * Vector2.right, ForceMode2D.Impulse);
			hopped = true;
		}

		if (!dmgDealt && stateInfo.normalizedTime > .6f) {
			Collider2D hitObj = Physics2D.OverlapCircle(behaviour.attackPoint.position, behaviour.attackRange, behaviour.whatIsPlayer);
			int inflictChance = Random.Range(1, 6);

			if (hitObj != null)
			{
				hitObj.GetComponent<PlayerStats>().TakeDamage(stats.atkDamage * dmgScale, stats.knockBackVal, animator.transform, KillSources.Rat);

				// Apply debuffs when has a chance.
				if (inflictChance == 1)
				{
					FindObjectOfType<DebuffManager>().ApplyDebuff(Instantiate(bleeding));
					FindObjectOfType<DebuffManager>().ApplyDebuff(Instantiate(slowness));
				}
			}

			dmgDealt = true;
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		dmgDealt = hopped = false;
		behaviour.atkAnimDone = true;
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
