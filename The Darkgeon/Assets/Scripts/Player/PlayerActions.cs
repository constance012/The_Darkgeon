using UnityEngine;
using CSTGames.CommonEnums;

/// <summary>
/// Manages the player's actions, like attacks or abilities.
/// </summary>
public class PlayerActions : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	private Animator animator;
	private CharacterController2D controller;

	// Fields.
	[Header("Player Attack")]
	[Space]

	public Transform atkPoint;
	public LayerMask enemyLayers;
	public float atkRange = .5f;
	public float m_ComboDelay = 0.5f;  // The cooldown between each combo is 0.3 second.

	[HideInInspector] public float comboDelay;

	public static bool IsAttacking { get; set; }
	public static bool IsComboDone { get; set; } = true;
	public static bool CanAttack { get; set; } = true;

	private bool _canFaceTowardsCursor;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		controller = GetComponentInParent<CharacterController2D>();
	}

	private void Update()
	{
		if (!CanAttack)
		{
			//Debug.Log("Can not attack");
			return;
		}

		_canFaceTowardsCursor = Mathf.Round(animator.GetFloat("Speed")) == 0f;
		
		KeyCode mouse0 = InputManager.instance.GetKeyForAction(KeybindingActions.PrimaryAttack);
		//Debug.Log(mouse0);

		// Check if there is enough time for the next combo to begin.
		if (Input.GetKeyDown(mouse0) && !GameManager.IsPause && IsComboDone)
			Attack();

		// Facing the player in the direction of the mouse when certain conditions are matched.
		if ((!animator.GetBool("IsRunning") || _canFaceTowardsCursor) && !GameManager.IsPause)
		{
			Vector2 facingDir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
			float angle = Mathf.Abs(Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg);

			if (angle <= 90 && !controller.m_FacingRight)
				controller.Flip();
			else if (angle > 90 && controller.m_FacingRight)
				controller.Flip();
		}
	}
	
	private void Attack()
	{
		if (Time.time > comboDelay && !animator.GetBool("IsCrouching") && animator.GetBool("Grounded"))
		{
			IsAttacking = true;
			
			// Facing the player in the aiming direction before attack.
			Vector2 aimingDir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);

			if ((aimingDir.x > 0f && !controller.m_FacingRight) || (aimingDir.x < 0f && controller.m_FacingRight))
				controller.Flip();

			comboDelay = m_ComboDelay;

			animator.SetBool("IsRunning", false);

			// Perform normal attack if is not dashing.
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
				animator.Play("Attack 1");
			
			IsComboDone = false;
		}
	}

	public void CancelAttacking()
	{
		IsAttacking = false;
		animator.ResetTrigger("Atk2");
	}

	private void OnDrawGizmosSelected()
	{
		if (atkPoint == null)
			return;

		Gizmos.DrawWireSphere(atkPoint.position, atkRange);
	}
}
