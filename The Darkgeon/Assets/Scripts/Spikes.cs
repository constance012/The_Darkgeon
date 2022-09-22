using UnityEngine;

public class Spikes : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private PlayerStats playerStats;

	private void Awake()
	{
		playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (playerStats.currentHP <= 0)
			playerStats.killSource = KillSources.Environment;
		
		if (collision.collider.tag == "Player"
			&& Time.time - playerStats.lastDamagedTime > playerStats.invincibilityTime)
			playerStats.TakeDamage(15);
	}
}
