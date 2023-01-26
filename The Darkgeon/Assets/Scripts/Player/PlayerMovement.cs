using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Manages the player's movement inputs and events.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private CharacterController2D controller;
	[SerializeField] private Animator animator;
	[SerializeField] private Rigidbody2D rb2D;

	[Header("Fields")]
	[Space]

	float horizontalMove;
	float dashAllowTime = 0f;

	bool jump = false;
	bool crouch = false;
	bool dash = false;

	private void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		animator = GetComponent<Animator>();
		rb2D = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	private void Update()
	{
		if (CharacterController2D.m_IsDashing || PlayerActions.ceasePlayerInput)
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
			animator.SetBool("IsJumping", true);
		}

		if (InputManager.instance.GetKeyDown(KeybindingActions.Dash) && Time.time > dashAllowTime && !controller.onSlope)
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
		controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, ref jump, ref dash);
	}

	public void OnLanding()
	{
		animator.SetBool("IsJumping", false);
		Debug.Log("Jump: " + jump);
	}

	public void OnCrouching(bool isCrouching)
	{
		animator.SetBool("IsCrouching", isCrouching);
		Debug.Log("Crouching: " + animator.GetBool("IsCrouching"));
	}
}
