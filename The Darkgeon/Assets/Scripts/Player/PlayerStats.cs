using System.Collections;
using UnityEngine;
using CSTGames.CommonEnums;
using CSTGames.DataPersistence;

/// <summary>
/// Manages all the player's stats.
/// </summary>
public class PlayerStats : MonoBehaviour, ISaveDataTransceiver
{
	// References.
	[Header("References")]
	[Space]
	public Transform dmgTextLoc;
	public GameObject dmgTextPrefab;

	private Animator animator;
	private Rigidbody2D rb2d;

	[Header("UI Elements")]
	[Space]
	public HealthBar hpBar;

	// Fields.
	[Header("STATS")]
	[Space]

	[Header("Offensive")]
	[Space]
	public Stat maxHP;
	public Stat atkDamage;
	public float currentHP { get; set; }

	[Space]
	public Stat criticalChance;

	/// <summary>
	/// The bonus damage for crtical hit, in percentage.
	/// </summary>
	public Stat criticalDamage;
	public Stat knockBackVal;

	[Header("Defensive")]
	[Space]
	public Stat armor;
	public Stat knockBackRes;
	public Stat m_RegenRate;  // Health/sec.
	public Stat m_MoveSpeed;
	public float damageRecFactor = .5f;

	[Header("Timers")]
	[Space]
	public float timeBeforeRegen = 5f;
	public Stat m_InvincibilityTime;

	[HideInInspector] public float lastDamagedTime = 0f;
	[HideInInspector] public KillSources killSource = KillSources.Unknown;
	[HideInInspector] public Vector3 respawnPosition;
	[HideInInspector] public Transform attacker = null;  // Position of the attacker.

	// Private fields
	private float invincibilityTime = .5f;
	private float regenDelay;
	[HideInInspector] public bool canRegen = true;

	private void Awake()
	{
		hpBar = GameObject.Find("Health Bar Slider").GetComponent<HealthBar>();
		dmgTextLoc = transform.Find("Damage Text Loc");

		animator = GetComponent<Animator>();
		rb2d = GetComponent<Rigidbody2D>();
	}

	private void Start()
	{
		currentHP = maxHP.Value;
		hpBar.SetMaxHealth(maxHP.Value);
		respawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		
		invincibilityTime = m_InvincibilityTime.Value;
		regenDelay = 1 / m_RegenRate.Value;

		EquipmentManager.instance.onEquipmentChanged.AddListener(OnEquipmentChanged);
	}

	private void Update()
	{
		if (invincibilityTime < 0f && Physics2D.GetIgnoreLayerCollision(3, 8) && !CharacterController2D.m_IsDashing)
			Physics2D.IgnoreLayerCollision(3, 8, false);

		if (invincibilityTime > 0f)
			invincibilityTime -= Time.deltaTime;

		if (Time.time - lastDamagedTime > timeBeforeRegen)
			Regenerate();
	}

	public void TakeDamage(float dmg, float knockBackVal = 0f, Transform attacker = null, KillSources source = KillSources.Unknown)
	{
		this.attacker = attacker;  // The transform of the attacker, default is null.
		killSource = source;  // The kill source, default is unknown.

		// Only receive damage if the player is alive and runs out of invincibily time.
		if (currentHP > 0 && invincibilityTime <= 0f)
		{
			int finalDmg = Mathf.RoundToInt(dmg - armor.Value * damageRecFactor);
			currentHP -= finalDmg;
			currentHP = Mathf.Clamp(currentHP, 0f, maxHP.Value);
			
			hpBar.SetCurrentHealth(currentHP);

			animator.SetTrigger("TakingDamage");
			DamageText.Generate(dmgTextPrefab, dmgTextLoc.position, finalDmg.ToString());

			StartCoroutine(BeingKnockedBack(knockBackVal));
			Physics2D.IgnoreLayerCollision(3, 8, true);
			
			lastDamagedTime = Time.time;
			invincibilityTime = m_InvincibilityTime.Value;
		}
	}

	public void Heal(int amount)
	{
		if (currentHP > 0)
		{
			currentHP += amount;
			currentHP = Mathf.Clamp(currentHP, 0f, maxHP.Value);
			
			hpBar.SetCurrentHealth(currentHP);

			DamageText.Generate(dmgTextPrefab, dmgTextLoc.position, Color.green, false, amount.ToString());
		}
	}

	/// <summary>
	/// Decide whether or not this attack is a critical hit.
	/// </summary>
	/// <returns></returns>
	public bool IsCriticalStrike() => Random.Range(0f, 100f) <= criticalChance.Value;

	public void LoadData(GameData gameData)
	{
		transform.position = gameData.playerData.playerPosition;
	}

	public void SaveData(GameData gameData)
	{
		gameData.playerData.playerPosition = transform.position;
	}

	private void Regenerate()
	{
		regenDelay -= Time.deltaTime;

		if (currentHP > 0 && regenDelay < 0f && canRegen)
		{
			currentHP += 1f;
			currentHP = Mathf.Clamp(currentHP, 0f, maxHP.Value);
			hpBar.SetCurrentHealth(currentHP);

			regenDelay = 1 / m_RegenRate.Value;
		}
	}

	private IEnumerator BeingKnockedBack(float knockBackValue)
	{
		if (attacker != null)
		{
			// Make sure the object doesn't move.
			rb2d.velocity = Vector3.zero;
			transform.GetComponent<PlayerMovement>().enabled = false;

			Vector2 knockbackDir = transform.position - attacker.position;  // The direction of the knock back.
			float knockbackForce = knockBackValue * (1f - knockBackRes.Value);  // Calculate the actual knock back value.
			
			rb2d.AddForce(knockbackForce * knockbackDir.normalized, ForceMode2D.Impulse);

			yield return new WaitForSeconds(.3f);

			transform.GetComponent<PlayerMovement>().enabled = true;
		}
	}

	private void OnEquipmentChanged(Equipment newEquipment, Equipment oldEquipment)
	{
		if (newEquipment != null)
		{
			Debug.Log($"Equip: {newEquipment.itemName}.");

			armor.AddModifier(newEquipment.armor);
			maxHP.AddModifier(newEquipment.maxHP);
			m_RegenRate.AddModifier(newEquipment.regenerateRate);
			m_InvincibilityTime.AddModifier(newEquipment.invincibilityTime);

			atkDamage.AddModifier(newEquipment.damagePerc);
			criticalChance.AddModifier(newEquipment.criticalChancePerc);
			criticalDamage.AddModifier(newEquipment.criticalDamagePerc);

			m_MoveSpeed.AddModifier(newEquipment.movementSpeedPerc);
			knockBackVal.AddModifier(newEquipment.knockbackPerc);
			knockBackRes.AddModifier(newEquipment.knockbackResistancePerc);
		}

		if (oldEquipment != null)
		{
			Debug.Log($"Unequip: {oldEquipment.itemName}.");

			armor.RemoveModifier(oldEquipment.armor);
			maxHP.RemoveModifier(oldEquipment.maxHP);
			m_RegenRate.RemoveModifier(oldEquipment.regenerateRate);
			m_InvincibilityTime.RemoveModifier(oldEquipment.invincibilityTime);

			atkDamage.RemoveModifier(oldEquipment.damagePerc);
			criticalChance.RemoveModifier(oldEquipment.criticalChancePerc);
			criticalDamage.RemoveModifier(oldEquipment.criticalDamagePerc);

			m_MoveSpeed.RemoveModifier(oldEquipment.movementSpeedPerc);
			knockBackVal.RemoveModifier(oldEquipment.knockbackPerc);
			knockBackRes.RemoveModifier(oldEquipment.knockbackResistancePerc);
		}

		// Update the health bar limit.
		hpBar.SetMaxHealth(maxHP.Value, false);
		regenDelay = 1 / m_RegenRate.Value;
	}
}