using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

/// <summary>
/// Manages all the player's stats.
/// </summary>
public class PlayerStats : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private Material playerMat;
	public Transform dmgTextLoc;
	public GameObject dmgTextPrefab;

	[SerializeField] private Animator animator;
	[SerializeField] private Rigidbody2D rb2d;

	[Header("UI Elements")]
	[Space]
	public HealthBar hpBar;

	// Fields.
	[Header("STATS")]
	[Space]
	[Header("Offensive")]
	[Space]
	public int maxHP = 100;
	public float atkDamage = 10f;
	[HideInInspector] public int currentHP;

	[Space]
	[Range(0f, 100f)] public float criticalChance;
	/// <summary>
	/// The bonus damage for crtical hit, in percentage.
	/// </summary>
	public float criticalDamage;

	[Header("Defensive")]
	[Space]
	public int armor = 5;
	public float damageRecFactor = .5f;
	[Range(0f, 1f)] public float knockBackRes = .2f;

	[Header("Timers")]
	[Space]
	public float invincibilityTime = .5f;
	public float outOfCombatTime = 5f;
	
	[HideInInspector] public float lastDamagedTime = 0f;
	[HideInInspector] public float knockBackVal = 1.5f;
	[HideInInspector] public KillSources killSource = KillSources.Unknown;
	[HideInInspector] public Vector3 respawnPos;
	[HideInInspector] public Transform attacker = null;  // Position of the attacker.

	// Regenerate health stats.
	private int regenRate = 1;
	private float regenDelay = 2f;
	[HideInInspector] public bool canRegen = true;

	//private LocalKeyword isOutlineOn;

	private void Awake()
	{
		playerMat = GetComponent<SpriteRenderer>().material;
		hpBar = GameObject.Find("Health Bar").GetComponent<HealthBar>();
		dmgTextLoc = transform.Find("Damage Text Loc");

		animator = GetComponent<Animator>();
		rb2d = GetComponent<Rigidbody2D>();
	}

	private void Start()
	{
		currentHP = maxHP;
		hpBar.SetMaxHealth(maxHP);
		respawnPos = transform.position;

		//var shader = playerMat.shader;
		//isOutlineOn = new LocalKeyword(shader, "_IS_OUTLINE_ON");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.E) && Time.time - lastDamagedTime > invincibilityTime)
		{
			TakeDamage(12);
			//playerMat.SetKeyword(isOutlineOn, !playerMat.IsKeywordEnabled(isOutlineOn));
			playerMat.SetFloat("_Thickness", playerMat.GetFloat("_Thickness") > 0f ? 0f : .0013f);
		}

		if (Time.time - lastDamagedTime > outOfCombatTime)
			Regenerate();

		if (Time.time - lastDamagedTime > .8f)
			hpBar.PerformEffect();
	}

	public void TakeDamage(float dmg, float knockBackVal = 0f, Transform attacker = null, KillSources source = KillSources.Unknown)
	{
		this.attacker = attacker;  // The transform of the attacker, default is null.
		killSource = source;  // The kill source, default is unknown.

		if (currentHP > 0)
		{
			lastDamagedTime = Time.time;

			int finalDmg = Mathf.RoundToInt(dmg - armor * damageRecFactor);
			currentHP -= finalDmg;
			currentHP = Mathf.Clamp(currentHP, 0, maxHP);
			hpBar.SetCurrentHealth(currentHP);

			animator.SetTrigger("TakingDamage");
			DamageText.Generate(dmgTextPrefab, dmgTextLoc.position, finalDmg.ToString());

			StartCoroutine(BeingKnockedBack(knockBackVal));
		}
	}

	private void Regenerate()
	{
		regenDelay -= Time.deltaTime;

		if (currentHP > 0 && regenDelay < 0f && canRegen)
		{
			currentHP += regenRate;
			currentHP = Mathf.Clamp(currentHP, 0, maxHP);
			hpBar.SetCurrentHealth(currentHP);

			regenDelay = 2f;
		}
	}

	public bool IsCriticalStrike()
	{
		float chance = Mathf.Round(criticalChance);
		float rand = Random.Range(0, 101);
		
		if (rand <= chance)
			return true;
		else
			return false;
	}

	private IEnumerator BeingKnockedBack(float knockBackValue)
	{
		if (attacker != null)
		{
			// Make sure the object doesn't move.
			rb2d.velocity = Vector3.zero;
			transform.GetComponent<PlayerMovement>().enabled = false;

			float knockbackDir = Mathf.Sign(transform.position.x - attacker.position.x);  // The direction of the knock back.
			knockBackValue = knockBackValue * (1f - knockBackRes) * knockbackDir;  // Calculate the actual knock back value.
			rb2d.velocity = new Vector2(knockBackValue, 0f);

			yield return new WaitForSeconds(.15f);

			transform.GetComponent<PlayerMovement>().enabled = true;
		}
	}
}