using UnityEngine;

/// <summary>
/// This class is responsible for one-way-platform physics.
/// </summary>
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
		verticalMove = InputManager.instance.GetAxisRaw("vertical");

		if (verticalMove < 0f && effector.surfaceArc != 0)
			effector.surfaceArc = 0;
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		effector.surfaceArc = 180;
	}
}
