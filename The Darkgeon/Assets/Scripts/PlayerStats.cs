using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private HealthBar hpBar;
	[SerializeField] private Transform dmgTextLoc;
	public GameObject dmgTextPrefab;
	public TextMeshPro dmgText;

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

	public void TakeDamage(int dmg)
	{
		lastDamagedTime = Time.time;
		currentHP -= (int)(dmg - armor * damageRecFactor);
		hpBar.SetCurrentHealth(currentHP);

		dmgText.color = Color.red;
		dmgText.text = "-" + (int)(dmg - armor * damageRecFactor);
		GameObject clone = Instantiate(dmgTextPrefab, transform.position, Quaternion.identity);
		clone.AddComponent<SelfDestruct>();
		clone.GetComponent<SelfDestruct>().animator = clone.GetComponent<Animator>();
		clone.SetActive(true);
	}
}
