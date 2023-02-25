using UnityEngine;

public class DashAttack : StateMachineBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private PlayerActions action;
	[SerializeField] private PlayerStats stats;
	[SerializeField] private ParticleSystem dustFx;

	[Header("Fields.")]
	[Space]

	private bool dmgDealt, canCrit;
	private float dmgScale = 1.1f;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		action = animator.GetComponent<PlayerActions>();
		stats = animator.GetComponent<PlayerStats>();
		dustFx = animator.transform.Find("Sword Dust Effect").GetComponent<ParticleSystem>();
		animator.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		FindObjectOfType<AudioManager>().Play("Dash Attack " + Random.Range(1, 3));

		canCrit = stats.IsCriticalStrike();

		PlayerMovement.ceaseKeyboardInput = true;
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!dmgDealt && stateInfo.normalizedTime > .4f)
		{
			// Dash attack has a larger range than regular attacks.
			Collider2D[] hitList = Physics2D.OverlapCircleAll(action.atkPoint.position, action.atkRange + .2f, action.enemyLayers);
			float baseDmg = stats.atkDamage * dmgScale;
			float critDmg = canCrit ? 1f + stats.criticalDamage / 100f : 1f;

			foreach (Collider2D enemy in hitList)
			{
				enemy.GetComponent<EnemyStat>().TakeDamage(baseDmg, critDmg, stats.knockBackVal);
			}

			dmgDealt = true;
		}

		if (stateInfo.normalizedTime > .4f && !dustFx.isPlaying)
			dustFx.Play();
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		dmgDealt = false;
		animator.SetBool("IsAttacking", false);
		animator.SetBool("DashAtk", false);  // Disable the bool.
		action.comboDelay += Time.time;
		PlayerActions.isComboDone = true;
		PlayerActions.canFaceTowardsCursor = false;
		PlayerMovement.ceaseKeyboardInput = false;  // The player can move again as soon as the animation is completed.
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
