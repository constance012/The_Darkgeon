using UnityEngine;
/// <summary>
/// An base class contains virtual enemy's behaviour methods and shared members.
/// </summary>
public abstract class EnemyBehaviour : MonoBehaviour
{
	[Header("Base Class Section")]
	[Space]

	[Header("Reference")]
	[Space]
	protected Rigidbody2D rb2d;
	protected Animator animator;
	protected PhysicsMaterial2D physicMat;

	[Header("Checking, Layers")]
	[Space]
	[SerializeField] protected Transform edgeCheck;
	[SerializeField] protected Transform centerPoint;
	public LayerMask whatIsPlayer;
	[SerializeField] protected LayerMask whatIsGround;

	[Header("Stats Scripts")]
	[Space]
	protected PlayerStats player;
	protected EnemyStat stats;

	[Header("Check Ranges")]
	[Space]
	public float walkSpeed;
	public float checkRadius;
	public float attackRange;
	public float inSightRange;

	[Header("Timers")]
	[Space]
	public float m_SpottingTimer = 3f;
	[SerializeField] protected float m_AbandonTimer = 8f;
	[SerializeField] protected float timeBetweenAtk = 1.5f;

	[HideInInspector] public bool facingRight { get; set; } = true;

	// Protected fields.
	protected bool alreadyAttacked;
	protected bool isPatrol = true;
	protected bool mustFlip, isTouchingWall;
	protected bool playerInAggro, canAttackPlayer;
	protected bool abilityUsed;

	protected float timeBetweenJump = 0f;  // Default is 2f.
	protected float switchDirDelay = 0f;  // Default is 1f.
	protected float chaseDirection = 1.5f;
	protected float abandonTimer;
	[HideInInspector] public float spottingTimer { get; set; }

	protected virtual void Awake()
	{
		rb2d = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		stats = GetComponent<EnemyStat>();
		physicMat = rb2d.sharedMaterial;
		player = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
	}

	protected virtual void Start()
	{
		spottingTimer = m_SpottingTimer;
	}

	protected virtual void Update()
	{
		if (!stats.isFlyingEnemy)
			animator.SetFloat("Speed", Mathf.Abs(rb2d.velocity.x));

		if (isPatrol)
			Patrol();

		if (!GameManager.IsPlayerDeath)
		{
			// Check if the player is in what range of the enemy.
			float distToPlayer = Vector2.Distance(centerPoint.position, player.transform.position);
			playerInAggro = distToPlayer <= inSightRange;

			canAttackPlayer = Physics2D.OverlapCircle(centerPoint.position, attackRange, whatIsPlayer);
		}

		// If the player's death, simply return.
		else
		{
			playerInAggro = canAttackPlayer = false;
			isPatrol = true;
			spottingTimer = m_SpottingTimer;
			abandonTimer = 0f;
			return;
		}

		#region Behaviours against the player if she's alive.
		// If the abandon timer runs out, then just patrol around.
		if (abandonTimer <= 0f)
		{
			isPatrol = true;
			spottingTimer = m_SpottingTimer;
		}

		// If the player is out of the aggro range, continue chasing the player until abandons.
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

			// Start chasing the player when the timer runs out.
			if (spottingTimer <= 0f)
				ChasePlayer();

			// Instantly spotting the player if currently facing towards her.
			if ((!isPlayerBehind && facingRight) || (isPlayerBehind && !facingRight))
				spottingTimer = 0f;

			// Else, decrease the timer over time.
			else
				spottingTimer -= Time.deltaTime;
		}

		// If the player is within the atk range.
		else if (canAttackPlayer && spottingTimer <= 0f)
			Attack();
		#endregion
	}

	protected virtual void FixedUpdate()
	{
		physicMat.friction = stats.grounded ? .5f : 0f;

		if (isPatrol)
			mustFlip = !Physics2D.OverlapCircle(edgeCheck.position, checkRadius, whatIsGround);
	}

	protected void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Ground") && !stats.isFlyingEnemy)
			isTouchingWall = true;
	}

	#region Virtual behaviour methods.
	protected virtual void Patrol()
	{
		if (mustFlip || isTouchingWall)
		{
			Flip();
			isTouchingWall = false;
		}

		rb2d.velocity = facingRight ? new Vector2(walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y)
									: new Vector2(-walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y);
	}

	protected abstract void ChasePlayer();
	protected abstract void Attack();

    protected void ResetAttack() => alreadyAttacked = false;
    
	protected void Flip()
	{
		isPatrol = false;

		transform.localScale = transform.localScale.FlipByScale('x');
		facingRight = !facingRight;

		isPatrol = true;
	}
	#endregion

	protected virtual void OnDrawGizmosSelected()
	{
		if (edgeCheck == null || centerPoint == null)
			return;

		Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);
		Gizmos.DrawWireSphere(centerPoint.position, attackRange);
		Gizmos.DrawWireSphere(centerPoint.position, inSightRange);
	}
}
