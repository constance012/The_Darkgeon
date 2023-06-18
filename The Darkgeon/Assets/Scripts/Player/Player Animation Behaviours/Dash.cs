using UnityEngine;

public class Dash : StateMachineBehaviour
{
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (Input.GetMouseButtonDown(0))
			animator.SetBool("DashAtk", true);
	}
}
