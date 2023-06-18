using UnityEngine;

/// <summary>
/// Contains methods for playing audio through player's animation events.
/// </summary>
public class PlayerAudio : MonoBehaviour
{
	private void NormalAttack1Sound() => AudioManager.instance.Play("Normal Attack 1");

	private void NormalAttack2Sound() => AudioManager.instance.Play("Normal Attack 2");

	private void DashAttackSounds() => AudioManager.instance.Play("Dash Attack");

	private void FootstepSounds() => AudioManager.instance.Play("Footstep");
}
