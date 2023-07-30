using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;

public class DialogueManager : Singleton<DialogueManager>
{
	[Header("Global Variable Manager JSON")]
	[Space]
	[SerializeField] private TextAsset varManagerJson;

	[Header("References")]
	[Space]
	[SerializeField] private TextMeshProUGUI dialogueText;
	[SerializeField] private TextMeshProUGUI speakerText;

	[SerializeField] private Animator portraitAnimator;
	[SerializeField] private Animator layoutAnimator;

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
	private InkVariableObserver _globalObserver;


	private string _previousSentence = "";
	private bool _hasChoices;

	// Ink's tag constants.
	private const string SPEAKER_TAG = "speaker";
	private const string PORTRAIT_TAG = "portrait";
	private const string LAYOUT_TAG = "layout";

	protected override void Awake()
	{
		base.Awake();
		
		// Initialize the observer.
		_globalObserver = new InkVariableObserver(varManagerJson);

		dialogueText = transform.GetComponentInChildren<TextMeshProUGUI>("Text");
		speakerText = transform.GetComponentInChildren<TextMeshProUGUI>("Speaker Frame/Background/Speaker Name");

		portraitAnimator = transform.GetComponentInChildren<Animator>("Portrait Frame/Image");
		layoutAnimator = GetComponent<Animator>();

		continueIcon = transform.Find("Continue Icon");

		// Populate the choices array.
		_choiceTexts = new TextMeshProUGUI[choiceObjects.Length];

		for (int i = 0; i < _choiceTexts.Length; i++)
			_choiceTexts[i] = choiceObjects[i].GetComponentInChildren<TextMeshProUGUI>();

		// TODO: This is used in development only, remove it before build the game.
		Application.quitting += SaveGlobalVariables;
	}

	private void OnEnable()
	{
		DisableAllChoices();
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
			// if receive an input from the user while the text is animating.
			if (dialogueText.maxVisibleCharacters != _previousSentence.Length)
			{
				StopCoroutine(_animateRoutine);
				_animateRoutine = null;

				dialogueText.maxVisibleCharacters = _previousSentence.Length;

				DisplayChoices();

				continueIcon.gameObject.SetActive(true);

				return;
			}

			// Force the user to pick a choice in order to continue the story.
			if (!_hasChoices)
				ContinueStory();
		}
	}

	public void SetVariable(string name, object value)
	{
		Value inkValue = Value.Create(value);
		_globalObserver.SetVariable(name, inkValue);
	}

	public Ink.Runtime.Object GetVariable(string name)
	{
		return _globalObserver.GetVariable(name);
	}

	public void SaveGlobalVariables()
	{
		_globalObserver.SaveStoryState();
	}

	public void TriggerDialogue(TextAsset inkJson)
	{
		_currentStory = new Story(inkJson.text);
		_globalObserver.Register(_currentStory);

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
		_globalObserver.Unregister(_currentStory);

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

			if (_previousSentence.Equals(""))
			{
				EndDialogue();
				return;
			}
			
			_animateRoutine = AnimateDialogueText(_previousSentence);

			StartCoroutine(_animateRoutine);

			HandleTags();
		}
		else
		{
			EndDialogue();
		}
	}

	private void HandleTags()
	{
		List<string> storyTags = _currentStory.currentTags;

		foreach (string tag in storyTags)
		{
			// Split the tag as a key value pair.
			string[] tagContent = tag.Split(':', 2);

			string tagKey = tagContent[0].Trim().ToLower();
			string tagValue = tagContent[1].Trim();

			switch (tagKey)
			{
				case SPEAKER_TAG:
					speakerText.text = tagValue.ToUpper();
					break;

				case PORTRAIT_TAG:
					portraitAnimator.Play(tagValue.ToLower());
					break;

				case LAYOUT_TAG:
					layoutAnimator.Play(tagValue.ToLower());
					break;

				default:
					Debug.LogWarning($"Tag {tag} doesn't have behaviour to handle.", this);
					break;
			}
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

	private IEnumerator AnimateDialogueText(string sentence)
	{
		dialogueText.text = sentence;
		dialogueText.maxVisibleCharacters = 0;

		continueIcon.gameObject.SetActive(false);

		bool isRichText = false;

		foreach (char letter in sentence)
		{
			// Skip if this letter is a part of rich text tag.
			if (letter.Equals('<') || isRichText)
			{
				isRichText = true;

				if (letter.Equals('>'))
					isRichText = false;
			}

			// Otherwise, increase the maximum visible characters by 1.
			else
			{
				dialogueText.maxVisibleCharacters++;
				yield return new WaitForSecondsRealtime(dialogueSpeed);
			}
		}

		DisplayChoices();

		continueIcon.gameObject.SetActive(true);
	}
}
