using UnityEngine;

public class SpikedSlimeNormalAtk : StateMachineBehaviour
{
    [Header("Reference")]
	[Space]
	[SerializeField] private SpikedSlimeBehaviour behaviour;
	[SerializeField] private Rigidbody2D rb2d;

	private bool hopped;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		behaviour = animator.GetComponent<SpikedSlimeBehaviour>();
		rb2d = animator.GetComponent<Rigidbody2D>();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!hopped && stateInfo.normalizedTime > .27f)
		{
			float hopForce = behaviour.facingRight ? 5f : -5f;
			rb2d.AddForce(hopForce * Vector2.right, ForceMode2D.Impulse);
			hopped = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		hopped = false;
		behaviour.atkAnimDone = true;
	}
}
