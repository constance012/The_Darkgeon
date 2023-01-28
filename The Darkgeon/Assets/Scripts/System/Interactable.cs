using UnityEngine;

public class Interactable : MonoBehaviour
{
	[SerializeField] protected float radius = 1f;

	[SerializeField] protected Transform player;

	private bool hasInteracted;

	private void Awake()
	{
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
