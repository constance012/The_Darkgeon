using UnityEngine;

public class Interactable : MonoBehaviour
{
	public Transform player;

	[SerializeField] protected float radius = 1f;

	protected Material mat;
	protected bool hasInteracted;

	private void Awake()
	{
		if (player == null)
			player = GameObject.FindWithTag("Player").transform;
	}

	protected void Update()
	{
		float distance = Vector2.Distance(player.position, transform.position);

		if (distance <= radius)
		{
			mat.SetFloat("_Thickness", .002f);

			if (!hasInteracted && InputManager.instance.GetKeyDown(KeybindingActions.Interact))
			{
				Interact();
				hasInteracted = true;
			}
		}

		else if (distance > radius)
		{
			mat.SetFloat("_Thickness", 0f);
			hasInteracted = false;
		}
	}

	public virtual void Interact()
	{
		Debug.Log("Interacting with " + transform.name);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
