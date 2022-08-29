using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	// References.
	[SerializeField] private CharacterController2D controller;
	[SerializeField] private Animator animator;
	[SerializeField] private Rigidbody2D rb2D;
	[SerializeField] private RectTransform playerPos;

	// Fields.
	public float moveSpeed = 30f;
	public float climbSpeed = 0.01f;
	public static bool useLadder = false;

	float horizontalMove;
	float verticalMove;
	float dashAllowTime = 0f;

	bool jump = false;
	bool crouch = false;
	bool dash = false;

	private void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		animator = GetComponent<Animator>();
		rb2D = GetComponent<Rigidbody2D>();
		playerPos = GetComponent<RectTransform>();
	}

	// Update is called once per frame
	private void Update()
	{
		horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;
		verticalMove = Input.GetAxisRaw("UseLadder");
		Debug.Log(verticalMove);

		animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
		animator.SetFloat("yVelocity", rb2D.velocity.y);

		if (useLadder && (!animator.GetBool("Grounded") || verticalMove == 0f))
		{
			if (animator.GetCurrentAnimatorStateInfo(0).IsName("LadderUp") ||
				animator.GetCurrentAnimatorStateInfo(0).IsName("LadderDown"))
				animator.speed = 0;
		}

		if (useLadder && verticalMove == 1f)
		{
			rb2D.gravityScale = 0;
			playerPos.position += new Vector3(0f, climbSpeed, 0f);
			animator.SetBool("LadderUp", true);
			animator.SetBool("LadderDown", false);
			animator.speed = 1;
		}
		else if (useLadder && verticalMove == -1f)
		{
			rb2D.gravityScale = 0;
			playerPos.position -= new Vector3(0f, climbSpeed, 0f);
			animator.SetBool("LadderDown", true);
			animator.SetBool("LadderUp", false);
			animator.speed = 1;
		}
		else if (!useLadder || animator.GetBool("Grounded"))
		{
			rb2D.gravityScale = 3;
			animator.SetBool("LadderUp", false);
			animator.SetBool("LadderDown", false);
			animator.speed = 1;
		}

		if (Input.GetButtonDown("Jump"))
		{
			jump = true;
			animator.SetBool("IsJumping", true);
		}

		if (Input.GetButtonDown("Dash") && Time.time > dashAllowTime)
		{
			dash = true;
			dashAllowTime = Time.time + 1f;
		}

		if (Input.GetButtonDown("Crouch"))
			crouch = true;
		else if (Input.GetButtonUp("Crouch"))
			crouch = false;
	}

	private void FixedUpdate()
	{
		controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, dash);
		jump = false;
		dash = false;
	}

	public void OnLanding()
	{
		animator.SetBool("IsJumping", false);
		Debug.Log(animator.GetBool("IsJumping"));
	}

	public void OnCrouching(bool isCrouching)
	{
		animator.SetBool("IsCrouching", isCrouching);
		Debug.Log("Crouching: " + animator.GetBool("IsCrouching"));
	}
}
