using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameDataManager))]
public class GameDataEditor : Editor
{
	public override void OnInspectorGUI()
	{
		GameDataManager manager = (GameDataManager)target;

		DrawDefaultInspector();

		GUILayout.Space(10f);

		// Label configuration.
		GUIStyle style = new GUIStyle();
		GUIContent content = new GUIContent("Encryption", "Check this box to specify whether to encrypt the data or not. " +
														  "Changes will happened immediately if the value of this box is changed.");

		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.white;
		GUILayout.Label(content, style);
		
		GUILayout.Space(5f);

		bool newValue = GUILayout.Toggle(manager.useEncryption, " Use Encryption");

		if (newValue != manager.useEncryption)
		{
			if (newValue)
				manager.EncryptManually();
			else
				manager.DecryptManually();

			manager.useEncryption = newValue;
		}
	}
}
