using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A character controller for 2D game's characters, originally written by Brackeys.
/// </summary>
public class CharacterController2D : MonoBehaviour
{
	// References.
	[Header("Movement")]
	[Space]
	[SerializeField] [Range(.1f, 10f)] private float m_Acceleration = 7f;
	[SerializeField] [Range(.1f, 10f)] private float m_Deceleration = 7f;
	[SerializeField] private float m_VelPower = .9f;
	[SerializeField] private float m_FrictionAmount = .2f;
	//[Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;   // How much to smooth out the movement
	[Range(0, 1)][SerializeField] private float m_CrouchSpeed = .36f;           // Amount of maxSpeed applied to crouching movement. 1 = 100%

	[Header("Jump, Dash, Slopes Handle")]
	[Space]
	[SerializeField] private float m_JumpForce = 600f;                          // Amount of force added when the player jumps.
	[SerializeField] private float m_DashForce = 200f;                          // Amount of force added when the player dashes.
	[SerializeField] private bool m_AirControl;                         // Whether or not a player can steer while jumping;
	[SerializeField] private float m_SlopeCheckDistance;
	[SerializeField] [Range(10f, 89f)] private float m_MaxSlopeAngle;

	[Header("Checks")]
	[Space]
	[SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
	[SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

	[Header("References")]
	[Space]
	[SerializeField] private PlayerStats m_Stats;
	[SerializeField] private Animator m_Animator;
	[SerializeField] private TrailRenderer m_TrailRenderer;

	[Header("Materials")]
	[Space]
	[SerializeField] private PhysicsMaterial2D slippery;
	[SerializeField] private PhysicsMaterial2D grippy;

	[Header("Effects")]
	[Space]
	[SerializeField] private ParticleSystem runningDust;
	[SerializeField] private ParticleSystem jumpingDust;

	[Header("Events")]
	[Space]
	public UnityEvent OnLandEvent;

	[Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;

	// Private fields.
	[HideInInspector] public bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private bool m_wasCrouching;
	private bool m_Grounded;            // Whether or not the player is grounded.
	public static bool m_IsDashing { get; private set; }
	
	private bool canWalkOnSlope;
	public bool onSlope { get; private set; }

	private const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded.
	private const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up.
	private float m_DashingTime = .2f;

	private float slopeDownAngle;  // Down because the raycast is shooting down.
	private float slopeDownAngleOld;
	private float slopeSideAngle;

	private Rigidbody2D m_Rigidbody2D;
	private Vector2 slopeNormalPerp;  // A vector to store the perpendicular normal vector of the RaycastHit's object.

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		m_Stats = GetComponent<PlayerStats>();
		m_Animator = GetComponent<Animator>();
		m_TrailRenderer = GetComponent<TrailRenderer>();
		m_CrouchDisableCollider = GetComponent<BoxCollider2D>();

		runningDust = transform.Find("Running Dust").GetComponent<ParticleSystem>();
		jumpingDust = transform.Find("Jumping Dust").GetComponent<ParticleSystem>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void FixedUpdate()
	{
		CheckGround();
		CheckSlope();
	}

	public void Move(float moveInput, bool crouch, ref bool jump, ref bool dash)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
				crouch = true;
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
			if ((moveInput > 0 && !m_FacingRight) || (moveInput < 0 && m_FacingRight))
				Flip();


			// Calculate the speed at the direction we want to move.
			float targetSpeed = moveInput * m_Stats.m_MoveSpeed.Value;

			// Calculate the difference between current velocity and desired velocity.
			float speedDiff = targetSpeed - m_Rigidbody2D.velocity.x;

			// Define the rate to be either accelerate or decelerate depending on moving or stopping situation.
			float accelRate = (Mathf.Abs(targetSpeed) > .01f) ? m_Acceleration : m_Deceleration;

			// Applies the rate to speed difference, then raises to a set power so acceleration increases with higher speed.
			// And multiplies by the sign to reaplly direction.
			float moveForce = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, m_VelPower) * Mathf.Sign(speedDiff);


			// Play the dust effect when running.
			if (Mathf.Abs(moveInput) > 0f && m_Grounded && runningDust.isStopped)
				runningDust.Play();
			
			else if ((moveInput == 0f && runningDust.isPlaying) || !m_Grounded)
				runningDust.Stop();


			// Applies the force to the rigidbody, respectively when on slope or not.
			if (onSlope && canWalkOnSlope)
				// Negative because the direction of the perpendicular vector.
				m_Rigidbody2D.AddForce(-moveForce * slopeNormalPerp);

			else if (!onSlope || !canWalkOnSlope)
				m_Rigidbody2D.AddForce(moveForce * Vector2.right);
		}

		// If the player should jump.
		if (jump && canWalkOnSlope)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
			jumpingDust.Play();

			jump = false;
			m_Animator.SetBool("IsJumping", false);
		}

		// If the player should dash.
		if (dash)
		{
			if (!onSlope)
				StartCoroutine(Dash());

			dash = false;
		}

		// Check if we're grounded and are trying to stop (not pressing movement keys).
		if (m_Grounded && Mathf.Abs(moveInput) < 0.01f)
		{
			if (onSlope && canWalkOnSlope)
				m_Rigidbody2D.sharedMaterial = grippy;
			
			else if (onSlope && !canWalkOnSlope)
				m_Rigidbody2D.sharedMaterial = slippery;
			
			else  // If not on slopes.
			{
				// Then set the friction force to the minimum value between the m_FrictionAmount and our velocity.
				float frictionForce = Mathf.Min(Mathf.Abs(m_Rigidbody2D.velocity.x), Mathf.Abs(m_FrictionAmount));

				// Set it to the movement direction.
				frictionForce *= Mathf.Sign(m_Rigidbody2D.velocity.x);

				// Then applies it against the movement direction.
				m_Rigidbody2D.AddForce(-frictionForce * Vector2.right, ForceMode2D.Impulse);
			}
		}
		else
			m_Rigidbody2D.sharedMaterial = slippery;
	}

	#region Check for ground and slopes
	private void CheckGround()
	{
		m_Animator.SetBool("Grounded", m_Grounded);

		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);

		for (int i = 0; i < colliders.Length; i++)
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;

				// Landing event only triggers when falling to the ground.
				if (!wasGrounded && m_Rigidbody2D.velocity.y < 0f)
					OnLandEvent.Invoke();
			}
	}

	private void CheckSlope()
	{
		Vector2 checkPos = m_GroundCheck.position;

		CheckSlopeHorizontal(checkPos);
		CheckSlopeVertical(checkPos);
	}

	private void CheckSlopeHorizontal(Vector2 checkPos)
	{
		RaycastHit2D frontHit = Physics2D.Raycast(checkPos, transform.right, m_SlopeCheckDistance, m_WhatIsGround);
		RaycastHit2D backHit = Physics2D.Raycast(checkPos, -transform.right, m_SlopeCheckDistance, m_WhatIsGround);
		
		if (frontHit)
		{
			onSlope = true;
			slopeSideAngle = Vector2.Angle(frontHit.normal, Vector2.up);
		}
		else if (backHit)
		{
			onSlope = true;
			slopeSideAngle = Vector2.Angle(backHit.normal, Vector2.up);
		}
		else
		{
			onSlope = false;
			slopeSideAngle = 0f;
		}

		Debug.DrawRay(frontHit.point, frontHit.normal, Color.yellow);
		Debug.DrawRay(backHit.point, backHit.normal, Color.yellow);
	}

	private void CheckSlopeVertical(Vector2 checkPos)
	{
		RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, m_SlopeCheckDistance, m_WhatIsGround);

		if (hit)
		{
			// Get the normalized perpendicular vector to our hit's normal vector.
			// The resulted vector always rotated 90-deg counter-clockwise.
			slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

			// The angle between the y-axis and our normal, which happens to be the angle between the x-axis and the slope.
			slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

			if (slopeDownAngle != slopeDownAngleOld)
				onSlope = true;

			slopeDownAngleOld = slopeDownAngle;

			//Debug.Log("On Slope: " + onSlope);
			Debug.DrawRay(hit.point, slopeNormalPerp, Color.red);
			Debug.DrawRay(hit.point, hit.normal, Color.yellow);
		}

		if (slopeSideAngle >= 90f)
			onSlope = canWalkOnSlope = false;
		else if (slopeDownAngle > m_MaxSlopeAngle || slopeSideAngle > m_MaxSlopeAngle)
			canWalkOnSlope = false;
		else
			canWalkOnSlope = true;
	}
	#endregion

	public void Flip()
	{
		// Switch the way the player is labelled as facing when she is not attacking.
		// Also switch the direction of the running dust particles.
		if (PlayerActions.isComboDone)
		{
			m_FacingRight = !m_FacingRight;
			transform.Rotate(0f, 180f, 0f);

			Vector2 scale = runningDust.transform.localScale;
			scale.x *= -1f;
			runningDust.transform.localScale = scale;
		}
	}

	private IEnumerator Dash()
	{
		m_IsDashing = true;
		m_Animator.SetTrigger("IsDashing");
		Physics2D.IgnoreLayerCollision(3, 8, true);

		float originalGravity = m_Rigidbody2D.gravityScale;
		m_Rigidbody2D.gravityScale = 0f;

		m_TrailRenderer.emitting = true;

		m_Rigidbody2D.AddForce(Mathf.Sign(transform.rotation.y) * m_DashForce * Vector2.right, ForceMode2D.Impulse);

		yield return new WaitForSeconds(m_DashingTime);

		m_TrailRenderer.emitting = false;
		m_Rigidbody2D.gravityScale = originalGravity;
		m_IsDashing = false;

		Physics2D.IgnoreLayerCollision(3, 8, false);
	}
}
