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

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		action = animator.GetComponent<PlayerActions>();
		stats = animator.GetComponent<PlayerStats>();
		dustFx = animator.transform.Find("Effects/Sword Dust Effect").GetComponent<ParticleSystem>();
		animator.GetComponent<Rigidbody2D>().velocity = Vector3.zero;

		canCrit = stats.IsCriticalStrike();

		PlayerMovement.canMove = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!dmgDealt && stateInfo.normalizedTime > .4f)
		{
			// Dash attack has a larger range than regular attacks.
			Collider2D[] hitList = Physics2D.OverlapCircleAll(action.atkPoint.position, action.atkRange + .2f, action.enemyLayers);
			float baseDmg = stats.atkDamage.Value * dmgScale;
			float critDmgMul = canCrit ? 1f + stats.criticalDamage.Value / 100f : 1f;

			foreach (Collider2D enemy in hitList)
			{
				enemy.GetComponent<EnemyStat>().TakeDamage(baseDmg, critDmgMul, stats.knockBackVal.Value);
			}

			dmgDealt = true;
		}

		if (stateInfo.normalizedTime > .4f && !dustFx.isPlaying)
			dustFx.Play();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		dmgDealt = false;
		animator.SetBool("IsAttacking", false);
		animator.SetBool("DashAtk", false);  // Disable the bool.
		action.comboDelay += Time.time;
		PlayerActions.isComboDone = true;
		PlayerMovement.canMove = false;  // The player can move again as soon as the animation is completed.
	}
}
