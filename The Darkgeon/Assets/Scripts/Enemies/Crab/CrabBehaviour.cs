using UnityEngine;
using System.Collections;
using UnityEditorInternal;

/// <summary>
/// Manages the Crab's behaviours and targeting AI.
/// </summary>
public class CrabBehaviour : MonoBehaviour, IEnemyBehaviour
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
	
	[HideInInspector] public bool facingRight = true;
	
	// Private fields.
	private bool alreadyAttacked, abilityUsed;
	private bool isPatrol = true;
	private bool mustFlip, isTouchingWall;
	private bool playerInAggro, canAttackPlayer;

	private float timeBetweenJump = 0f;  // Default is 2f.
	private float switchDirDelay = 0f;  // Default is 1f.
	private float chaseDirection = 1.5f;
	private float abandonTimer;
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

		if(!abilityUsed && ((double)stats.currentHP/stats.maxHealth) <= .5)
		{
			StartCoroutine(UseAbility());
			abilityUsed = true;
		}
			

		if (!GameManager.isPlayerDeath)
		{
			// Check if the player is in what range of the enemy.
			float distToPlayer = Vector2.Distance(centerPoint.position, player.transform.position);
			playerInAggro = distToPlayer <= inSightRange;
			canAttackPlayer = Physics2D.OverlapCircle(centerPoint.position, attackRange, whatIsPlayer);
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

			// Begin chasing the player if she's right in front of the enemy.
			if ((!isPlayerBehind && facingRight) || (isPlayerBehind && !facingRight))
				spottingTimer = 0f;

			else
				spottingTimer -= Time.deltaTime;
		}

		// If the player is within the atk range and already spotted her.
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

	#region Crab's behaviours
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
		if (yDistance >= 2f && switchDirDelay <= 0f)
		{
			chaseDirection = Mathf.Sign(player.transform.position.x - centerPoint.position.x) * 1.5f;
			switchDirDelay = 1f;
		}

		else if (yDistance < 2f)
			chaseDirection = Mathf.Sign(player.transform.position.x - centerPoint.position.x) * 1.5f;

		rb2d.velocity = new Vector2(walkSpeed * chaseDirection * Time.fixedDeltaTime, rb2d.velocity.y);

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

	public void ResetAttack()
	{
		alreadyAttacked = false;
	}

	// Crab's Ability: Hard Shell.
	private IEnumerator UseAbility()
	{
		float baseSpeed = walkSpeed;
		float baseAtkSpeed = timeBetweenAtk;
		int baseArmor = stats.armor;
		float baseAtkDamage = stats.atkDamage;
		float baseKBRes = stats.knockBackRes;

		animator.SetTrigger("Ability");
		Color popupTextColor = new Color(.84f, .45f, .15f);
		DamageText.Generate(stats.dmgTextPrefab, stats.dmgTextPos.position, popupTextColor, false, "Hard Shell");

		walkSpeed /= 2;
		timeBetweenAtk *= 2;
		stats.armor *= 2;
		stats.atkDamage *= 2;
		stats.knockBackRes *= 2;

		yield return new WaitForSeconds(abilityDuration);

		walkSpeed = baseSpeed;
		timeBetweenAtk = baseAtkSpeed;
		stats.armor = baseArmor;
		stats.atkDamage = baseAtkDamage;
		stats.knockBackRes = baseKBRes;
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
		if (edgeCheck == null || centerPoint == null || attackPoint == null)
			return;

		Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);
		Gizmos.DrawWireSphere(attackPoint.position, attackRange);
		Gizmos.DrawWireSphere(centerPoint.position, inSightRange);
	}
}
