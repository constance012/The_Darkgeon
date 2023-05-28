using UnityEngine;

public class Attack1 : StateMachineBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private PlayerActions action;
	[SerializeField] private PlayerStats stats;

	[Header("Fields.")]
	[Space]

	private bool dmgDealt;
	private bool isAtk2Triggered;
	private bool canCrit;
	private float dmgScale = .8f;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		action = animator.GetComponent<PlayerActions>();
		stats = animator.GetComponent<PlayerStats>();

		canCrit = stats.IsCriticalStrike();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!dmgDealt && stateInfo.normalizedTime > .7f)
		{
			Collider2D[] hitList = Physics2D.OverlapCircleAll(action.atkPoint.position, action.atkRange, action.enemyLayers);
			float baseDmg = stats.atkDamage.Value * dmgScale;
			float critDmgMul = canCrit ? 1f + stats.criticalDamage.Value / 100f : 1f;
			
			foreach (Collider2D enemy in hitList)
			{
				enemy.GetComponent<EnemyStat>().TakeDamage(baseDmg, critDmgMul, stats.knockBackVal.Value);
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
			animator.SetBool("IsAttacking", false);
			action.comboDelay += Time.time;
			PlayerActions.isComboDone = true;
		}

		dmgDealt = isAtk2Triggered = false;
	}
}
