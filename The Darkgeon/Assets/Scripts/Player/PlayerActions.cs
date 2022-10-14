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

	[HideInInspector] public bool isComboDone = true;
	[HideInInspector] public float lastComboTime;
	float comboDelay = 0.5f;  // The cooldown between each combo is 0.3 second.

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack"))
			animator.SetBool("DashAtk", false);

		Debug.Log(isComboDone);
		// Check if there is enough time for the next combo to begin.
		if (Input.GetMouseButtonDown(0) && isComboDone)
			Attack();
	}
	
	private void Attack()
	{
		if (Time.time - lastComboTime >= comboDelay)
		{
			// Make sure the player can not move.
			GetComponent<PlayerMovement>().enabled = false;
			GetComponent<Rigidbody2D>().velocity = Vector3.zero;
			animator.SetBool("IsRunning", false);
			animator.SetFloat("Speed", 0f);

			if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
				animator.SetBool("DashAtk", true);

			// Only attacking if grounded.
			if (animator.GetBool("Grounded"))
				animator.SetTrigger("Atk1");

			isComboDone = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (atkPoint == null)
			return;

		Gizmos.DrawWireSphere(atkPoint.position, atkRange);
	}
}
