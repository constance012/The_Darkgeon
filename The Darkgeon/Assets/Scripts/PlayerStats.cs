using UnityEngine;
using UnityEngine.Rendering;

public class PlayerStats : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private Material playerMat;
	[SerializeField] private HealthBar hpBar;
	[SerializeField] private Transform dmgTextLoc;
	public GameObject dmgTextPrefab;

	[SerializeField] private Animator animator;
	[SerializeField] private CharacterController2D controller;

	[SerializeField] private PlayerMovement moveScript;
	[SerializeField] private PlayerActions actionsScript;

	// Fields.
	[Header("Stats")]
	[Space]
	public int maxHP = 100;
	public int armor = 5;
	public float damageRecFactor = .5f;

	public float invincibilityTime = .5f;
	public float outOfCombatTime = 5f;
	public float lastDamagedTime = 0f;
	
	int currentHP;
	int regenRate = 1;
	float regenDelay = 2f;
	//private LocalKeyword isOutlineOn;

	private void Awake()
	{
		playerMat = GetComponent<SpriteRenderer>().material;
		hpBar = GameObject.Find("Health Bar").GetComponent<HealthBar>();
		dmgTextLoc = transform.Find("Damage Text Loc");
		animator = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
		moveScript = GetComponent<PlayerMovement>();
		actionsScript = GetComponent<PlayerActions>();
	}

	private void Start()
	{
		currentHP = maxHP;
		hpBar.SetMaxHealth(maxHP);

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

		if (currentHP <= 0)
		{
			animator.SetBool("IsDeath", true);
			controller.enabled = false;
			moveScript.enabled = false;
			actionsScript.enabled = false;
		}
	}

	public void TakeDamage(int dmg)
	{
		if (currentHP > 0)
		{
			lastDamagedTime = Time.time;

			int finalDmg = (int)(dmg - armor * damageRecFactor);
			currentHP -= finalDmg;
			currentHP = Mathf.Clamp(currentHP, 0, 100);
			hpBar.SetCurrentHealth(currentHP);

			animator.SetTrigger("TakingDamage");
			DamageText.Generate(dmgTextPrefab, dmgTextLoc.position, Color.red, finalDmg);
		}
	}

	private void Regenerate()
	{
		regenDelay -= Time.deltaTime;

		if (currentHP > 0 && regenDelay < 0f)
		{
			currentHP += regenRate;
			currentHP = Mathf.Clamp(currentHP, 0, 100);
			hpBar.SetCurrentHealth(currentHP);

			regenDelay = 2f;
		}
	}
}
