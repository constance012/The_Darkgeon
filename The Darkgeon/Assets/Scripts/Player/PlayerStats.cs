using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private Material playerMat;
	[SerializeField] private Transform dmgTextLoc;
	public GameObject dmgTextPrefab;

	[SerializeField] private Animator animator;
	[SerializeField] private Rigidbody2D rb2d;

	[Header("UI Elements")]
	[Space]
	[SerializeField] private Transform worldCanvas;
	public HealthBar hpBar;

	// Fields.
	[Header("Stats")]
	[Space]
	public int maxHP = 100;
	public int currentHP;
	public int armor = 5;
	public float damageRecFactor = .5f;
	
	[Space]
	public float invincibilityTime = .5f;
	public float outOfCombatTime = 5f;
	[HideInInspector] public float lastDamagedTime = 0f;
	[HideInInspector] public int knockBackForce = 5;
	
	[Space]	
	public KillSources killSource = KillSources.Unknown;
	public Vector3 respawnPos;
	[HideInInspector] public Transform attacker = null;  // Position of the attacker.

	int regenRate = 1;
	float regenDelay = 2f;

	//private LocalKeyword isOutlineOn;

	private void Awake()
	{
		playerMat = GetComponent<SpriteRenderer>().material;
		hpBar = GameObject.Find("Health Bar").GetComponent<HealthBar>();
		dmgTextLoc = transform.Find("Damage Text Loc");
		worldCanvas = GameObject.Find("World Canvas").transform;

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

	// Update is called once per frame
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

	public void TakeDamage(int dmg, Transform attacker = null, KillSources source = KillSources.Unknown)
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
			DamageText.Generate(dmgTextPrefab, worldCanvas, dmgTextLoc.position, Color.red, finalDmg);

			StartCoroutine(BeingKnockedBack());
		}
	}

	private void Regenerate()
	{
		regenDelay -= Time.deltaTime;

		if (currentHP > 0 && regenDelay < 0f)
		{
			currentHP += regenRate;
			currentHP = Mathf.Clamp(currentHP, 0, maxHP);
			hpBar.SetCurrentHealth(currentHP);

			regenDelay = 2f;
		}
	}

	private IEnumerator BeingKnockedBack()
	{
		if (attacker != null)
		{
			// Make sure the object doesn't move.
			transform.GetComponent<PlayerMovement>().enabled = false;

			Vector2 knockbackDir = new Vector2(Mathf.Sign(transform.position.x - attacker.position.x), 0f);
			rb2d.AddForce(3 * knockbackDir, ForceMode2D.Impulse);

			yield return new WaitForSeconds(.15f);

			rb2d.velocity = Vector3.zero;
			transform.GetComponent<PlayerMovement>().enabled = true;
		}
	}
}

public enum KillSources
{
	Unknown,
	Environment,
	Bat,
	Crab,
	Golem,
	ReinforcedGolem,
	Rat,
	Skull,
	SpikedSlime
}