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

	[Space]
	public float spottingTimer = 3f;
	[SerializeField] float abandonTimer = 8f;
	[SerializeField] bool alreadyAttacked = false;
	
	bool isPatrol = true;
	bool mustFlip, isTouchingWall;
	bool facingRight = true;
	bool playerInAggro, canAttackPlayer;

	float timeBetweenAtk = 1.5f;
	float timeBetweenJump = 2f;

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
		float distToPlayer = Vector2.Distance(centerPoint.position, player.transform.position);
		
		playerInAggro = distToPlayer <= inSightRange;
		canAttackPlayer = Physics2D.OverlapCircle(centerPoint.position, attackRange, whatIsPlayer);

		if (abandonTimer <= 0f)
		{
			isPatrol = true;
			spottingTimer = 3f;
		}

		if (isPatrol)
			Patrol();

		if (!playerInAggro)
		{
			if (spottingTimer <= 0f)
			{
				ChasePlayer();
				abandonTimer -= Time.deltaTime;
			}
			else
				spottingTimer = 3f;
		}

		else if (playerInAggro && !canAttackPlayer)
		{
			abandonTimer = 10f;
			bool isPlayerBehind = player.transform.position.x < centerPoint.position.x;

			if (spottingTimer <= 0f)
				ChasePlayer();

			if ((!isPlayerBehind && facingRight) || (isPlayerBehind && !facingRight))
				spottingTimer = 0f;

			else
				spottingTimer -= Time.deltaTime;
		}

		else if (canAttackPlayer)
			Attack();
	}

	private void FixedUpdate()
	{
		if (isPatrol)
			mustFlip = !Physics2D.OverlapCircle(edgeCheck.position, checkRadius, whatIsGround);
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.CompareTag("Ground"))
			isTouchingWall = true;
	}

	private void Patrol()
	{
		abandonTimer = 10f;

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
		isPatrol = false;
		timeBetweenJump -= Time.deltaTime;

		// Chase is faster than patrol.
		float direction = Mathf.Sign(player.transform.position.x - centerPoint.position.x) * 1.5f;  

		rb2d.velocity = new Vector2(walkSpeed * direction * Time.fixedDeltaTime, rb2d.velocity.y);

		if (isTouchingWall && timeBetweenJump <= 0f)
		{
			rb2d.velocity = new Vector2(rb2d.velocity.x, 5f);
			isTouchingWall = false;
			timeBetweenJump = 2f;
		}

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
