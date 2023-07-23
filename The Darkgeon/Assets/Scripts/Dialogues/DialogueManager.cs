using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using System.Threading.Tasks;
using Unity.VisualScripting;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance { get; private set; }

	[Header("References")]
	[Space]
	[SerializeField] private TextMeshProUGUI dialogueText;
	[SerializeField] private Transform continueIcon;

	[Header("Choices Interface")]
	[Space]
	[SerializeField] private GameObject[] choiceObjects;
	private TextMeshProUGUI[] _choiceTexts;

	[Header("Dialogue Configs")]
	[Space]
	[SerializeField] private float dialogueSpeed;

	public static bool DialogueIsPlaying { get; private set; }

	private Story _currentStory;
	private IEnumerator _animateRoutine;
	private string _previousSentence = "";
	private bool _hasChoices;

    private void Awake()
    {
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Dialogue Manager found!! Destroy the newest one.");
			Destroy(gameObject);
			return;
		}

		dialogueText = transform.GetComponentInChildren<TextMeshProUGUI>("Text");
		continueIcon = transform.Find("Continue Icon");

		_choiceTexts = new TextMeshProUGUI[choiceObjects.Length];

		for (int i = 0; i < _choiceTexts.Length; i++)
			_choiceTexts[i] = choiceObjects[i].GetComponentInChildren<TextMeshProUGUI>();
	}

	private void Start()
	{
		DialogueIsPlaying = false;
		this.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (!DialogueIsPlaying)
			return;

		if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
		{
			// Stop the coroutine and display the text and choices instantly
			// if receive an input from the user while the coroutine is running.
			if (!dialogueText.text.Equals(_previousSentence))
			{
				StopCoroutine(_animateRoutine);
				_animateRoutine = null;

				dialogueText.text = _previousSentence;

				DisplayChoices();

				continueIcon.gameObject.SetActive(true);

				return;
			}

			// Force the user to pick a choice in order to continue the story.
			if (!_hasChoices)
				ContinueStory();
		}
	}

	public void TriggerDialogue(TextAsset inkJson)
	{
		_currentStory = new Story(inkJson.text);

		PlayerMovement.canMove = false;
		PlayerActions.canAttack = false;

		this.gameObject.SetActive(true);
		DialogueIsPlaying = true;

		ContinueStory();
	}

	public void MakeChoice(Transform choiceButton)
	{
		int index = choiceButton.GetSiblingIndex();
		_currentStory.ChooseChoiceIndex(index);

		DisableAllChoices();
		ContinueStory();
	}

	private void EndDialogue()
	{
		PlayerMovement.canMove = true;
		PlayerActions.canAttack = true;

		DialogueIsPlaying = false;
		this.gameObject.SetActive(false);

		dialogueText.text = "";
	}

	private void ContinueStory()
	{
		if (_currentStory.canContinue)
		{
			_previousSentence = _currentStory.Continue().ToUpper();
			_animateRoutine = DisplayDialogueText(_previousSentence);

			StartCoroutine(_animateRoutine);
		}
		else
		{
			EndDialogue();
		}
	}

	private void DisplayChoices()
	{
		List<Choice> currentChoices = _currentStory.currentChoices;
		_hasChoices = currentChoices.Count != 0;

		if (currentChoices.Count > choiceObjects.Length)
		{
			Debug.LogError($"The Ink file has more choices than the UI can support. Total choices: {currentChoices.Count}", this);
		}

		if (!_hasChoices)
		{
			DisableAllChoices();
			return;
		}

		for (int index = 0; index < choiceObjects.Length; index++)
		{
			// Disable all the unused choices left on the UI.
			if (index >= currentChoices.Count)
			{
				choiceObjects[index].SetActive(false);
				_choiceTexts[index].text = $"CHOICE {index}";
				continue;
			}

			// Enable and initialize the choices UI up to the amount of choices at this point of the Story.
			choiceObjects[index].SetActive(true);
			_choiceTexts[index].text = currentChoices[index].text.ToUpper();
		}
	}

	private void DisableAllChoices()
	{
		for (int index = 0; index < choiceObjects.Length; index++)
		{
			choiceObjects[index].SetActive(false);
			_choiceTexts[index].text = $"CHOICE {index}";
		}
	}

	private IEnumerator DisplayDialogueText(string sentence)
	{
		dialogueText.text = "";
		continueIcon.gameObject.SetActive(false);

		foreach (char letter in sentence)
		{
			dialogueText.text += letter;
			yield return new WaitForSecondsRealtime(dialogueSpeed);
		}

		DisplayChoices();

		continueIcon.gameObject.SetActive(true);
	}
}
