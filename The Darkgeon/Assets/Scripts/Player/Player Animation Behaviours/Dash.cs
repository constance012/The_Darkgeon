using UnityEngine;
using CSTGames.CommonEnums;

public class Dash : StateMachineBehaviour
{
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (InputManager.instance.GetKeyDown(KeybindingActions.PrimaryAttack))
			animator.Play("Dash-Attack");
	}
}
