using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private HealthBar hpBar;
	[SerializeField] private Transform dmgTextLoc;
	[SerializeField] private Animator animator;
	[SerializeField] private CharacterController2D controller;
	[SerializeField] private PlayerMovement moveScript;
	[SerializeField] private PlayerActions actionsScript;
	public GameObject dmgTextPrefab;

	// Fields.
	[Header("Stats")]
	[Space]
	public int maxHP = 100;
	public int armor = 5;
	public float damageRecFactor = .5f;
	public float invincibilityTime = .5f;
	
	public float lastDamagedTime = 0f;
	int currentHP;

	private void Awake()
	{
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
	}

	// Update is called once per frame
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.E) && Time.time - lastDamagedTime > invincibilityTime)
			TakeDamage(12);

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
			hpBar.SetCurrentHealth(currentHP);

			animator.SetTrigger("TakingDamage");
			DamageText.Generate(dmgTextPrefab, dmgTextLoc.position, Color.red, finalDmg);
		}
	}
}
