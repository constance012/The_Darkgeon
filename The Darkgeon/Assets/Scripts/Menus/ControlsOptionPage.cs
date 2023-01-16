using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Runtime.CompilerServices;

public class ControlsOptionPage : MonoBehaviour
{
	[Serializable]
	public class KeyCodeEvent : UnityEvent<KeyCode> { }

	[Header("References")]
	[Space]

	[Header("Keyset")]
	[Space]
	[SerializeField] private Keyset keySet;

	[Header("UIs")]
	[Space]

	[Header("TMPs.")]
	[Space]
	[SerializeField] private TMP_Dropdown keySetDropdown;
	[SerializeField] private TMP_InputField inputField;

	[Header("Buttons.")]
	[Space]
	[SerializeField] private Button cancelButton;
	[SerializeField] private Button addButton;

	[Header("Key Events")]
	[Space]
	public KeyCodeEvent keyDownListener;
	public KeyCodeEvent keyUpListener;
	public KeyCodeEvent keyPressListener;

	// Private fields.

	// Get all the keycode of the keyboard's keys only.
	private static readonly KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode))
		.Cast<KeyCode>()
		.Where(k => ((int)k < (int)KeyCode.JoystickButton0))
		.ToArray();


	// A list of all pressed down keys.
	private List<KeyCode> _keysDown;
	private string[] jsonFiles;

	private KeybindingActions currentAction = KeybindingActions.None;
	private TextMeshProUGUI currentButtonTextUI = null;
	
	private bool isRegistering;
	private string originalButtonText = "";


	private void Awake()
	{
		keySetDropdown = transform.Find("Keyset Area/Keyset Dropdown").GetComponent<TMP_Dropdown>();
		inputField = transform.Find("Keyset Area/Keyset Input Field").GetComponent<TMP_InputField>();

		cancelButton = transform.Find("Keyset Area/Cancel Button").GetComponent<Button>();
		addButton = transform.Find("Keyset Area/Add Button").GetComponent<Button>();
	}

	private void Start()
	{
		FetchJsonFiles();
	}

	private void OnEnable()
	{
		_keysDown = new List<KeyCode>();
	}

	private void OnDisable()
	{
		CancelBinding(originalButtonText);
		_keysDown = null;
	}

	private void Update()
	{
		if (inputField.gameObject.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				cancelButton.onClick.Invoke();

			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
				CreateNewJson();
		}
		
		if (!isRegistering)
			return;

		// Checks for key down event.
		if (Input.anyKeyDown)
			for (int i = 0; i < keyCodes.Length; i++)
			{
				KeyCode kc = keyCodes[i];

				if (Input.GetKeyDown(kc))
				{
					_keysDown.Add(kc);

					keyDownListener?.Invoke(kc);
				}
			}

		// Checks for key up and key press events.
		if (_keysDown.Count > 0)
			for (int i = 0; i < _keysDown.Count; i++)
			{
				KeyCode kc = _keysDown[i];

				if (Input.GetKeyUp(kc))
				{
					_keysDown.RemoveAt(i);
					i--;

					keyUpListener?.Invoke(kc);
					keyPressListener?.Invoke(kc);
				}
			}
	}

	/// <summary>
	/// Set new key for the selected action.
	/// </summary>
	/// <param name="action"></param>
	public void RegisterNewKey(string action)
	{
		TextMeshProUGUI clickedButtonTextUI = transform.Find("Scroll View/Viewport/Content/" + action + " Button/Text")
														.GetComponent<TextMeshProUGUI>();
		
		// If click on another action while currently binding this action, cancel the binding process first.
		//if (currentButtonTextUI != null && currentButtonTextUI != clickedButtonTextUI)
		//	CancelBinding(originalButtonText);

		currentButtonTextUI = clickedButtonTextUI;

		isRegistering = true;

		// Set the current action.
		Enum.TryParse<KeybindingActions>(ClearWhitespaces(action), true, out currentAction);

		originalButtonText = currentButtonTextUI.text;
		currentButtonTextUI.color = new Color(1f, .76f, 0f);  // Change the text's color to pressed color.
		currentButtonTextUI.text = "...";
	}

	/// <summary>
	/// Unbind the key of the selected action.
	/// </summary>
	public void UnbindSelectedKey()
	{
		keyDownListener?.Invoke(KeyCode.None);
		keyUpListener?.Invoke(KeyCode.None);
		keyPressListener?.Invoke(KeyCode.None);
	}

	// Set the key of the corresponding action in the Keyset, ignore if the new key is the same as the old one.
	// And save the new Keyset to a json file.
	public void OnAnyKeyDown(KeyCode keyCode)
	{
		Debug.Log("Pressed key: " + keyCode);
		//Debug.Log("Current key action: " + currentAction);

		foreach (Keyset.Key key in keySet.keyList)
			if (key.action == currentAction && key.keyCode != keyCode)
			{
				key.keyCode = keyCode;

				string selectedFile = keySetDropdown.options[keySetDropdown.value].text;
				keySet.SaveKeysetToJson(selectedFile);
				break;
			}
	}

	// Reset the UI, clear the current fields when the key is released.
	public void OnAnyKeyUp(KeyCode keyCode)
	{
		Debug.Log("Released key: " + keyCode);
		
		// Add a whitespace character between each capital letter, and trim out the leading whitespace.
		string buttonText = AddWhitespaceBeforeCapital(keyCode.ToString());
		buttonText = AddHyphenBeforeNumber(buttonText);
		
		CancelBinding(buttonText);
	}

	// When hits Back, Reset or OnDisable.
	public void OnCancelBinding()
	{
		CancelBinding(originalButtonText);
	}

	public void OnKeysetDropdownSelect(int index)
	{
		Debug.Log(index);

		string[] splitPath = jsonFiles[index].Split('/', '.');
		string fileName = splitPath[splitPath.Length - 2];
		
		keySet.LoadKeysetFromJson(fileName);

		PlayerPrefs.SetString("SelectedKeyset", fileName);

		ReloadUI();
	}

	public void OnDeleteButtonClick()
	{
		TextMeshProUGUI warningText = transform.Find("Delete Keyset Page/Delete Box/Warning Text").GetComponent<TextMeshProUGUI>();
		
		TMP_Dropdown.OptionData selectedOption = keySetDropdown.options[keySetDropdown.value];
		
		warningText.text = "DELETE KEYSET [" + selectedOption.text.ToUpper() + "]?";
	}

	public void OnDeleteKeysetConfirm()
	{
		string selectedFile = jsonFiles[keySetDropdown.value];
		
		// Delete the json file itself and its metadata file together.
		File.Delete(selectedFile);
		File.Delete(selectedFile + ".meta");

		FetchJsonFiles();
		OnKeysetDropdownSelect(keySetDropdown.value);
	}
	
	public void ResetToDefault()
	{
		keySetDropdown.value = keySetDropdown.options.FindIndex(keySet => keySet.text.ToLower() == "default");
		cancelButton.onClick.Invoke();
	}

	private void CreateNewJson()
	{
		string newFile = inputField.text.Trim();

		if (String.IsNullOrEmpty(newFile))
		{
			Debug.Log("File name must have at least 1 character.");
			return;
		}

		else
		{
			// Load the default data to the keyset.
			keySet.LoadKeysetFromJson("Default");

			// Create and Save that data to the new file.
			keySet.SaveKeysetToJson(newFile);

			// Fetch the files to the dropdown.
			FetchJsonFiles();

			cancelButton.onClick.Invoke();
		}
	}

	private void FetchJsonFiles()
	{
		string path = Application.streamingAssetsPath + "/Keyset Data/";
		//string persistentPath = Application.persistentDataPath + "/Keyset Data/";

		keySetDropdown.ClearOptions();
		
		jsonFiles = Directory.GetFiles(path, "*.json");
		
		for (int i = 0; i < jsonFiles.Length; i++)
		{
			string[] splitPath = jsonFiles[i].Split('/', '.');

			// Get the file name, excluding the extension and add it to the dropdown.
			TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(splitPath[splitPath.Length - 2]);
			keySetDropdown.options.Add(optionData);
		}

		keySetDropdown.RefreshShownValue();

		if (keySetDropdown.options.Count < 6)
		{
			addButton.interactable = true;
			Destroy(addButton.GetComponent<TooltipTrigger>());
		}
		else
		{
			addButton.interactable = false;
			
			if (addButton.gameObject.GetComponent<TooltipTrigger>() == null)
				addButton.gameObject.AddComponent<TooltipTrigger>().content =
				"You've reached the maximum amount of 5 custom keysets, delete some of them or edit the existng ones.";
		}

		// Set the value of the dropdown to the previously chosen Keyset, without notify.
		string keySetName = PlayerPrefs.GetString("SelectedKeyset", "Keyset_Default");
		int index = keySetDropdown.options.FindIndex(keySet => keySet.text == keySetName);

		keySetDropdown.SetValueWithoutNotify(index);

		ReloadUI();
	}

	private void ReloadUI()
	{
		foreach (Keyset.Key key in keySet.keyList)
		{
			// Get each button and edit its text.
			string buttonName = AddWhitespaceBeforeCapital(key.action.ToString());

			TextMeshProUGUI buttonTextUI = transform.Find("Scroll View/Viewport/Content/" + buttonName + " Button/Text").GetComponent<TextMeshProUGUI>();
			
			string keyName = AddWhitespaceBeforeCapital(key.keyCode.ToString()).ToUpper();

			buttonTextUI.text = AddHyphenBeforeNumber(keyName);
		}
	}

	private void CancelBinding(string buttonText)
	{
		if (currentButtonTextUI != null)
		{
			currentButtonTextUI.color = Color.white;
			currentButtonTextUI.text = buttonText.ToUpper();
		}

		isRegistering = false;
		_keysDown.Clear();

		currentAction = KeybindingActions.None;
		currentButtonTextUI = null;
		originalButtonText = "";

		transform.Find("Wait For Input Page").gameObject.SetActive(false);
	}

	public static string AddWhitespaceBeforeCapital(string str)
	{
		return String.Concat(str.Select(x => Char.IsUpper(x) ? " " + x : x.ToString()))
								.TrimStart(' ');
	}

	public static string AddHyphenBeforeNumber(string str)
	{
		return String.Concat(str.Select(x => Char.IsDigit(x) ? "-" + x : x.ToString()))
								.TrimStart('-');
	}

	public static string ClearWhitespaces(string str)
	{
		return new string(str.ToCharArray()
			.Where(c => !Char.IsWhiteSpace(c))
			.ToArray());
	}
}
