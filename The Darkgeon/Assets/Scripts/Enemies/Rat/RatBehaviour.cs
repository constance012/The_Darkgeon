using System.Collections;
using UnityEngine;
using static Unity.VisualScripting.Member;

/// <summary>
/// Manages the Rat's behaviours and targeting AI.
/// </summary>
public class RatBehaviour : MonoBehaviour, IEnemyBehaviour
{
	[Header("Reference")]
	[Space]
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Animator animator;
	[SerializeField] private PhysicsMaterial2D physicMat;

	[Space]
	[SerializeField] private Transform edgeCheck;
	[SerializeField] private Transform centerPoint;
	public LayerMask whatIsPlayer;
	[SerializeField] private LayerMask whatIsGround;

	[Space]
	[SerializeField] private PlayerStats player;
	[SerializeField] private EnemyStat stats;

	[Header("Debuffs")]
	[Space]
	[SerializeField] private Debuff bleeding;
	[SerializeField] private Debuff slowness;

	[Header("Fields")]
	[Space]
	public float walkSpeed;
	public float checkRadius;
	public float attackRange;
	public float inSightRange;

	[Space]
	public float m_SpottingTimer = 3f;
	[SerializeField] private float m_AbandonTimer = 8f;
	[SerializeField] private float timeBetweenAtk = 1.5f;

	[HideInInspector] public bool facingRight = true;
	[HideInInspector] public bool atkAnimDone = true;
	
	// Private fields.
	private bool alreadyAttacked;
	private bool isPatrol = true;
	private bool mustFlip, isTouchingWall;
	private bool playerInAggro, canAttackPlayer;
	private bool abilityUsed, canUseAbilityAtk;

	private float timeBetweenJump = 0f;  // Default is 2f.
	private float switchDirDelay = 0f;  // Default is 1f.
	private float chaseDirection = 1.5f;
	private float abandonTimer;
	[HideInInspector] public float spottingTimer { get; set; }

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

		if (!abilityUsed & (float)stats.currentHP / stats.maxHealth <= .5f)
		{
			UseAbility();
			abilityUsed = true;
		}

		if (!GameManager.isPlayerDeath)
		{
			// Check if the player is in what range of the enemy.
			float distToPlayer = Vector2.Distance(centerPoint.position, player.transform.position);
			playerInAggro = distToPlayer <= inSightRange;
			
			// Bigger range for hopping towards player when attacks.
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

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			// Engage immediately if contacted with the player.
			spottingTimer = 0f;
			
			int inflictChance = Random.Range(1, 11);

			// Deal contact damage if needed.
			if (stats.contactDamage > 0f)
			{
				player.TakeDamage(stats.contactDamage, stats.knockBackVal, this.transform, KillSources.Rat);

				// Inflict heavier bleeding and slowness debuff if the ability is used.
				if (canUseAbilityAtk) 
				{
					Debuff extendedBleeding = Instantiate(bleeding);
					extendedBleeding.duration += 5f;

					FindObjectOfType<DebuffManager>().ApplyDebuff(extendedBleeding);
					FindObjectOfType<DebuffManager>().ApplyDebuff(Instantiate(slowness));

					canUseAbilityAtk = false;
				}

				// Else, has 10% chance of inflicting regular bleeding
				else if (inflictChance == 1)
					FindObjectOfType<DebuffManager>().ApplyDebuff(Instantiate(bleeding));
			}
		}
	}

	#region Rat's behaviours
	public void Patrol()
	{
		if (mustFlip || isTouchingWall)
		{
			Flip();
			isTouchingWall = false;
		}

		rb2d.velocity = facingRight ? new Vector2(walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y)
									: new Vector2(-walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y);
	}

	public void ChasePlayer()
	{
		isPatrol = false;
		timeBetweenJump -= Time.deltaTime;
		switchDirDelay -= Time.deltaTime;

		float yDistance = Mathf.Abs(player.transform.position.y - centerPoint.position.y);
		//Debug.Log("Y Distance: " + yDistance);

		// Chase is faster than patrol.
		if (atkAnimDone)
		{
			if (yDistance >= 2f && switchDirDelay <= 0f)
			{
				chaseDirection = Mathf.Sign(player.transform.position.x - centerPoint.position.x) * 1.5f;
				switchDirDelay = 1f;
			}

			else if (yDistance < 2f)
				chaseDirection = Mathf.Sign(player.transform.position.x - centerPoint.position.x) * 1.5f;

			rb2d.velocity = new Vector2(walkSpeed * chaseDirection * Time.fixedDeltaTime, rb2d.velocity.y);
		}

		// Jump if there's an obstacle ahead.
		if (isTouchingWall && stats.grounded && timeBetweenJump <= 0f)
		{
			rb2d.velocity = new Vector2(rb2d.velocity.x, 7f);
			isTouchingWall = false;
			timeBetweenJump = 2f;
		}

		if (!facingRight && chaseDirection > 0f)
			Flip();

		else if (facingRight && chaseDirection < 0f)
			Flip();
	}

	public void Attack()
	{
		if (!alreadyAttacked)
		{
			// Make sure the enemy doesn't move.
			rb2d.velocity = Vector3.zero;

			animator.SetTrigger("Atk");

			atkAnimDone = false;
			alreadyAttacked = true;
			Invoke(nameof(ResetAttack), timeBetweenAtk);  // Can attack every 2 seconds.
		}
	}

	public void ResetAttack()
	{
		alreadyAttacked = false;
	}

	private void UseAbility()
	{
		if (Random.Range(1, 6) == 1)
		{
			animator.SetTrigger("Ability");

			Color popupTextColor = new Color(1f, .76f, 0f);
			DamageText.Generate(stats.dmgTextPrefab, stats.dmgTextPos.position, popupTextColor, false, "Deadly Bite");

			canUseAbilityAtk = true;
		}
	}
	#endregion

	public void Flip()
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
