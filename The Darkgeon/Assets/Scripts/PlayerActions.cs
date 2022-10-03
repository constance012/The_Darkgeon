using UnityEngine;

public class PlayerActions : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private Animator animator;

	// Fields.
	[Header("Player Attack")]
	[Space]

	[SerializeField] private Transform atkPoint;
	[SerializeField] private LayerMask enemyLayers;
	public float atkRange = .5f;

	public static int clickCount = 0;
	float lastClickTime = 0f;
	float comboDelay = 0.5f;  // The cooldown between each combo is 1 second.

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash-Attack"))
			animator.SetBool("DashAtk", false);
		
		// Check if there is enough time for the next combo to begin.
		if (Time.time - lastClickTime > comboDelay)
			clickCount = 0;

		if (Input.GetMouseButtonDown(0))
			Attack();
	}
	
	private void Attack()
	{
		lastClickTime = Time.time;
		Debug.Log(lastClickTime);
		clickCount++;

		bool canDoAtk1 = animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("Run");
		bool canDoAtk2 = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1");
		bool isAtk1 = false;
		bool isAtk2 = false;

		Collider2D[] hitList = Physics2D.OverlapCircleAll(atkPoint.position, atkRange, enemyLayers);

		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
			animator.SetBool("DashAtk", true);

		if (clickCount == 1 && canDoAtk1)
		{
			animator.SetTrigger("Atk1");
			isAtk1 = true;
		}

		clickCount = Mathf.Clamp(clickCount, 0, 3);

		if (clickCount == 2 && canDoAtk2)
		{
			animator.SetTrigger("Atk2");
			isAtk2 = true;
		}

		foreach (Collider2D enemy in hitList)
		{
			if (isAtk1)
				enemy.GetComponent<EnemyStat>().TakeDamage(15);
			else if (isAtk2)
				enemy.GetComponent<EnemyStat>().TakeDamage(12);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (atkPoint == null)
			return;

		Gizmos.DrawWireSphere(atkPoint.position, atkRange);
	}
}
