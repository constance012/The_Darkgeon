using System;
using UnityEditor.SceneManagement;
using UnityEngine;

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

	[Space]
	[Header("General Info")]
	[SerializeField, ReadOnly] protected string ID;

	[ContextMenu("Generate Unique ID")]
	private void GenerateID()
	{
		ID = Guid.NewGuid().ToString();
		EditorSceneManager.MarkSceneDirty(gameObject.scene);
	}
	
	[ContextMenu("Clear Unique ID")]
	private void ClearID()
	{
		ID = "";
		EditorSceneManager.MarkSceneDirty(gameObject.scene);
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

	[Header("Dialogue (Optional)")]
	[ReadOnly] public DialogueTrigger dialogueTrigger;
	[ReadOnly] public bool oneTimeDialogueTriggered;

	// Protected fields.
	protected Transform worldCanvas;
	protected SpriteRenderer spriteRenderer;
	protected Material mat;
	protected InteractionPopupLabel clone;

	public bool HasDialogue => dialogueTrigger != null;

	protected virtual void Awake()
	{
		if (player == null)
			player = GameObject.FindWithTag("Player").transform;

		worldCanvas = GameObject.FindWithTag("World Canvas").transform;
		spriteRenderer = GetComponent<SpriteRenderer>();
		mat = spriteRenderer.material;

		dialogueTrigger = GetComponent<DialogueTrigger>();
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
		Debug.Log($"Interacting with {transform.name}.");

		// TODO - call the method in Dialogue Manager asynchronously.

		if (HasDialogue)
			dialogueTrigger.TriggerDialogue();
	}

	/// <summary>
	/// Bind this function to an Ink story for external function execution.
	/// </summary>
	public virtual void InkExternalFunction()
	{
		Debug.Log($"Invoke external function of {transform.name}.");
	}

	/// <summary>
	/// This method is responsible for being executed by other <c>Interactable</c> objects.
	/// </summary>
	public virtual void ExecuteRemoteLogic(bool state)
	{
		Debug.Log($"Execute logic of {transform.name} remotely.");
	}

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
