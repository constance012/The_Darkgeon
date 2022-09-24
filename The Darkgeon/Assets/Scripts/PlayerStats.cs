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
	[SerializeField] private CharacterController2D controller;

	[SerializeField] private PlayerMovement moveScript;
	[SerializeField] private PlayerActions actionsScript;

	[SerializeField] private GameObject deathPanel;

	[Header("UI Elements")]
	[Space]
	[SerializeField] private HealthBar hpBar;
	[SerializeField] private TextMeshProUGUI deathMessageText;
	[SerializeField] private TextMeshProUGUI killSourceText;
	[SerializeField] private Transform worldCanvas;

	// Fields.
	[Header("Stats")]
	[Space]
	public int maxHP = 100;
	public int currentHP;
	public int armor = 5;
	public float damageRecFactor = .5f;

	public float invincibilityTime = .5f;
	public float outOfCombatTime = 5f;
	public float lastDamagedTime = 0f;

	public KillSources killSource = KillSources.Unknown;

	int regenRate = 1;
	float regenDelay = 2f;
	bool isDeath = false;
	Vector3 respawnPos;

	private string[] deathMessages = new string[] {"YOUR SOUL HAS BEEN CONSUMED", "YOUR HEAD WAS DETACHED", "YOUR FACE WAS RIPPED OFF", "YOUR BODY WAS EVISCERATED", "YOUR FATE WAS SHATTERED"};
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

		deathPanel = GameObject.Find("Death Message");
		deathMessageText = deathPanel.transform.Find("Message").GetComponent<TextMeshProUGUI>();
		killSourceText = deathPanel.transform.Find("Kill Source").GetComponent<TextMeshProUGUI>();
		worldCanvas = GameObject.Find("World Canvas").transform;
	}

	private void Start()
	{
		deathPanel.SetActive(false);
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
			killSource = KillSources.Unknown;
			//playerMat.SetKeyword(isOutlineOn, !playerMat.IsKeywordEnabled(isOutlineOn));
			playerMat.SetFloat("_Thickness", playerMat.GetFloat("_Thickness") > 0f ? 0f : .0013f);
		}

		if (Time.time - lastDamagedTime > outOfCombatTime)
			Regenerate();

		if (Time.time - lastDamagedTime > .8f)
			hpBar.PerformEffect();

		if (currentHP <= 0 && !isDeath)
		{
			SetDeathMessage();
			Death();
			isDeath = true;
		}
	}

	public void TakeDamage(int dmg)
	{
		if (currentHP > 0)
		{
			lastDamagedTime = Time.time;

			int finalDmg = Mathf.RoundToInt(dmg - armor * damageRecFactor);
			currentHP -= finalDmg;
			currentHP = Mathf.Clamp(currentHP, 0, 100);
			hpBar.SetCurrentHealth(currentHP);

			animator.SetTrigger("TakingDamage");
			DamageText.Generate(dmgTextPrefab, worldCanvas, dmgTextLoc.position, Color.red, finalDmg);
		}
	}

	public void Respawn()
	{
		deathPanel.SetActive(false);
		currentHP = maxHP;
		hpBar.SetMaxHealth(maxHP);

		controller.enabled = true;
		moveScript.enabled = true;
		actionsScript.enabled = true;

		animator.SetTrigger("Respawn");
		animator.SetBool("IsDeath", false);

		transform.position = respawnPos;
		isDeath = false;
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

	private void Death()
	{
		animator.SetBool("IsDeath", true);
		deathPanel.SetActive(true);

		controller.enabled = false;
		moveScript.enabled = false;
		actionsScript.enabled = false;
	}

	private void SetDeathMessage()
	{
		int randomIndex = Random.Range(0, deathMessages.Length);
		deathMessageText.text = deathMessages[randomIndex];

		switch (killSource)
		{
			case KillSources.Environment:
				killSourceText.text = "Kill By: Environment";
				break;
			case KillSources.Bat:
				killSourceText.text = "Kill By: Bat";
				break;
			case KillSources.Crab:
				killSourceText.text = "Kill By: Crab";
				break;
			case KillSources.Golem:
				killSourceText.text = "Kill By: Golem";
				break;
			case KillSources.ReinforcedGolem:
				killSourceText.text = "Kill By: Reinforced Golem";
				break;
			case KillSources.Rat:
				killSourceText.text = "Kill By: Rat";
				break;
			case KillSources.Skull:
				killSourceText.text = "Kill By: Floating Skull";
				break;
			case KillSources.SpikedSlime:
				killSourceText.text = "Kill By: Spiked Slime";
				break;
			default:
				killSourceText.text = "Kill By: Unknown";
				break;
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