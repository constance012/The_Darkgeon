using UnityEngine;
using CSTGames.CommonEnums;

/// <summary>
/// Manages the Spiked Slime's behaviours and targeting AI.
/// </summary>
public class SpikedSlimeBehaviour : EnemyBehaviour
{
	[Header("Derived Class Section")]
	[Space]

	[Header("Debuffs")]
	[Space]
	[SerializeField] private Debuff poisoned;

	[Header("Effects")]
	[Space]
	[SerializeField] private ParticleSystem abilityFx;

	public float abilityDuration;
	[HideInInspector] public bool atkAnimDone = true;

	protected override void Awake()
	{
		base.Awake();
		
		abilityFx = transform.Find("Toxic Cloud Effect").GetComponent<ParticleSystem>();
	}

	protected override void Update()
	{
		base.Update();

		if (!abilityUsed & (float)stats.currentHP / stats.maxHealth <= .8f)
		{
			Invoke(nameof(UseAbility), 2f);
			abilityUsed = true;
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		//Debug.Log("Contacted with the something.");

		if (collision.collider.CompareTag("Player"))
		{
			// Engage immediately if contacted with the player.
			spottingTimer = 0f;
			int inflictChance = Random.Range(1, 11);

			// Deal contact damage if needed.
			if (stats.contactDamage > 0f)
			{
				player.TakeDamage(stats.contactDamage, stats.knockBackVal, this.transform, KillSources.SpikedSlime);

				if (inflictChance == 1)
					DebuffManager.instance.ApplyDebuff(Instantiate(poisoned));
			}
		}
	}

	#region Spiked Slime's Behaviours
	protected override void ChasePlayer()
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
				switchDirDelay = 1f;

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

	protected override void Attack()
	{
		if (!alreadyAttacked)
		{
			// Make sure the enemy doesn't move.
			rb2d.velocity = Vector2.zero;

			animator.SetTrigger("Atk");

			atkAnimDone = false;
			alreadyAttacked = true;

			Invoke(nameof(ResetAttack), timeBetweenAtk);  // Can attack every 2 seconds.
		}
	}

	private void UseAbility()
	{
		if (Random.Range(1, 6) == 1)
		{
			animator.SetTrigger("Ability");
			abilityFx.Play();

			Collider2D hitObj = Physics2D.OverlapCircle(centerPoint.position, attackRange + .2f, whatIsPlayer);

			if (hitObj != null)
			{
				player.TakeDamage(stats.atkDamage, 0f, this.transform, KillSources.SpikedSlime);
				
				Debuff heavyPoisoned = Instantiate(poisoned);
				heavyPoisoned.duration *= 3f;

				DebuffManager.instance.ApplyDebuff(heavyPoisoned);
			}

			Color popupTextColor = new Color(1f, .76f, 0f);
			DamageText.Generate(stats.dmgTextPrefab, stats.dmgTextPos.position, popupTextColor, false, "Toxic Cloud");
		}
	}
	#endregion
}
