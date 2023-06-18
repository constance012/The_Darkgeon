using TMPro;
using UnityEngine;
using CSTGames.CommonEnums;

public abstract class Interactable : MonoBehaviour
{
	public enum InteractableType { Passive, Active, Manual }

	[Header("Type")]
	public InteractableType type;

	[Header("Reference")]
	public Transform player;
	[SerializeField] protected GameObject popupLabelPrefab;

	[Space]

	[Header("Interact Radius")]
	[SerializeField] protected float radius = 1f;

	protected Transform worldCanvas;
	protected Material mat;
	protected GameObject clone;

	[SerializeField, ReadOnly] protected bool hasInteracted;

	protected virtual void Awake()
	{
		if (player == null)
			player = GameObject.FindWithTag("Player").transform;

		worldCanvas = GameObject.FindWithTag("World Canvas").transform;
	}

	protected virtual void Update()
	{
		if (type == InteractableType.Passive)
			return;

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

	/// <summary>
	/// This method is responsible for being executed by other <c>Interactable</c> objects.
	/// </summary>
	public abstract void ExecuteRemoteLogic(bool state);

	protected virtual void CreatePopupLabel()
	{
		clone = Instantiate(popupLabelPrefab);
		clone.name = "Popup Label";

		clone.transform.Find("Names/Object Name").GetComponent<TextMeshProUGUI>().text = "";

		clone.transform.SetParent(worldCanvas, true);
		clone.transform.SetAsLastSibling();
		clone.transform.position = transform.position;
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
