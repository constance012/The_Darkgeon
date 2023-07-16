using UnityEngine;
using UnityEngine.EventSystems;

public class MenuRatBehavior : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private Transform edgeCheck;
	[SerializeField] private LayerMask whatIsGround;

	[Space]
	[SerializeField] private Animator animator;
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private Material enemyMat;
	[SerializeField] private ParticleSystem deathFx;

	[Header("Cursors")]
	[Space]
	[SerializeField] private Texture2D swordCursor;

	[Header("Others")]
	[Space]
	public float timeToDissolve = 5f;
	public float walkSpeed;
	public float checkRadius;

	// Private fields.
	private bool isPatrol = true;
	private bool mustFlip, isTouchingWall;
	private bool canDissolve;
	private bool facingRight = true;
	private float fade = 1f;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		rb2d = GetComponent<Rigidbody2D>();
		enemyMat = GetComponent<SpriteRenderer>().material;
		deathFx = transform.GetComponentInChildren<ParticleSystem>("Soul Release Effect");
	}

	private void Update()
	{
		animator.SetFloat("Speed", Mathf.Abs(rb2d.velocity.x));

		if (isPatrol)
			Patrol();
		
		if (canDissolve && Time.time > timeToDissolve)
			Dissolve();
	}

	private void FixedUpdate()
	{
		if (isPatrol)
			mustFlip = !Physics2D.OverlapCircle(edgeCheck.position, checkRadius, whatIsGround);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Ground"))
			isTouchingWall = true;
	}

	private void OnMouseDown()
	{
		Die();
		MainMenu.isRatAlive = false;
	}

	private void OnMouseEnter()
	{
		Cursor.SetCursor(swordCursor, new Vector2(5, 2), CursorMode.Auto);
	}

	private void OnMouseExit()
	{
		Cursor.SetCursor(null, new Vector2(10, 5), CursorMode.Auto);
	}

	private void Patrol()
	{
		if (mustFlip || isTouchingWall)
		{
			Flip();
			isTouchingWall = false;
		}

		rb2d.velocity = facingRight ? new Vector2(walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y)
									: new Vector2(-walkSpeed * Time.fixedDeltaTime, rb2d.velocity.y);
	}

	private void Flip()
	{
		isPatrol = false;

		transform.localScale = new Vector2(transform.localScale.x * -1f, transform.localScale.y);
		facingRight = !facingRight;

		isPatrol = true;
	}

	public void Die()
	{
		animator.SetTrigger("Hit");
		animator.SetBool("IsDeath", true);

		isPatrol = false;
		canDissolve = true;
		timeToDissolve += Time.time;
	}

	private void Dissolve()
	{
		fade -= Time.deltaTime;
		enemyMat.SetFloat("_Fade", fade);

		if (fade < .4f && fade > 0f && !deathFx.isPlaying)
			deathFx.Play();

		if (fade <= 0f && deathFx.isStopped)
		{
			fade = 0f;
			canDissolve = false;
			Destroy(gameObject);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (edgeCheck == null)
			return;

		Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);
	}
}
