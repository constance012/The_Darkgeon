using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private HealthBar hpBar;
	[SerializeField] private PlayerStats playerStats;

	private void Awake()
	{
		hpBar = GameObject.Find("Health Bar").GetComponent<HealthBar>();
		playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.collider.tag == "Player"
			&& Time.time - playerStats.lastDamagedTime > playerStats.invincibilityTime)
			playerStats.TakeDamage(15);
	}
}
