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
	public static bool useLadder = false;

	float horizontalMove;
	float verticalMove;
	bool jump = false;
	bool crouch = false;

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
		Debug.Log(horizontalMove);

		animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
		animator.SetFloat("yVelocity", rb2D.velocity.y);

		if (useLadder && (!animator.GetBool("Grounded") || verticalMove == 0f))
		{
			animator.speed = 0;
		}

		if (useLadder && verticalMove == 1f)
		{
			rb2D.gravityScale = 0;
			playerPos.position += new Vector3(0f, 0.02f, 0f);
			animator.SetBool("UseLadder", true);
			animator.speed = 1;
		}
		else if (useLadder && verticalMove == -1f)
		{
			rb2D.gravityScale = 0;
			playerPos.position -= new Vector3(0f, 0.02f, 0f);
			animator.SetBool("UseLadder", true);
			animator.speed = 1;
		}
		else if (!useLadder)
		{
			rb2D.gravityScale = 3;
			animator.SetBool("UseLadder", false);
			animator.speed = 1;
		}

		if (Input.GetButtonDown("Jump"))
		{
			jump = true;
			animator.SetBool("IsJumping", true);
		}

		if (Input.GetButtonDown("Crouch"))
			crouch = true;
		else if (Input.GetButtonUp("Crouch"))
			crouch = false;
	}

	private void FixedUpdate()
	{
		controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
		jump = false;
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
