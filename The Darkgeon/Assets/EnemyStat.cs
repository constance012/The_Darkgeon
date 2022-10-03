using System.Collections;
using UnityEngine;

public class EnemyStat : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private EnemyHPBar hpBar;
	[SerializeField] private Transform worldCanvas;

	[Header("Stats")]
	[Space]
	public int maxHealth = 50;
	public int currentHP;
	public int armor = 5;
	public float dmgRecFactor = .25f;
	public float disposeTime = 5f;

	private void Awake()
	{
		hpBar = transform.Find("Enemy Health Bar").GetComponent<EnemyHPBar>();
		worldCanvas = GameObject.Find("World Canvas").transform;
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
			currentHP = Mathf.Clamp(currentHP, 0, 100);

			hpBar.SetCurrentHealth(currentHP);
		}
	}

	private IEnumerator Die()
	{
		// Play animation.
		gameObject.GetComponent<Rigidbody2D>().simulated = false;
		hpBar.gameObject.SetActive(false);

		yield return new WaitForSeconds(disposeTime);

		Destroy(hpBar.gameObject);
		Destroy(gameObject);
	}
}
