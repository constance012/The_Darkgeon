using TMPro;
using UnityEngine;
using CSTGames.CommonEnums;

/// <summary>
/// Base class for all interactable objects.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Interactable : MonoBehaviour
{
	public enum InputSource { Mouse, Keyboard, Joystick, None }

	public enum InteractableType
	{
		/// <summary>
		/// Can only be controlled by other mechanisms.
		/// </summary>
		Passive,

		/// <summary>
		/// Can either be controlled by other mechanisms or interacted by the player.
		/// </summary>
		Active,

		/// <summary>
		/// Can only be interacted manually by the player.
		/// </summary>
		Manual
	}

	[Header("Type")]
	public InteractableType type;
	public InputSource inputSource;

	[Header("Reference")]
	public Transform player;
	[SerializeField] protected GameObject popupLabelPrefab;

	[Space]

	[Header("Interaction Radius")]
	[SerializeField, Tooltip("The radius of this object in which the object name will be displayed when the mouse hovering over.")]
	protected float radius = 1f;
	
	[SerializeField, Tooltip("The distance required for the player to interact with this object.")]
	protected float interactDistance;
	
	[SerializeField, ReadOnly] protected bool hasInteracted;

	// Protected fields.
	protected Transform worldCanvas;
	protected SpriteRenderer spriteRenderer;
	protected Material mat;
	protected InteractionPopupLabel clone;

	protected virtual void Awake()
	{
		if (player == null)
			player = GameObject.FindWithTag("Player").transform;

		worldCanvas = GameObject.FindWithTag("World Canvas").transform;
		spriteRenderer = GetComponent<SpriteRenderer>();
		mat = spriteRenderer.material;
	}

	protected void Update()
	{
		if (type == InteractableType.Passive)
			return;

		Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		float mouseDistance = Vector2.Distance(worldMousePos, transform.position);
		float playerDistance = Vector2.Distance(player.position, transform.position);

		CheckForInteraction(mouseDistance, playerDistance);
	}

	public virtual void Interact()
	{
		Debug.Log("Interacting with " + transform.name);
	}

	/// <summary>
	/// This method is responsible for being executed by other <c>Interactable</c> objects.
	/// </summary>
	public abstract void ExecuteRemoteLogic(bool state);

	protected virtual void CheckForInteraction(float mouseDistance, float playerDistance)
	{
		if (mouseDistance <= radius)
		{
			TriggerInteraction(playerDistance);
		}

		else
		{
			CancelInteraction(playerDistance);
		}
	}

	protected virtual void TriggerInteraction(float playerDistance)
	{
		if (clone == null)
			CreatePopupLabel();
		else
			clone.transform.position = transform.position;

		mat.SetFloat("_Thickness", 1f);

		// TODO - derived classes implement their own way to trigger interaction.
	}

	protected virtual void CancelInteraction(float playerDistance)
	{
		if (clone != null)
			Destroy(clone.gameObject);

		mat.SetFloat("_Thickness", 0f);

		// TODO - derived classes implement their own way to cancel interaction.
	}

	protected virtual void CreatePopupLabel()
	{
		GameObject label = Instantiate(popupLabelPrefab);
		label.name = popupLabelPrefab.name;

		clone = label.GetComponent<InteractionPopupLabel>();

		clone.SetupLabel(this.transform, inputSource);
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, interactDistance);
	}
}
