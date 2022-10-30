using UnityEngine;

public class RatBehaviour : MonoBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Animator animator;
	[SerializeField] private PhysicsMaterial2D physicMat;

	[Space]
	[SerializeField] private Transform edgeCheck;
	[SerializeField] private Transform centerPoint;
	public Transform attackPoint;
	public LayerMask whatIsPlayer;
	[SerializeField] private LayerMask whatIsGround;

	[Space]
	[SerializeField] private PlayerStats player;
	[SerializeField] private EnemyStat stats;

	[Header("Fields")]
	[Space]
	public float walkSpeed;
	public float checkRadius;
	public float attackRange;
	public float inSightRange;
	public float abilityDuration;

	[Space]
	public float m_SpottingTimer = 3f;
	[SerializeField] private float m_AbandonTimer = 8f;
	[SerializeField] private float timeBetweenAtk = 1.5f;

	// Private fields.
	public bool facingRight = true;
	bool alreadyAttacked;
	bool isPatrol = true;
	bool mustFlip, isTouchingWall;
	bool playerInAggro, canAttackPlayer;

	float timeBetweenJump = 2f;
	float abandonTimer;
	[HideInInspector] public float spottingTimer;

	private void Awake()
	{
		rb2d = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		stats = GetComponent<EnemyStat>();
		physicMat = GetComponent<CircleCollider2D>().sharedMaterial;
		player = GameObject.Find("Player").GetComponent<PlayerStats>();
	}

	private void Start()
	{
		spottingTimer = m_SpottingTimer;
	}

	private void Update()
	{
		animator.SetFloat("Speed", Mathf.Abs(rb2d.velocity.x));

		if (isPatrol)
			Patrol();

		if (!GameManager.isPlayerDeath)
		{
			// Check if the player is in what range of the enemy.
			float distToPlayer = Vector2.Distance(centerPoint.position, player.transform.position);
			playerInAggro = distToPlayer <= inSightRange;
			
			// Bigger range for hopping towards player when attacks.
			canAttackPlayer = Physics2D.OverlapCircle(centerPoint.position, attackRange + .3f, whatIsPlayer);
		}
		else
		{
			playerInAggro = canAttackPlayer = false;
			isPatrol = true;
			spottingTimer = m_SpottingTimer;
			abandonTimer = 0f;
			return;  // If the player's death, simply return.
		}

		#region Behaviours against the player if she's alive.
		// If the abandon timer runs, then just patrol around.
		if (abandonTimer <= 0f)
		{
			isPatrol = true;
			spottingTimer = m_SpottingTimer;
		}

		// If the player is out of the aggro range.
		if (!playerInAggro)
		{
			if (spottingTimer <= 0f)
			{
				ChasePlayer();
				abandonTimer -= Time.deltaTime;
			}
			else
				spottingTimer = m_SpottingTimer;
		}

		// If the player is within the aggro range but outside the atk range.
		else if (playerInAggro && !canAttackPlayer)
		{
			abandonTimer = m_AbandonTimer;
			bool isPlayerBehind = player.transform.position.x < centerPoint.position.x;

			if (spottingTimer <= 0f)
				ChasePlayer();

			if ((!isPlayerBehind && facingRight) || (isPlayerBehind && !facingRight))
				spottingTimer = 0f;

			else
				spottingTimer -= Time.deltaTime;
		}

		// If the player is within the atk range.
		else if (canAttackPlayer)
			Attack();
		#endregion
	}

	private void FixedUpdate()
	{
		physicMat.friction = stats.grounded ? .5f : 0f;

		if (isPatrol)
			mustFlip = !Physics2D.OverlapCircle(edgeCheck.position, checkRadius, whatIsGround);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Ground"))
			isTouchingWall = true;
	}

	#region Rat's behaviours
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
		isPatrol = false;
		timeBetweenJump -= Time.deltaTime;

		// Chase is faster than patrol.
		float direction = Mathf.Sign(player.transform.position.x - centerPoint.position.x) * 1.5f;

		rb2d.velocity = new Vector2(walkSpeed * direction * Time.fixedDeltaTime, rb2d.velocity.y);

		// Jump if there's an obstacle ahead.
		if (isTouchingWall && stats.grounded && timeBetweenJump <= 0f)
		{
			rb2d.velocity = new Vector2(0f, 7f);
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
		if (!alreadyAttacked)
		{
			// Make sure the enemy doesn't move.
			rb2d.velocity = Vector3.zero;

			animator.SetTrigger("Atk");

			alreadyAttacked = true;
			Invoke(nameof(ResetAttack), timeBetweenAtk);  // Can attack every 2 seconds.
		}
	}

	private void ResetAttack()
	{
		alreadyAttacked = false;
	}
	#endregion

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
