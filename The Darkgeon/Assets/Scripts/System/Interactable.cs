using UnityEngine;

public class Interactable : MonoBehaviour
{
	public Transform player;

	[SerializeField] protected float radius = 1f;

	private bool hasInteracted;

	private void Awake()
	{
		if (player == null)
			player = GameObject.FindWithTag("Player").transform;
	}

	private void Update()
	{
		float distance = Vector2.Distance(player.position, transform.position);

		if (distance <= radius && !hasInteracted && InputManager.instance.GetKeyDown(KeybindingActions.Interact))
		{
			Interact();
			hasInteracted = true;
		}

		else if (distance > radius)
			hasInteracted = false;
	}

	protected virtual void Interact()
	{
		Debug.Log("Interacting with " + transform.name);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
