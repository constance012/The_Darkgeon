using UnityEngine;

/// <summary>
/// Manages the player's vertical movement, like climbing a ladder.
/// </summary>
public class LadderClimbing : MonoBehaviour
{
	[Header("Reference")]
	[Space]
	private Rigidbody2D rb2d;
	private Animator animator;

	[Header("Fields")]
	[Space]
	public float speed = .8f;
	
	private float verticalMove;
	private float originalGravity;
	private bool isClimbing;

	private int useLadderHash, verticalInputHash, groundedHash;

	private void Awake()
	{
		rb2d = GetComponent<Rigidbody2D>();
		animator = transform.GetComponentInChildren<Animator>("Graphic");
		originalGravity = rb2d.gravityScale;
	}

	private void Start()
	{
		useLadderHash = Animator.StringToHash("UseLadder");
		verticalInputHash = Animator.StringToHash("VerticalInput");
		groundedHash = Animator.StringToHash("Grounded");
	}

	private void FixedUpdate()
	{
		if (isClimbing)
			rb2d.velocity = new Vector2(rb2d.velocity.x, verticalMove * speed);
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (!collision.CompareTag("Ladder"))
			return;

		verticalMove = InputManager.instance.GetAxisRaw("vertical");
		animator.SetFloat(verticalInputHash, verticalMove);

		bool wasClimbing = isClimbing;

		isClimbing = false;

		if (verticalMove != 0f)
			isClimbing = true;

		if (animator.GetBool(groundedHash) && animator.GetBool(useLadderHash))
		{
			animator.SetBool(useLadderHash, false);
			animator.speed = 1f;
		}
		else if (isClimbing && !animator.GetBool(useLadderHash))
			animator.SetBool(useLadderHash, true);

		if (wasClimbing != isClimbing)
		{
			if (isClimbing)
			{
				rb2d.gravityScale = originalGravity;
				animator.speed = 1f;
			}

			else
			{
				rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
				rb2d.gravityScale = 0f;
				animator.speed = 0f;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Ladder"))
		{
			isClimbing = false;
			rb2d.gravityScale = originalGravity;

			animator.SetBool(useLadderHash, false);
			animator.speed = 1f;
		}
	}
}
