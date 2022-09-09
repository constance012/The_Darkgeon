using UnityEngine;

public class PlayerStats : MonoBehaviour
{
	// References.
	[SerializeField] private HealthBar hpBar;

	// Fields.
	[Header("Stats")]
	[Space]
	public int maxHP = 100;
	public int armor = 5;
	public float damageRecFactor = .5f;
	public float invincibilityTime = .5f;
	
	float lastDamagedTime = 0f;
	int currentHP;

	private void Awake()
	{
		hpBar = GameObject.Find("Health Bar").GetComponent<HealthBar>();
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
	}

	void TakeDamage(int dmg)
	{
		lastDamagedTime = Time.time;
		currentHP -= (int)(dmg - armor * damageRecFactor);
		hpBar.SetCurrentHealth(currentHP);
	}
}
