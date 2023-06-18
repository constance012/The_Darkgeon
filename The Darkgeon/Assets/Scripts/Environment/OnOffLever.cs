using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class OnOffLever : Interactable
{
	public float maxInteractDistance;
	
	[Header("Object to Control")]
	[Space]
	[SerializeField] private Interactable target;

	[Space]
	[SerializeField] private LogicEvaluator evaluator;

	public bool Status
	{
		get { return hasInteracted; }
	}

	protected override void Awake()
	{
		base.Awake();
		mat = GetComponent<SpriteRenderer>().material;

		LogicEvaluator targetEvaluator;
		target.TryGetComponent<LogicEvaluator>(out targetEvaluator);
		
		if (targetEvaluator == evaluator)
		{
			Debug.Log($"The {target.name} is controlled by a logic evaluator, removes its reference from {this.name}.");
			target = null;
		}
	}

	protected override void Update()
	{
		Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		float mouseDistance = Vector2.Distance(worldMousePos, transform.position);
		float playerDistance = Vector2.Distance(player.position, transform.position);

		if (mouseDistance <= radius)
		{
			if (clone == null)
				CreatePopupLabel();
			else
				clone.transform.position = transform.position;

			mat.SetFloat("_Thickness", .002f);

			if (Input.GetMouseButtonDown(1) && playerDistance <= maxInteractDistance)
				Interact();
		}

		else
		{
			Destroy(clone);
			mat.SetFloat("_Thickness", 0f);
		}
	}

	public override void Interact()
	{
		base.Interact();

		hasInteracted = !hasInteracted;
		transform.localScale = transform.localScale.FlipByScale('x');
		evaluator.Evaluate();

		UpdatePopupLabel();

		if (target != null)
			target.ExecuteRemoteLogic(hasInteracted);
	}

	public override void ExecuteRemoteLogic(bool state)
	{
		
	}

	protected override void CreatePopupLabel()
	{
		base.CreatePopupLabel();
		UpdatePopupLabel();
	}

	private void UpdatePopupLabel()
	{
		if (clone != null)
		{
			string status = hasInteracted ? "ON" : "OFF";
			clone.transform.Find("Names/Object Name").GetComponent<TextMeshProUGUI>().text = status;
		}
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, maxInteractDistance);

		if (target != null)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(transform.position, target.transform.position);
		}
	}
}
