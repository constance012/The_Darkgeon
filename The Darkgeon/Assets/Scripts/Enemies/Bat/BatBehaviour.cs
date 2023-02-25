using UnityEngine;
using Pathfinding;

public class BatBehaviour : EnemyBehaviour
{
	[Header("Derived Class Section")]
	[Space]

	[Header("Path Finding")]
	[Space]
	[SerializeField] private Seeker seeker;

	public float nextWaypointDist = .35f;

	// Private fields.
	private Vector3 patrolCenterPoint;
	private float randomNextTimer;

	private Vector3 target;
	private Path currentPath;
	private int currentWaypoint;
	
	//private bool reachedEndOfPath;

	protected override void Awake()
	{
		base.Awake();

		seeker = GetComponent<Seeker>();
	}

	protected override void Start()
	{
		base.Start();

		// Set the center point once the bat spawns.
		patrolCenterPoint = transform.position;
		InvokeRepeating("UpdatePath", 0f, .5f);
	}

	protected override void Update()
	{
		base.Update();

		if (!facingRight && rb2d.velocity.x > .01f)
			Flip();

		else if (facingRight && rb2d.velocity.x < .01f)
			Flip();
	}

	protected override void FixedUpdate()
	{
		if (currentPath == null)
			return;

		// If the bat has reached the end of the current path.
		if (currentWaypoint >= currentPath.vectorPath.Count)
		{
			//reachedEndOfPath = true;
			return;
		}
		//else
		//	reachedEndOfPath = false;

		Vector2 dir = (currentPath.vectorPath[currentWaypoint] - transform.position).normalized;
		rb2d.AddForce(walkSpeed * dir);

		float distance = Vector2.Distance(transform.position, currentPath.vectorPath[currentWaypoint]);

		if (distance < nextWaypointDist)
			currentWaypoint++;
	}

	protected override void Patrol()
	{
		randomNextTimer -= Time.deltaTime;

		if (randomNextTimer <= 0f)
		{
			// Use check radius as patrol area to pick random target point for setting path.
			float randomX = Random.Range(patrolCenterPoint.x - checkRadius, patrolCenterPoint.x + checkRadius);
			float randomY = Random.Range(patrolCenterPoint.y - checkRadius, patrolCenterPoint.y + checkRadius);

			target = new Vector2(randomX, randomY);
			randomNextTimer = 1.5f;
		}
	}

	protected override void ChasePlayer()
	{
		isPatrol = false;
		target = player.transform.position;
	}

	protected override void Attack()
	{
		isPatrol = false;
		if (!alreadyAttacked)
		{
			rb2d.velocity = Vector2.zero;
			animator.SetTrigger("Atk");

			Vector2 aimingDir = player.transform.position - transform.position;
			float angle = Mathf.Atan2(aimingDir.y, aimingDir.x) * Mathf.Rad2Deg;
			Debug.Log(angle);
			
			if (!facingRight)
			{
				if (angle < 0f)
					angle += 180f;
				else
					angle -= 180f;
			}

			rb2d.SetRotation(angle);

			alreadyAttacked = true;

			Invoke(nameof(ResetAttack), timeBetweenAtk);
		}
	}

	private void OnPathGenerate(Path path)
	{
		if (!path.error)
		{
			currentPath = path;
			currentWaypoint = 0;
		}
	}

	private void UpdatePath()
	{
		if (seeker.IsDone())
			seeker.StartPath(transform.position, target, OnPathGenerate);
	}

	protected override void OnDrawGizmosSelected()
	{
		if (centerPoint == null)
			return;

		Gizmos.DrawWireSphere(centerPoint.position, attackRange);
		Gizmos.DrawWireSphere(centerPoint.position, inSightRange);
		Gizmos.DrawLine(patrolCenterPoint, target);
	}
}
