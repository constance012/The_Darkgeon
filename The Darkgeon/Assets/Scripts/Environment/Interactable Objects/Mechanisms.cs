using UnityEngine;

/// <summary>
/// Base class of all mechanisms components.
/// </summary>
public abstract class Mechanisms : Interactable
{
	[Header("Object to Control")]
	[Space]
	[SerializeField] protected Interactable target;

	[Space]
	[SerializeField] protected LogicEvaluator evaluator;

	public abstract bool Status { get; }

	protected override void Awake()
	{
		base.Awake();

		target.TryGetComponent<LogicEvaluator>(out LogicEvaluator targetEvaluator);

		if (targetEvaluator == evaluator)
		{
			Debug.Log($"The {target.name} is controlled by a logic evaluator, removes its reference from {this.name}.");
			target = null;
		}
	}

	protected override void TriggerInteraction(float playerDistance)
	{
		base.TriggerInteraction(playerDistance);

		if (Input.GetMouseButtonDown(1) && playerDistance <= interactDistance)
			Interact();
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();

		if (target != null)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(transform.position, target.transform.position);
		}
	}
}
