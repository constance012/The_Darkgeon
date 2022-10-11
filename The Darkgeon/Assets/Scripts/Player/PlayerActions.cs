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

	[HideInInspector] public float lastComboTime;
	[HideInInspector] public float inputWaitTime;
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
		GetComponent<PlayerMovement>().enabled = false;
		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		animator.SetBool("IsRunning", false);
		animator.SetFloat("Speed", 0f);

		if (inputWaitTime <= 0f)
		{
			if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
				animator.SetBool("DashAtk", true);

			if (animator.GetBool("Grounded"))
				animator.SetTrigger("Atk1");

			inputWaitTime = 2f;
		}

		else if(inputWaitTime > 0f && animator.GetBool("Grounded"))
		{
			animator.SetTrigger("Atk2");
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (atkPoint == null)
			return;

		Gizmos.DrawWireSphere(atkPoint.position, atkRange);
	}
}
