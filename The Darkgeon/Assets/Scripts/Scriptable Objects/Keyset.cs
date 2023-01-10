using UnityEngine;

/// <summary>
/// A scriptable object for creating a set of keys use in keybinding.
/// </summary>
[CreateAssetMenu(fileName = "New Keyset", menuName = "Keybinding/Keyset")]
public class Keyset : ScriptableObject
{
	[System.Serializable]
	public class Key
	{
		public KeybindingActions action;
		public KeyCode keyCode;
	}

	public Key[] keyList;
}
