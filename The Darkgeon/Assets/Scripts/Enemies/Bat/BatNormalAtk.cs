using UnityEngine;

public class BatNormalAtk : StateMachineBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private BatBehaviour behaviour;
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Transform player;

	private bool dashed;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		behaviour = animator.GetComponent<BatBehaviour>();
		rb2d = animator.GetComponent<Rigidbody2D>();
		player = GameObject.FindWithTag("Player").transform;
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!dashed && stateInfo.normalizedTime > .2f)
		{
			float dashForce = 3.5f;
			Vector2 aimingDir = player.position - animator.transform.position;

			rb2d.AddForce(dashForce * aimingDir.normalized, ForceMode2D.Impulse);
			dashed = true;
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		dashed = false;
		rb2d.SetRotation(0f);
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
