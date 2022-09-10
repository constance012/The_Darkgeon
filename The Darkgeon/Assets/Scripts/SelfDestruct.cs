using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	public Animator animator;

	private void Update()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
			Destroy(gameObject);
	}
}
