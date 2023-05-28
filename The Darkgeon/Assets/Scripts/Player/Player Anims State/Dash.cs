using UnityEngine;

public class Dash : StateMachineBehaviour
{
	private bool performDashAtk;

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (Input.GetMouseButtonDown(0))
			animator.SetBool("DashAtk", true);

		if (stateInfo.normalizedTime > .5f)
			PlayerActions.canFaceTowardsCursor = true;
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!performDashAtk)
			PlayerActions.canFaceTowardsCursor = false;

		performDashAtk = false;
	}
}
