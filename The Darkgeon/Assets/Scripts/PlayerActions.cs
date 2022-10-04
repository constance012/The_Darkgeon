using UnityEngine;

public class PlayerActions : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private Animator animator;

	// Fields.
	[Header("Player Attack")]
	[Space]

	public Transform atkPoint;
	public LayerMask enemyLayers;
	public float atkRange = .5f;

	public static int clickCount = 0;

	public float lastComboTime = 0f;
	public float inputWaitTime = 0f;
	float comboDelay = 0.5f;  // The cooldown between each combo is 0.3 second.

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack"))
			animator.SetBool("DashAtk", false);
		
		// Check if there is enough time for the next combo to begin.
		if (inputWaitTime > 0f)
			inputWaitTime -= Time.deltaTime;

		if (Input.GetMouseButtonDown(0) && Time.time - lastComboTime >= comboDelay)
			Attack();
	}
	
	private void Attack()
	{
		if (inputWaitTime <= 0f)
		{
			bool canDoAtk1 = animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("Run");

			if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
				animator.SetBool("DashAtk", true);

			if (canDoAtk1)
			{
				animator.SetTrigger("Atk1");
			}

			inputWaitTime = 1f;
		}

		else
		{
			animator.SetTrigger("Atk2");
			inputWaitTime = 0f;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (atkPoint == null)
			return;

		Gizmos.DrawWireSphere(atkPoint.position, atkRange);
	}
}
