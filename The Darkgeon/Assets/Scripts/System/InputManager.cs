using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

/// <summary>
/// Manages all the keyboard input for the game.
/// </summary>
public class InputManager : MonoBehaviour
{
	public static InputManager instance { get; private set; }

	[Header("Keyset Reference")]
	[Space]
	[SerializeField] private Keyset keySet;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Destroy(gameObject);
			return;
		}
	}

	public KeyCode GetKeyForAction(KeybindingActions action)
	{
		foreach (Keyset.Key key in keySet.keyList)
			if (key.action == action)
				return key.keyCode;

		return KeyCode.None;
	}

	/// <summary>
	/// Returns true while the user holds down the key for the specified action.
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public bool GetKey(KeybindingActions action)
	{
		foreach (Keyset.Key key in keySet.keyList)
			if (key.action == action)
				return Input.GetKey(key.keyCode);

		return false;
	}

	/// <summary>
	/// Returns true during the frame the user starts pressing down the key for the specified action.
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public bool GetKeyDown(KeybindingActions action)
	{
		foreach (Keyset.Key key in keySet.keyList)
			if (key.action == action)
				return Input.GetKeyDown(key.keyCode);

		return false;
	}

	/// <summary>
	/// Returns true during the frame the user releases the key for the specified action.
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public bool GetKeyUp(KeybindingActions action)
	{
		foreach (Keyset.Key key in keySet.keyList)
			if (key.action == action)
				return Input.GetKeyUp(key.keyCode);

		return false;
	}

	/// <summary>
	/// Returns the value of the axis based on which key is being held.
	/// </summary>
	/// <param name="axis"></param>
	/// <returns></returns>
	public float GetAxisRaw(string axis)
	{
		axis = axis.ToLower().Trim();
		
		switch (axis)
		{
			case "horizontal":
				if (GetKey(KeybindingActions.MoveRight))
					return 1f;
				
				else if (GetKey(KeybindingActions.MoveLeft))
					return -1f;

				else
					return 0f;

			case "vertical":
				if (GetKey(KeybindingActions.ClimbUp))
					return 1f;
				
				else if (GetKey(KeybindingActions.ClimbDown))
					return -1f;

				else
					return 0f;
		}

		return 0f;
	}
}
