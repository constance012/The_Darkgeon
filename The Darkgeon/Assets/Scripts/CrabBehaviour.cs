using UnityEngine;

public class CrabBehaviour : MonoBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private Rigidbody2D rb2d;

	[SerializeField] private Transform edgeCheck;
	[SerializeField] private LayerMask whatIsGround;

	[Header("Fields")]
	[Space]
	public float walkSpeed = 1f;
	public float checkRadius = .3f;

	bool isPatrol = true;
	bool mustFlip;
	bool isTouchingWall;

	private void Awake()
	{
		rb2d = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		if (isPatrol)
			Patrol();
	}

	private void FixedUpdate()
	{
		if (isPatrol)
			mustFlip = !Physics2D.OverlapCircle(edgeCheck.position, checkRadius, whatIsGround);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Ground")
			isTouchingWall = true;
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		isTouchingWall = false;
	}

	void Patrol()
	{
		if (mustFlip || isTouchingWall)
			Flip();

		rb2d.velocity = new Vector2(walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y);
	}

	void Flip()
	{
		isPatrol = false;

		transform.localScale = new Vector2(transform.localScale.x * -1f, transform.localScale.y);
		walkSpeed *= -1f;

		isPatrol = true;
	}

	private void OnDrawGizmosSelected()
	{
		if (edgeCheck == null)
			return;

		Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);
	}
}
