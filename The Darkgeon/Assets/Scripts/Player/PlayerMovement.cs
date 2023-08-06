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
	private float _jumpTime;
	private float _horizontalMove;
	private float _dashAllowTime = 0f;

	private bool _jump = false;
	private bool _crouch = false;
	private bool _dash = false;

	public static bool CanMove { get; set; } = true;
	public static bool IsModifierKeysOccupied { get; set; }

	private void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		animator = transform.GetComponentInChildren<Animator>("Graphic");
		rb2D = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	private void Update()
	{
		if (CharacterController2D.m_IsDashing || !CanMove)
		{
			_horizontalMove = 0f;
			return;
		}

		_horizontalMove = InputManager.instance.GetAxisRaw("Horizontal");
		//Debug.Log(horizontalMove);

		animator.SetBool("IsRunning", Mathf.Abs(_horizontalMove) == 1);
		animator.SetFloat("Speed", rb2D.velocity.magnitude);
		animator.SetFloat("yVelocity", rb2D.velocity.y);

		if (InputManager.instance.GetKeyDown(KeybindingActions.Jump) && animator.GetBool("Grounded"))
		{
			_jump = true;
			_jumpTime = baseJumpTime;
			animator.SetBool("IsJumping", true);
		}

		if (InputManager.instance.GetKey(KeybindingActions.Jump) && animator.GetBool("IsJumping"))
		{
			if (_jumpTime <= 0f)
			{
				OnLanding();
				return;
			}

			_jump = true;
			_jumpTime -= Time.deltaTime;
		}

		if (InputManager.instance.GetKeyUp(KeybindingActions.Jump))
			OnLanding();

		if (IsModifierKeysOccupied)
			return;

		if (InputManager.instance.GetKeyDown(KeybindingActions.Dash) && Time.time > _dashAllowTime)
		{
			_dash = true;
			_dashAllowTime = Time.time + 1f;
		}

		if (InputManager.instance.GetKeyDown(KeybindingActions.Crouch))
			_crouch = true;
		else if (InputManager.instance.GetKeyUp(KeybindingActions.Crouch))
			_crouch = false;
	}

	private void FixedUpdate()
	{
		controller.Move(_horizontalMove, _crouch, _jump, ref _dash);
	}

	/// <summary>
	/// Callback method for the On Landing event in the CharacterController2D.
	/// </summary>
	public void OnLanding()
	{
		animator.SetBool("IsJumping", false);
		_jump = false;
		//Debug.Log("Jump: " + jump, this);
	}

	/// <summary>
	/// Callback method for the On Crouching event in the CharacterController2D.
	/// </summary>
	/// <param name="isCrouching"></param>
	public void OnCrouching(bool isCrouching)
	{
		animator.SetBool("IsCrouching", isCrouching);
		//Debug.Log("Crouching: " + animator.GetBool("IsCrouching"), this);
	}
}
