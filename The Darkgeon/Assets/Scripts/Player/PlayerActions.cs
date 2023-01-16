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
	[SerializeField] private Rigidbody2D rb2d;

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
	public static bool canFaceTowardsCursor { get; set; }

	private void Awake()
	{
		animator = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
		rb2d = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		// Check if there is enough time for the next combo to begin.
		if (InputManager.instance.GetKeyDown(KeybindingActions.PrimaryAttack) && animator.GetBool("Grounded") && isComboDone)
			Attack();

		// Facing the player in the direction of the mouse when certain conditions are matched.
		if ((rb2d.velocity == Vector2.zero || canFaceTowardsCursor) && !GameManager.isPause)
		{
			Vector2 aimingDir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
			float angle = Mathf.Abs(Mathf.Atan2(aimingDir.y, aimingDir.x) * Mathf.Rad2Deg);

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

			// Perform normal attack if is not dashing.
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
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
