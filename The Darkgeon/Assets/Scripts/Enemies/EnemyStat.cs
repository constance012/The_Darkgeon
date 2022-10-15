using System.Collections;
using UnityEngine;

public class EnemyStat : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private EnemyHPBar hpBar;
	[SerializeField] private Transform dmgTextPos;

	[SerializeField] private Transform centerPoint;
	[SerializeField] private Transform groundCheck;
	[SerializeField] private CrabBehaviour behaviour;
	[SerializeField] private LayerMask whatIsGround;

	[SerializeField] private Transform worldCanvas;
	[SerializeField] private Animator animator;
	[SerializeField] private Rigidbody2D rb2d;

	[SerializeField] private PlayerStats player;

	public GameObject dmgTextPrefab;

	[Header("Stats")]
	[Space]
	public int maxHealth = 50;
	public int currentHP;
	public int armor = 0;
	public float atkDamage = 15f;
	public float dmgRecFactor = .5f;
	public float disposeTime = 5f;
	[Range(0f, 1f)] public float knockBackRes = .2f;

	[HideInInspector] public float knockBackVal = 2f;
	[HideInInspector] public bool grounded;

	bool isDeath;

	private void Awake()
	{
		hpBar = transform.Find("Enemy Health Bar").GetComponent<EnemyHPBar>();
		dmgTextPos = transform.Find("Damage Text Pos").transform;

		worldCanvas = GameObject.Find("World Canvas").transform;
		animator = GetComponent<Animator>();
		rb2d = GetComponent<Rigidbody2D>();
		behaviour = GetComponent<CrabBehaviour>();
		centerPoint = transform.Find("Center Point");
		groundCheck = transform.Find("Ground Check");

		player = GameObject.Find("Player").GetComponent<PlayerStats>();
	}

	private void Start()
	{
		currentHP = maxHealth;

		hpBar.SetMaxHealth(maxHealth);
		hpBar.transform.SetParent(worldCanvas, true);
	}

	private void Update()
	{
		if (currentHP <= 0 && !isDeath)
		{
			StopAllCoroutines();
			StartCoroutine(Die());
			isDeath = true;
		}

		if (isDeath && grounded)
			rb2d.simulated = false;
	}

	// Check if the enemy is grounded.
	private void FixedUpdate()
	{
		grounded = false;

		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, .1f, whatIsGround);
		foreach (Collider2D collider in colliders)
			if (collider.CompareTag("Ground"))
				grounded = true;
	}

	public void TakeDamage(float dmg, float knockBackVal = 0f)
	{
		behaviour.spottingTimer = 0f;

		if (currentHP > 0)
		{
			int finalDmg = Mathf.RoundToInt(dmg - armor * dmgRecFactor);
			
			currentHP -= finalDmg;
			currentHP = Mathf.Clamp(currentHP, 0, maxHealth);

			hpBar.SetCurrentHealth(currentHP);

			animator.SetTrigger("Hit");
			DamageText.Generate(dmgTextPrefab, worldCanvas, dmgTextPos.position, Color.yellow, finalDmg);

			StartCoroutine(BeingKnockedBack(knockBackVal));
		}
	}

	private IEnumerator Die()
	{
		animator.SetBool("IsDeath", true);

		behaviour.enabled = false;
		hpBar.gameObject.SetActive(false);

		yield return new WaitForSeconds(disposeTime);

		Destroy(hpBar.gameObject);
		Destroy(gameObject);
	}

	private IEnumerator BeingKnockedBack(float knockBackValue)
	{
		// Make sure the object doesn't move.
		rb2d.velocity = Vector3.zero;
		behaviour.enabled = false;

		float knockbackDir = Mathf.Sign(centerPoint.position.x - player.transform.position.x);  // The direction of the knock back.
		knockBackValue = knockBackValue * (1f - knockBackRes) * knockbackDir;  // Calculate the actual knock back value.
		rb2d.velocity = new Vector2(knockBackValue, 0f);

		yield return new WaitForSeconds(.15f);

		behaviour.enabled = true;
	}
}
