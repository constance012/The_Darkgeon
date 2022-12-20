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
	[SerializeField] private CharacterController2D controller;

	// Fields.
	[Header("Player Attack")]
	[Space]

	public Transform atkPoint;
	public LayerMask enemyLayers;
	public float atkRange = .5f;
	public float m_ComboDelay = 0.5f;  // The cooldown between each combo is 0.3 second.

	[HideInInspector] public float comboDelay;

	public static bool ceasePlayerInput { get; set; }
	public static bool isComboDone { get; set; } = true;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
	}

	private void Update()
	{
		// Check if there is enough time for the next combo to begin.
		if (Input.GetMouseButtonDown(0) && animator.GetBool("Grounded") && isComboDone)
			Attack();

		// Facing the player in the direction of the mouse when she is not moving.
		if (!animator.GetBool("IsRunning"))
		{
			Vector2 aimingDir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
			float angle = Mathf.Abs(Mathf.Atan2(aimingDir.y, aimingDir.x) * Mathf.Rad2Deg);
			Debug.Log("Angle:" + angle);

			if (angle <= 90 && !controller.m_FacingRight)
				controller.Flip();
			else if (angle > 90 && controller.m_FacingRight)
				controller.Flip();
		}
	}
	
	private void Attack()
	{
		if (Time.time > comboDelay && !GameManager.isPause)
		{
			comboDelay = m_ComboDelay;

			animator.SetBool("IsAttacking", true);
			animator.SetBool("IsRunning", false);

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
