using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	// References.
	[SerializeField] private CharacterController2D controller;
	[SerializeField] private Animator animator;
	[SerializeField] private Rigidbody2D rb2D;

	// Fields.
	public float moveSpeed = 30f;

	float horizontalMove;
	bool jump = false;
	bool crouch = false;

	private void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		animator = GetComponent<Animator>();
		rb2D = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	private void Update()
	{
		horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;
		animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
		animator.SetFloat("yVelocity", rb2D.velocity.y);

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
