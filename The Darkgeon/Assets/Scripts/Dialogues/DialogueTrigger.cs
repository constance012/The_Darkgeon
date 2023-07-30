using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class DialogueTrigger : MonoBehaviour
{
	[Header("Dialogue JSON file")]
	[Space]
	[SerializeField] private TextAsset inkJson;

	[Header("Configurations")]
	[Space]
	[Tooltip("Should this dialogue be triggered one time only?")]
	public bool triggerOneTime;

	private bool wasTriggered;

	private void Start()
	{
		if (triggerOneTime && wasTriggered)
		{
			SelfDestruct();
		}
	}

	public void TriggerDialogue()
	{
		if (inkJson == null)
		{
			Debug.LogWarning("The Ink Json file is not found, please assign it in the Inspector before triggering the Dialogue.");
			return;
		}

		if (!DialogueManager.DialogueIsPlaying)
		{
			DialogueManager.instance.TriggerDialogue(inkJson);

			if (triggerOneTime)
			{
				wasTriggered = true;
				SelfDestruct();
			}
		}
	}

	private void SelfDestruct()
	{
		GetComponent<Interactable>().dialogueTrigger = null;
		Destroy(this);
	}
}
