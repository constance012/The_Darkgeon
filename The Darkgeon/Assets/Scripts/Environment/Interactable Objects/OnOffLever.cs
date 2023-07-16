using TMPro;
using UnityEngine;

public class OnOffLever : Mechanisms
{
	public override bool Status => hasInteracted;

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
		Interact();
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
			clone.SetObjectName(status);
		}
	}
}
