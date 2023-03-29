using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages all the enemy's stats.
/// </summary>
public class EnemyStat : MonoBehaviour
{
	[Header("Death Loots")]
	[Space]
	[SerializeField] private GameObject droppedItemPrefab;
	[SerializeField] private GameObject droppedCoinPrefab;
	[SerializeField] private DeathLoot[] loots;

	[Header("Transform, Layers")]
	[Space]
	[SerializeField] private EnemyHPBar hpBar;
	public Transform dmgTextPos;
	public Transform worldCanvas;

	[Space]
	[SerializeField] private Transform player;
	[SerializeField] private Transform centerPoint;
	[SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask whatIsGround;

	[Header("References")]
	[Space]
	[SerializeField] private Animator animator;
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Material enemyMat;
	[SerializeField] private ParticleSystem deathFx;

	public GameObject dmgTextPrefab;
	public EnemyBehaviour behaviour;

	[Header("STATS")]
	[Space]
	[SerializeField] private string enemyName;
	public bool isFlyingEnemy;

	[Header("Offensive")]
	[Space]
	public int maxHealth = 50;
	public float atkDamage = 15f;
	public float contactDamage = 5f;
	public float knockBackVal = 2f;
	[HideInInspector] public int currentHP;

	[Header("Defensive")]
	[Space]
	public int armor = 0;
	public float armorReduceFactor = .5f;
	[Range(0f, 1f)] public float knockBackRes = .2f;

	[Header("Others")]
	[Space]
	public float timeToDissolve = 5f;

	[HideInInspector] public bool grounded;

	private bool isDeath, canDissolve;
	private float fade = 1f;

	private void Awake()
	{
		hpBar = transform.Find("Enemy Health Bar").GetComponent<EnemyHPBar>();
		dmgTextPos = transform.Find("Damage Text Pos").transform;

		worldCanvas = GameObject.FindWithTag("Enemies World Canvas").transform;
		animator = GetComponent<Animator>();
		enemyMat = GetComponent<SpriteRenderer>().material;
		deathFx = transform.Find("Soul Release Effect").GetComponent<ParticleSystem>();
		
		rb2d = GetComponent<Rigidbody2D>();
		centerPoint = transform.Find("Center Point");
		groundCheck = transform.Find("Ground Check");
		GetBehaviour();
		
		player = GameObject.FindWithTag("Player").transform;
	}

	private void Start()
	{
		currentHP = maxHealth;

		hpBar.SetMaxHealth(maxHealth);
		hpBar.transform.SetParent(worldCanvas, true);
	}

	private void Update()
	{
		if (currentHP <= 0 && !isDeath)
		{
			StopAllCoroutines();
			Die();
			isDeath = true;
		}

		if (canDissolve && Time.time > timeToDissolve)
			Dissolve();

		if (isDeath && grounded)
			rb2d.simulated = false;
	}

	// Check if the enemy is grounded. Ignore for flying enemies.
	private void FixedUpdate()
	{
		grounded = false;

		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, .1f, whatIsGround);
		foreach (Collider2D collider in colliders)
			if (collider.CompareTag("Ground"))
				grounded = true;
	}

	public void TakeDamage(float dmg, float critDmgMul = 1f, float knockBackVal = 0f)
	{
		behaviour.spottingTimer = 0f;

		if (currentHP > 0)
		{
			// Reduce the damage by armor and then multiply with crit damage.
			float armorReducedDmg = dmg - armor * armorReduceFactor;
			int finalDmg = Mathf.RoundToInt(armorReducedDmg * critDmgMul);
			bool isCrit = critDmgMul != 1f;
			Color textColor;
			
			currentHP -= finalDmg;
			currentHP = Mathf.Clamp(currentHP, 0, maxHealth);

			hpBar.SetCurrentHealth(currentHP);

			animator.SetTrigger("Hit");

			// If there's critical hit, then popup text will be different.
			textColor = isCrit ? new Color(1f, .5f, 0f) : new Color(1f, .84f, .2f);
			DamageText.Generate(dmgTextPrefab, dmgTextPos.position, textColor, isCrit, finalDmg.ToString());

			StartCoroutine(BeingKnockedBack(knockBackVal));
		}
	}

	private void Die()
	{
		animator.SetBool("IsDeath", true);

		if (isFlyingEnemy)
			rb2d.gravityScale = 1f;

		behaviour.enabled = false;
		hpBar.gameObject.SetActive(false);
		
		canDissolve = true;
		timeToDissolve += Time.time;
	}

	private void Dissolve()
	{
		fade -= Time.deltaTime;
		enemyMat.SetFloat("_Fade", fade);

		if (fade < .4f && fade > 0f && !deathFx.isPlaying)
		{
			deathFx.Play();
			DropDeathLoots();
		}

		if (fade <= 0f && deathFx.isStopped)
		{
			fade = 0f;
			canDissolve = false;
			Destroy(hpBar.gameObject);
			Destroy(gameObject);
		}
	}

	private void DropDeathLoots()
	{
		void DropLoot(DeathLoot target, GameObject selectedPrefab)
		{
			// Set up the drop.
			ItemPickup droppedLoot = selectedPrefab.GetComponent<ItemPickup>();

			droppedLoot.itemPrefab = Instantiate(target.loot);
			droppedLoot.itemPrefab.quantity = target.quantity;
			droppedLoot.player = player;

			// Make the drop.
			GameObject droppedItemObj = Instantiate(selectedPrefab, transform.position, Quaternion.identity);

			droppedItemObj.name = target.loot.name;
			droppedItemObj.transform.SetParent(GameObject.Find("Items").transform);

			// Add force to the dropped item.
			Rigidbody2D rb2d = droppedItemObj.GetComponent<Rigidbody2D>();

			float randomX = UnityEngine.Random.Range(transform.position.x - 1f, transform.position.x + 1f);
			float randomY = UnityEngine.Random.Range(transform.position.y, transform.position.y + 1f);

			Vector3 randomPoint = new Vector3(randomX, randomY);
			Vector3 aimingDir = randomPoint - transform.position;

			Debug.Log(aimingDir);

			rb2d.AddForce(2f * aimingDir.normalized, ForceMode2D.Impulse);
		}

		for (int i = 0; i < loots.Length; i++)
		{
			if (loots[i].loot.itemName.Equals("Coin"))
			{
				DropLoot(loots[i], droppedCoinPrefab);
				continue;
			}

			if (loots[i].isGuaranteed)
			{
				DropLoot(loots[i], droppedItemPrefab);
				continue;
			}

			float rand = UnityEngine.Random.Range(0f, 100f);
			if (rand <= loots[i].dropChance)
				DropLoot(loots[i], droppedItemPrefab);
		}
	}

	private IEnumerator BeingKnockedBack(float knockBackValue)
	{
		// Make sure the object doesn't move.
		rb2d.velocity = Vector3.zero;
		behaviour.enabled = false;

		// The direction of the knock back.
		Vector2 knockbackDir = isFlyingEnemy ? transform.position - player.transform.position 
											: centerPoint.position - player.transform.position;
		float knockBackForce = knockBackValue * (1f - knockBackRes);  // Calculate the actual knock back value.

		rb2d.AddForce(knockBackForce * knockbackDir.normalized, ForceMode2D.Impulse);

		yield return new WaitForSeconds(.15f);

		behaviour.enabled = true;
	}

	private void GetBehaviour()
	{
		switch (enemyName.ToLower().Trim())
		{
			case "crab":
				behaviour = GetComponent<CrabBehaviour>();
				break;
			case "rat":
				behaviour = GetComponent<RatBehaviour>();
				break;
			case "spiked slime":
				behaviour = GetComponent<SpikedSlimeBehaviour>();
				break;
			case "bat":
				behaviour = GetComponent<BatBehaviour>();
				break;

			default:
				behaviour = null;
				Debug.LogWarning("Behaviour script for enemy " + name + " not found!!");
				break;
		}
	}
}

/// <summary>
/// Represents a loot dropped from enemies or generated in chests.
/// </summary>
[Serializable]
public struct DeathLoot
{
	public Item loot;

	[Space]
	public int quantity;
	[Range(.001f, 100f)] public float dropChance;
	public bool isGuaranteed;
}