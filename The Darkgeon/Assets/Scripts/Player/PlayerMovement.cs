using UnityEngine;
using CSTGames.CommonEnums;

/// <summary>
/// Manages the player's movement inputs and events.
/// </summary>
[RequireComponent(typeof(CharacterController2D))]
public class PlayerMovement : MonoBehaviour
{
	[Header("References")]
	[Space]
	private CharacterController2D controller;
	private Animator animator;
	private Rigidbody2D rb2D;

	[Header("Fields")]
	[Space]
	[Tooltip("The time (s) required to achieve a full height jump.")]
	public float baseJumpTime;

	// Private fields.
	private float jumpTime;
	private float horizontalMove;
	private float dashAllowTime = 0f;

	private bool jump = false;
	private bool crouch = false;
	private bool dash = false;

	public static bool canMove { get; set; } = true;
	public static bool isModifierKeysOccupied { get; set; }

	private void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		animator = transform.GetComponentInChildren<Animator>("Graphic");
		rb2D = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	private void Update()
	{
		if (CharacterController2D.m_IsDashing || !canMove)
		{
			horizontalMove = 0f;
			return;
		}

		horizontalMove = InputManager.instance.GetAxisRaw("Horizontal");
		//Debug.Log(horizontalMove);

		animator.SetBool("IsRunning", Mathf.Abs(horizontalMove) == 1);
		animator.SetFloat("Speed", rb2D.velocity.magnitude);
		animator.SetFloat("yVelocity", rb2D.velocity.y);

		if (InputManager.instance.GetKeyDown(KeybindingActions.Jump) && animator.GetBool("Grounded"))
		{
			jump = true;
			jumpTime = baseJumpTime;
			animator.SetBool("IsJumping", true);
		}

		if (InputManager.instance.GetKey(KeybindingActions.Jump) && animator.GetBool("IsJumping"))
		{
			if (jumpTime <= 0f)
			{
				OnLanding();
				return;
			}

			jump = true;
			jumpTime -= Time.deltaTime;
		}

		if (InputManager.instance.GetKeyUp(KeybindingActions.Jump))
			OnLanding();

		if (isModifierKeysOccupied)
			return;

		if (InputManager.instance.GetKeyDown(KeybindingActions.Dash) && Time.time > dashAllowTime)
		{
			dash = true;
			dashAllowTime = Time.time + 1f;
		}

		if (InputManager.instance.GetKeyDown(KeybindingActions.Crouch))
			crouch = true;
		else if (InputManager.instance.GetKeyUp(KeybindingActions.Crouch))
			crouch = false;
	}

	private void FixedUpdate()
	{
		controller.Move(horizontalMove, crouch, jump, ref dash);
	}

	public void OnLanding()
	{
		animator.SetBool("IsJumping", false);
		jump = false;
		//Debug.Log("Jump: " + jump, this);
	}

	public void OnCrouching(bool isCrouching)
	{
		animator.SetBool("IsCrouching", isCrouching);
		//Debug.Log("Crouching: " + animator.GetBool("IsCrouching"), this);
	}
}
