using UnityEngine;

/// <summary>
/// Manages the player's actions, like attacks or abilities.
/// </summary>
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
	public static bool ceasePlayerInput { get; set; }
	float comboDelay = 0.5f;  // The cooldown between each combo is 0.3 second.

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		// Check if there is enough time for the next combo to begin.
		if (Input.GetMouseButtonDown(0) && animator.GetBool("Grounded") && isComboDone)
			Attack();
	}
	
	private void Attack()
	{
		if (Time.time - lastComboTime >= comboDelay && !GameManager.isPause)
		{
			// Make sure the player can not move.
			ceasePlayerInput = true;
			animator.SetBool("IsAttacking", true);
			animator.SetBool("IsRunning", false);
			animator.SetFloat("Speed", 0f);

			// Only attack during dashing.
			if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
				animator.SetBool("DashAtk", true);

			// Only attacking if grounded and not dashing.
			else
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
