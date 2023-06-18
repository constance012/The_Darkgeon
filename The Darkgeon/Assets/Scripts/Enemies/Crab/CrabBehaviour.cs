using UnityEngine;
using System.Collections;
using CSTGames.CommonEnums;

/// <summary>
/// Manages the Crab's behaviours and targeting AI.
/// </summary>
[RequireComponent(typeof(EnemyStat))]
public class CrabBehaviour : EnemyBehaviour
{
	[Header("Derived Class Section")]
	[Space]

	[Header("Additional references")]
	[Space]
	public Transform attackPoint;

	[Header("Additional members")]
	[Space]
	public float abilityDuration;

	protected override void Update()
	{
		base.Update();

		if (!abilityUsed && (float)stats.currentHP / stats.maxHealth <= .5f)
		{
			StartCoroutine(UseAbility());
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

			// Deal contact damage if needed.
			if (stats.contactDamage > 0f)
				player.TakeDamage(stats.contactDamage, stats.knockBackVal, this.transform, KillSources.Crab);
		}
	}

	#region Crab's behaviours
	protected override void ChasePlayer()
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

	protected override void Attack()
	{
		// Make sure the enemy doesn't move.
		rb2d.velocity = Vector2.zero;

		if (!alreadyAttacked)
		{
			int randomNum = Random.Range(1, 4);
			animator.SetTrigger("Atk" + randomNum);

			alreadyAttacked = true;
			Invoke(nameof(ResetAttack), timeBetweenAtk);  // Can attack every 2 seconds.
		}
	}

	// Crab's Ability: Hard Shell.
	private IEnumerator UseAbility()
	{
		if (Random.Range(1, 6) == 1)
		{
			float baseSpeed = walkSpeed;
			float baseAtkSpeed = timeBetweenAtk;
			int baseArmor = stats.armor;
			float baseAtkDamage = stats.atkDamage;
			float baseKBRes = stats.knockBackRes;

			animator.SetTrigger("Ability");

			Color popupTextColor = new Color(1f, .76f, 0f);
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
	}
	#endregion

	protected override void OnDrawGizmosSelected()
	{
		if (edgeCheck == null || centerPoint == null || attackPoint == null)
			return;

		Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);
		Gizmos.DrawWireSphere(attackPoint.position, attackRange);
		Gizmos.DrawWireSphere(centerPoint.position, inSightRange);
	}
}
