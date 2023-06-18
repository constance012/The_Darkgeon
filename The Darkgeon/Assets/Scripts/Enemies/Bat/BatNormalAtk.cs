using UnityEngine;

public class BatNormalAtk : StateMachineBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private BatBehaviour behaviour;
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Transform player;

	private bool dashed;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		behaviour = animator.GetComponent<BatBehaviour>();
		rb2d = animator.GetComponent<Rigidbody2D>();
		player = GameObject.FindWithTag("Player").transform;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!dashed && stateInfo.normalizedTime > .2f)
		{
			float dashForce = 3.5f;
			Vector2 aimingDir = player.position - animator.transform.position;

			rb2d.AddForce(dashForce * aimingDir.normalized, ForceMode2D.Impulse);
			dashed = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		dashed = false;
		rb2d.SetRotation(0f);
	}
}
