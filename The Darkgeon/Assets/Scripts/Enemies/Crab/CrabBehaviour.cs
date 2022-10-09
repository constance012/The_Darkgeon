using UnityEngine;

public class CrabBehaviour : MonoBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Animator animator;
	[Space]
	[SerializeField] private Transform edgeCheck;
	[SerializeField] private Transform centerPoint;
	public Transform attackPoint;
	public LayerMask whatIsPlayer;
	[SerializeField] private LayerMask whatIsGround;
	[Space]
	[SerializeField] private PlayerStats player;

	[Header("Fields")]
	[Space]
	public float walkSpeed;
	public float checkRadius;
	public float attackRange;
	public float inSightRange;

	bool isPatrol = true;
	bool mustFlip, isTouchingWall;
	bool facingRight = true;
	[SerializeField] bool alreadyAttacked = false;
	bool playerSpotted, canAttackPlayer;

	float timeBetweenAtk = 2f;

	private void Awake()
	{
		rb2d = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		player = GameObject.Find("Player").GetComponent<PlayerStats>();
	}

	private void Update()
	{
		animator.SetFloat("Speed", Mathf.Abs(rb2d.velocity.x));

		// Check if the player is in what range of the enemy.
		float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
		playerSpotted = distToPlayer <= inSightRange;
		canAttackPlayer = Physics2D.OverlapCircle(centerPoint.position, attackRange, whatIsPlayer);

		if ((!playerSpotted && !canAttackPlayer) || player.isDeath)
		{
			isPatrol = true;
			Patrol();
		}
		else if (playerSpotted && !canAttackPlayer)
		{
			isPatrol = false;
			ChasePlayer();
		}
		else if (canAttackPlayer)
			Attack();
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

	private void Patrol()
	{
		if (mustFlip || isTouchingWall)
		{
			Flip();
			isTouchingWall = false;
		}

		rb2d.velocity = facingRight ? new Vector2(walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y) 
									: new Vector2(-walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y);
	}

	private void ChasePlayer()
	{
		// Chase is faster than patrol.
		float direction = Mathf.Sign(player.transform.position.x - centerPoint.position.x) * 1.5f;  

		rb2d.velocity = new Vector2(walkSpeed * direction * Time.fixedDeltaTime, rb2d.velocity.y);

		if (!facingRight && direction > 0f)
			Flip();

		else if (facingRight && direction < 0f)
			Flip();
	}

	private void Attack()
	{
		// Make sure the enemy doesn't move.
		rb2d.velocity = Vector3.zero;

		if (!alreadyAttacked)
		{
			int randomNum = Random.Range(1, 4);
			animator.SetTrigger("Atk" + randomNum);

			alreadyAttacked = true;
			Invoke(nameof(ResetAttack), timeBetweenAtk);  // Can attack every 2 seconds.
		}
	}

	private void ResetAttack()
	{
		alreadyAttacked = false;
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
		if (edgeCheck == null || centerPoint == null || attackPoint == null)
			return;

		Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);
		Gizmos.DrawWireSphere(attackPoint.position, attackRange);
		Gizmos.DrawWireSphere(centerPoint.position, inSightRange);
	}
}
