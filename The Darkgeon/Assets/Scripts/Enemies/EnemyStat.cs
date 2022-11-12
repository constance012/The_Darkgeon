using System.Collections;
using UnityEngine;

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

	[SerializeField] private Transform centerPoint;
	[SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask whatIsGround;

	[SerializeField] private Animator animator;
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Material enemyMat;
	[SerializeField] private ParticleSystem deathFx;

	[SerializeField] private PlayerStats player;

	public GameObject dmgTextPrefab;
	public MonoBehaviour behaviour;

	[Header("Stats")]
	[Space]
	[SerializeField] private new string name;
	public int maxHealth = 50;
	public int currentHP;
	public int armor = 0;
	public float atkDamage = 15f;
	public float knockBackVal = 2f;
	public float dmgRecFactor = .5f;
	public float timeToDissolve = 5f;
	[Range(0f, 1f)] public float knockBackRes = .2f;

	[HideInInspector] public bool grounded;

	private bool isDeath, isDissolving;
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

		if (isDissolving && Time.time > timeToDissolve)
		{
			if (fade < .7f && !deathFx.isPlaying)
				deathFx.Play();
			Dissolve();
		}

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

	public void TakeDamage(float dmg, float knockBackVal = 0f)
	{
		ResetSpottingTimer();

		if (currentHP > 0)
		{
			int finalDmg = Mathf.RoundToInt(dmg - armor * dmgRecFactor);
			
			currentHP -= finalDmg;
			currentHP = Mathf.Clamp(currentHP, 0, maxHealth);

			hpBar.SetCurrentHealth(currentHP);

			animator.SetTrigger("Hit");
			DamageText.Generate(dmgTextPrefab, dmgTextPos.position, Color.yellow, finalDmg.ToString());

			StartCoroutine(BeingKnockedBack(knockBackVal));
		}
	}

	private void Die()
	{
		animator.SetBool("IsDeath", true);

		behaviour.enabled = false;
		hpBar.gameObject.SetActive(false);
		
		isDissolving = true;
		timeToDissolve += Time.time;
	}

	private void Dissolve()
	{
		fade -= Time.deltaTime;
		enemyMat.SetFloat("_Fade", fade);

		if (fade <= 0f)
		{
			fade = 0f;
			isDissolving = false;
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
		switch (name)
		{
			case "Crab":
				behaviour = GetComponent<CrabBehaviour>();
				break;
			case "Rat":
				behaviour = GetComponent<RatBehaviour>();
				break;
			default:
				behaviour = null;
				Debug.LogWarning("Behaviour script for enemy " + name + " not found!!");
				break;
		}
	}

	private void ResetSpottingTimer()
	{
		switch (name)
		{
			case "Crab":
				GetComponent<CrabBehaviour>().spottingTimer = 0f;
				break;
			case "Rat":
				GetComponent<RatBehaviour>().spottingTimer = 0f;
				break;
			default:
				behaviour = null;
				Debug.LogWarning("Behaviour script for enemy " + name + " not found!!");
				break;
		}
	}
}
