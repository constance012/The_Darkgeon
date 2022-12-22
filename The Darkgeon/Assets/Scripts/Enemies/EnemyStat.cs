using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all the enemy's stats.
/// </summary>
public class EnemyStat : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private EnemyHPBar hpBar;
	public Transform dmgTextPos;
	public Transform worldCanvas;

	[Space]
	[SerializeField] private Transform centerPoint;
	[SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask whatIsGround;

	[Space]
	[SerializeField] private Animator animator;
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Material enemyMat;
	[SerializeField] private ParticleSystem deathFx;

	[Space]
	[SerializeField] private PlayerStats player;

	public GameObject dmgTextPrefab;
	public MonoBehaviour behaviour;

	[Header("STATS")]
	[Space]
	[SerializeField] private new string name;

	[Header("Offensive")]
	[Space]
	public int maxHealth = 50;
	public float atkDamage = 15f;
	public float contactDamage = 5f;
	[HideInInspector] public int currentHP;

	[Header("Defensive")]
	[Space]
	public int armor = 0;
	public float knockBackVal = 2f;
	public float armorReduceFactor = .5f;
	[Range(0f, 1f)] public float knockBackRes = .2f;

	[Header("Others")]
	[Space]
	public float timeToDissolve = 5f;

	[HideInInspector] public bool grounded;

	private KillSources source;
	private bool isDeath, canDissolve;
	private float fade = 1f;

	private void Awake()
	{
		hpBar = transform.Find("Enemy Health Bar").GetComponent<EnemyHPBar>();
		dmgTextPos = transform.Find("Damage Text Pos").transform;

		worldCanvas = GameObject.Find("World Canvas").transform;
		animator = GetComponent<Animator>();
		rb2d = GetComponent<Rigidbody2D>();
		enemyMat = GetComponent<SpriteRenderer>().material;
		deathFx = transform.Find("Soul Release Effect").GetComponent<ParticleSystem>();
		
		centerPoint = transform.Find("Center Point");
		groundCheck = transform.Find("Ground Check");
		GetBehaviour();
		
		player = GameObject.Find("Player").GetComponent<PlayerStats>();
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
			Dissolve();
			isDeath = true;
		}

		if (canDissolve && Time.time > timeToDissolve)
			Dissolve();

		if (isDeath && grounded)
			rb2d.simulated = false;
	}

	// Check if the enemy is grounded.
	private void FixedUpdate()
	{
		grounded = false;

		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, .1f, whatIsGround);
		foreach (Collider2D collider in colliders)
			if (collider.CompareTag("Ground"))
				grounded = true;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//Debug.Log("Contacted with the something.");

		if (collision.collider.CompareTag("Player"))
		{
			// Engage immediately if contacted with the player.
			EngageThePlayer();

			// Deal contact damage if needed.
			if (contactDamage > 0f && Time.time - player.lastDamagedTime > player.invincibilityTime)
				player.TakeDamage(contactDamage, knockBackVal, this.transform, source);
		}
	}

	public void TakeDamage(float dmg, float critDmgMul = 1f, float knockBackVal = 0f)
	{
		EngageThePlayer();

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
			deathFx.Play();

		if (fade <= 0f && deathFx.isStopped)
		{
			fade = 0f;
			canDissolve = false;
			Destroy(hpBar.gameObject);
			Destroy(gameObject);
		}
	}

	private IEnumerator BeingKnockedBack(float knockBackValue)
	{
		// Make sure the object doesn't move.
		rb2d.velocity = Vector3.zero;
		behaviour.enabled = false;

		float knockbackDir = Mathf.Sign(centerPoint.position.x - player.transform.position.x);  // The direction of the knock back.
		knockBackValue = knockBackValue * (1f - knockBackRes) * knockbackDir;  // Calculate the actual knock back value.
		rb2d.velocity = new Vector2(knockBackValue, 0f);

		yield return new WaitForSeconds(.15f);

		behaviour.enabled = true;
	}

	private void GetBehaviour()
	{
		string enemyName = name.ToLower().Trim();

		switch (enemyName)
		{
			case "crab":
				behaviour = GetComponent<CrabBehaviour>();
				source = KillSources.Crab;
				break;
			case "rat":
				behaviour = GetComponent<RatBehaviour>();
				source = KillSources.Rat;
				break;
			case "spiked slime":
				behaviour = GetComponent<SpikedSlimeBehaviour>();
				source = KillSources.SpikedSlime;
				break;
			
			default:
				behaviour = null;
				source = KillSources.Unknown;
				Debug.LogWarning("Behaviour script for enemy " + name + " not found!!");
				break;
		}
	}

	private void EngageThePlayer()
	{
		string enemyName = name.ToLower().Trim();

		switch (enemyName)
		{
			case "crab":
				GetComponent<CrabBehaviour>().spottingTimer = 0f;
				break;
			
			case "rat":
				GetComponent<RatBehaviour>().spottingTimer = 0f;
				break;
			
			case "spiked slime":
				GetComponent<SpikedSlimeBehaviour>().spottingTimer = 0f;
				break;
			
			default:
				Debug.LogWarning("Behaviour script for enemy " + name + " not found!!");
				break;
		}
	}
}
