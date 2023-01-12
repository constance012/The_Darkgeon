using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using TMPro;

public class ControlsOptionPage : MonoBehaviour
{
	[Serializable]
	public class KeyCodeEvent : UnityEvent<KeyCode> { }

	[Header("Keyset Reference")]
	[Space]
	[SerializeField] private Keyset keySet;

	[Header("Key Events")]
	[Space]
	public KeyCodeEvent keyDownListener;
	public KeyCodeEvent keyUpListener;
	public KeyCodeEvent keyPressListener;

	// Private fields.

	// Get all the keycode of the keyboard's keys only.
	private static readonly KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode))
		.Cast<KeyCode>()
		.Where(k => ((int)k < (int)KeyCode.Mouse0))
		.ToArray();


	// A list of all pressed down keys.
	private List<KeyCode> _keysDown;

	private KeybindingActions currentAction = KeybindingActions.None;
	private TextMeshProUGUI currentButtonTextUI = null;
	
	private bool isRegistering;
	private string originalButtonText = "";


	public void OnEnable()
	{
		_keysDown = new List<KeyCode>();

		ReloadUI();
	}

	public void OnDisable()
	{
		CancelBinding(originalButtonText);
		_keysDown = null;
	}

	public void Update()
	{
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

	public void RegisterNewKey(string action)
	{
		isRegistering = !isRegistering;
		
		currentButtonTextUI = transform.Find("Scroll View/Viewport/Content/" + action + " Button").Find("Text").GetComponent<TextMeshProUGUI>();

		if (isRegistering)
		{
			Enum.TryParse<KeybindingActions>(ClearWhitespaces(action), true, out currentAction);

			originalButtonText = currentButtonTextUI.text;
			currentButtonTextUI.color = new Color(1f, .76f, 0f);  // Change the text's color to pressed color.
			currentButtonTextUI.text = "...";
		}
		else
		{
			currentAction = KeybindingActions.None;
			currentButtonTextUI.color = Color.white;
			currentButtonTextUI.text = originalButtonText;
		}
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
				keySet.SaveKeysetToJson("Keyset_1");
				break;
			}
	}

	// Reset the UI, clear the current fields when the key is released.
	public void OnAnyKeyUp(KeyCode keyCode)
	{
		Debug.Log("Released key: " + keyCode);
		CancelBinding(keyCode.ToString());
	}

	// When hits Back, Reset or OnDisable.
	public void OnCancelBinding()
	{
		CancelBinding(originalButtonText);
	}

	public void OnInputFieldEnter(string enteredText)
	{
		Debug.Log("You entered: " + enteredText);
	}

	public void ResetToDefault()
	{
		ReloadUI();
	}

	private void ReloadUI()
	{
		foreach (Keyset.Key key in keySet.keyList)
		{
			// Get each button and edit its text.
			string buttonAction = AddWhitespaceBeforeCapital(key.action.ToString());
			TextMeshProUGUI buttonTextUI = transform.Find("Scroll View/Viewport/Content/" + buttonAction + " Button").Find("Text").GetComponent<TextMeshProUGUI>();
			
			buttonTextUI.text = AddWhitespaceBeforeCapital(key.keyCode.ToString()).ToUpper();
		}
	}

	private void CancelBinding(string buttonText)
	{
		if (currentButtonTextUI != null)
		{
			// Add a whitespace character between each capital letter, and trim out the leading whitespace.
			buttonText = AddWhitespaceBeforeCapital(buttonText);

			currentButtonTextUI.color = Color.white;
			currentButtonTextUI.text = buttonText.ToUpper();
		}

		isRegistering = false;
		currentAction = KeybindingActions.None;
		currentButtonTextUI = null;
		originalButtonText = "";
	}

	private string AddWhitespaceBeforeCapital(string str)
	{
		return String.Concat(str.Select(x => Char.IsUpper(x) ? " " + x : x.ToString()))
								.TrimStart(' ');
	}

	private string ClearWhitespaces(string str)
	{
		return new string(str.ToCharArray()
			.Where(c => !Char.IsWhiteSpace(c))
			.ToArray());
	}
}
