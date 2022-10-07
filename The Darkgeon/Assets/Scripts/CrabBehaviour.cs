using UnityEngine;

public class CrabBehaviour : MonoBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Animator animator;

	[SerializeField] private Transform edgeCheck;
	[SerializeField] private Transform centerPoint;
	[SerializeField] private LayerMask whatIsGround;

	[SerializeField] private PlayerStats player;

	[Header("Fields")]
	[Space]
	public float walkSpeed;
	public float checkRadius;
	public float attackRange;
	public float inSightRange;

	bool isPatrol = true;
	bool mustFlip;
	bool isTouchingWall;
	bool facingRight = true;

	private void Awake()
	{
		rb2d = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		player = GameObject.Find("Player").GetComponent<PlayerStats>();
	}

	private void Update()
	{
		if (isPatrol)
			Patrol();

		animator.SetFloat("Speed", Mathf.Abs(rb2d.velocity.x));

		float distToPlayer = Vector2.Distance(transform.position, player.transform.position);

		if (distToPlayer <= inSightRange)
		{
			isPatrol = false;
			ChasePlayer();
		}
		else
			isPatrol = true;
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

	private void Patrol()
	{
		if (mustFlip || isTouchingWall)
			Flip();

		rb2d.velocity = facingRight ? new Vector2(walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y) 
									: new Vector2(-walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y);
	}

	private void ChasePlayer()
	{
		if (centerPoint.position.x < player.transform.position.x)
		{
			rb2d.velocity = new Vector2(walkSpeed * 1.5f * Time.fixedDeltaTime, rb2d.velocity.y);

			if (!facingRight) Flip();
		}

		else if (centerPoint.position.x > player.transform.position.x)
		{
			rb2d.velocity = new Vector2(walkSpeed * -1.5f * Time.fixedDeltaTime, rb2d.velocity.y);

			if (facingRight) Flip();
		}
	}

	private void Flip()
	{
		isPatrol = false;

		transform.localScale = new Vector2(transform.localScale.x * -1f, transform.localScale.y);
		facingRight = !facingRight;

		isPatrol = true;
	}

	private void OnDrawGizmosSelected()
	{
		if (edgeCheck == null || centerPoint == null)
			return;

		Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);
		Gizmos.DrawWireSphere(centerPoint.position, attackRange);
		Gizmos.DrawWireSphere(centerPoint.position, inSightRange);
	}
}
