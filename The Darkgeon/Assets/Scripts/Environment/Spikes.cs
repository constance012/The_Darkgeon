using UnityEngine;

/// <summary>
/// This class is responsible for spike collision affects.
/// </summary>
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
		if (collision.collider.CompareTag("Player"))
			playerStats.TakeDamage(15, 0, null, KillSources.Environment);
	}
}
