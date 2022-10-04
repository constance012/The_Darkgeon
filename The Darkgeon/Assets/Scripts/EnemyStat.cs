using System.Collections;
using UnityEngine;

public class EnemyStat : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private EnemyHPBar hpBar;
	[SerializeField] private Transform dmgTextPos;

	[SerializeField] private Transform worldCanvas;
	[SerializeField] private Animator animator;

	public GameObject dmgTextPrefab;

	[Header("Stats")]
	[Space]
	public int maxHealth = 50;
	public int currentHP;
	public int armor = 0;
	public float dmgRecFactor = .25f;
	public float disposeTime = 5f;

	private void Awake()
	{
		hpBar = transform.Find("Enemy Health Bar").GetComponent<EnemyHPBar>();
		dmgTextPos = transform.Find("Damage Text Pos").transform;

		worldCanvas = GameObject.Find("World Canvas").transform;
		animator = GetComponent<Animator>();
	}

	private void Start()
	{
		currentHP = maxHealth;

		hpBar.SetMaxHealth(maxHealth);
		hpBar.transform.SetParent(worldCanvas, true);
	}

	private void Update()
	{
		if (currentHP <= 0)
			StartCoroutine(Die());
	}

	public void TakeDamage(int dmg)
	{
		if (currentHP > 0)
		{
			int finalDmg = Mathf.RoundToInt(dmg - armor * dmgRecFactor);
			
			currentHP -= finalDmg;
			currentHP = Mathf.Clamp(currentHP, 0, maxHealth);

			hpBar.SetCurrentHealth(currentHP);

			animator.SetTrigger("Hit");
			DamageText.Generate(dmgTextPrefab, worldCanvas, dmgTextPos.position, Color.yellow, finalDmg);
		}
	}

	private IEnumerator Die()
	{
		animator.SetBool("IsDeath", true);

		gameObject.GetComponent<Rigidbody2D>().simulated = false;
		hpBar.gameObject.SetActive(false);

		yield return new WaitForSeconds(disposeTime);

		Destroy(hpBar.gameObject);
		Destroy(gameObject);
	}
}
