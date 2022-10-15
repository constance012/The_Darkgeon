using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
	// References.
	[Header("Movement")]
	[Space]

	[SerializeField] private float m_MoveSpeed = 10f;
	[SerializeField] private float m_Acceleration = 7f;
	[SerializeField] private float m_Deceleration = 7f;
	[SerializeField] private float m_VelPower = 0.9f;
	[SerializeField] private float m_FrictionAmount = 0.2f;
	//[Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;   // How much to smooth out the movement
	[Range(0, 1)][SerializeField] private float m_CrouchSpeed = .36f;           // Amount of maxSpeed applied to crouching movement. 1 = 100%

	[Header("Jump, Dash")]
	[Space]

	[SerializeField] private float m_JumpForce = 600f;                          // Amount of force added when the player jumps.
	[SerializeField] private float m_DashForce = 200f;                          // Amount of force added when the player dashes.
	[SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;

	[Header("Checks")]
	[Space]

	[SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
	[SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

	[Header("References")]
	[Space]

	[SerializeField] private Animator m_Animator;
	[SerializeField] private TrailRenderer m_TrailRenderer;

	// Fields.
	private bool m_Grounded;            // Whether or not the player is grounded.
	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded.
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up.

	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	public static bool m_IsDashing;
	private float m_DashingTime = 0.2f;

	private Rigidbody2D m_Rigidbody2D;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		m_Animator = GetComponent<Animator>();
		m_TrailRenderer = GetComponent<TrailRenderer>();
		m_CrouchDisableCollider = GetComponent<BoxCollider2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void FixedUpdate()
	{
		m_Animator.SetBool("Grounded", m_Grounded);

		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);

		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;

				// Landing event only triggers when falling to the ground.
				if (!wasGrounded && m_Rigidbody2D.velocity.y < 0)
					OnLandEvent.Invoke();
			}
		}
	}


	public void Move(float moveInput, bool crouch, ref bool jump, ref bool dash)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch)
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				moveInput *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			}
			else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}

			// If the input is moving the player right and the player is facing left...
			if (moveInput > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (moveInput < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}

			// Calculate the speed at the direction we want to move.
			float targetSpeed = moveInput * m_MoveSpeed;
			
			// Calculate the difference between current velocity and desired velocity.
			float speedDiff = targetSpeed - m_Rigidbody2D.velocity.x;

			// Define the rate to be either accelerate or decelerate depending on moving or stopping situation.
			float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? m_Acceleration : m_Deceleration;

			// Applies the rate to speed difference, then raises to a set power so acceleration increases with higher speed.
			// And multiplies by the sign to reaplly direction.
			float moveForce = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, m_VelPower) * Mathf.Sign(speedDiff);

			// Applies the force to the rigidbody, multiplying by Vector2.right so that it only affects the X axis.
			m_Rigidbody2D.AddForce(moveForce * Vector2.right);
		}
		
		// If the player should jump.
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
			
			jump = false;
			m_Animator.SetBool("IsJumping", false);
		}

		// If the player should dash.
		if (dash)
		{
			StartCoroutine(Dash());
			dash = false;
		}

		// Check if we're grounded and are trying to stop (not pressing movement keys).
		if (m_Grounded && Mathf.Abs(moveInput) < 0.01f)
		{
			// Then set the friction force to the minimum value between the m_FrictionAmount and our velocity.
			float frictionForce = Mathf.Min(Mathf.Abs(m_Rigidbody2D.velocity.x), Mathf.Abs(m_FrictionAmount));

			// Set it to the movement direction.
			frictionForce *= Mathf.Sign(m_Rigidbody2D.velocity.x);

			// Then applies it against the movement direction.
			m_Rigidbody2D.AddForce(-frictionForce * Vector2.right, ForceMode2D.Impulse);
		}
	}


	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;
		transform.Rotate(0f, 180f, 0f);
	}

	private IEnumerator Dash()
	{
		m_IsDashing = true;
		m_Animator.SetTrigger("IsDashing");
		
		float originalGravity = m_Rigidbody2D.gravityScale;
		m_Rigidbody2D.gravityScale = 0f;

		m_TrailRenderer.emitting = true;

		m_Rigidbody2D.velocity = new Vector2(Mathf.Sign(transform.rotation.y) * m_DashForce, 0f);

		yield return new WaitForSeconds(m_DashingTime);

		m_TrailRenderer.emitting = false;
		m_Rigidbody2D.gravityScale = originalGravity;
		m_IsDashing = false;
	}
}