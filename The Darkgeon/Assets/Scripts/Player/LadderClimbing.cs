using UnityEngine;

/// <summary>
/// Manages the player's vertical movement, like climbing a ladder.
/// </summary>
public class LadderClimbing : MonoBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Animator animator;

	[Header("Fields")]
	[Space]
	public float speed = .8f;
	
	private float verticalMove;
	private float originalGravity;
	private bool useLadder;
	private bool isClimbing;

	private void Awake()
	{
		rb2d = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		originalGravity = rb2d.gravityScale;
	}

	private void Update()
	{
		verticalMove = InputManager.instance.GetAxisRaw("vertical");

		if (useLadder)
		{
			if (verticalMove != 0f)
				isClimbing = true;

			if (verticalMove == 0f || !animator.GetBool("Grounded"))
			{
				if (animator.GetCurrentAnimatorStateInfo(0).IsName("LadderUp") ||
					animator.GetCurrentAnimatorStateInfo(0).IsName("LadderDown"))
					animator.speed = 0f;
			}

			if (verticalMove > 0f)
			{
				animator.SetBool("LadderUp", true);
				animator.SetBool("LadderDown", false);
				animator.speed = 1f;
			}

			else if (verticalMove < 0f)
			{
				animator.SetBool("LadderDown", true);
				animator.SetBool("LadderUp", false);
				animator.speed = 1f;
			}
		}
	}

	private void FixedUpdate()
	{
		if (isClimbing)
		{
			rb2d.gravityScale = 0f;
			rb2d.velocity = new Vector2(rb2d.velocity.x, verticalMove * speed);
		}
		else
			rb2d.gravityScale = originalGravity;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Ladder"))
		{
			useLadder = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Ladder"))
		{
			useLadder = isClimbing = false;

			animator.SetBool("LadderUp", false);
			animator.SetBool("LadderDown", false);
			animator.speed = 1f;
		}
	}
}
