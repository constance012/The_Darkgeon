using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
	// Reference.
	[SerializeField] private PlatformEffector2D effector;

	// Fields.
	float verticalMove;
	private void Awake()
	{
		effector = GetComponent<PlatformEffector2D>();
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		verticalMove = Input.GetAxisRaw("UseLadder");

		if (verticalMove == -1f && effector.surfaceArc != 0)
			effector.surfaceArc = 0;
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		effector.surfaceArc = 180;
	}
}
