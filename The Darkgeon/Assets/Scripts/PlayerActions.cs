using UnityEngine;

public class PlayerActions : MonoBehaviour
{
	// Reference.
	[SerializeField] private Animator animator;

	// Fields.
	public float cooldownTime = 0.7f;
	private float nextFireTime = 0f;
	public static int clickCount = 0;
	float lastClickTime = 0f;
	float comboDelay = 0.7f;  // The cooldown between each combo is 1 second.

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		// Check if there is enough time for the next combo to begin.
		if (Time.time - lastClickTime > comboDelay)
			clickCount = 0;

		if (Time.time > nextFireTime)
			if (Input.GetMouseButtonDown(0))
				OnClick();
	}
	
	private void OnClick()
	{
		lastClickTime = Time.time;
		Debug.Log(lastClickTime);
		clickCount++;

		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
		{
			animator.SetTrigger("DashAtk");
		}

		if (clickCount == 1 && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			animator.SetTrigger("Atk1");

		clickCount = Mathf.Clamp(clickCount, 0, 3);

		if (clickCount == 2 && animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
		{
			animator.SetTrigger("Atk2");
		}
	}
}
