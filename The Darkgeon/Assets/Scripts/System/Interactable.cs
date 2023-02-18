using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
	[Header("Reference")]
	public Transform player;
	[SerializeField] protected GameObject popupLabelPrefab;

	[Space]

	[Header("Interact Radius")]
	[SerializeField] protected float radius = 1f;

	protected Transform worldCanvas;
	protected Material mat;
	protected GameObject clone;

	protected bool hasInteracted;

	private void Awake()
	{
		if (player == null)
			player = GameObject.FindWithTag("Player").transform;

		worldCanvas = GameObject.Find("World Canvas").transform;
	}

	protected virtual void Update()
	{
		float distance = Vector2.Distance(player.position, transform.position);

		if (distance <= radius)
		{
			if (clone == null)
				CreatePopupLabel();
			else
				clone.transform.position = transform.position;

			mat.SetFloat("_Thickness", .002f);

			if (!hasInteracted && InputManager.instance.GetKeyDown(KeybindingActions.Interact))
			{
				Interact();
				hasInteracted = true;
			}
		}

		else if (distance > radius)
		{
			Destroy(clone);

			mat.SetFloat("_Thickness", 0f);
			hasInteracted = false;
		}
	}

	public virtual void Interact()
	{
		Debug.Log("Interacting with " + transform.name);
	}

	protected virtual void CreatePopupLabel()
	{
		clone = Instantiate(popupLabelPrefab);
		clone.name = "Popup Label";

		clone.transform.Find("Object Name").GetComponent<TextMeshProUGUI>().text = "";

		clone.transform.SetParent(worldCanvas, true);
		clone.transform.SetAsLastSibling();
		clone.transform.position = transform.position;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
