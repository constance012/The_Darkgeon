using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameDataManager))]
public class GameDataEditor : Editor
{
	GameDataManager inspectedObject;
	SerializedObject managerSerializedObj;
	SerializedProperty enableManager;

	private void OnEnable()
	{
		inspectedObject = (GameDataManager)target;
		managerSerializedObj = new SerializedObject(inspectedObject);
		enableManager = managerSerializedObj.FindProperty("enableManager");
	}

	public override void OnInspectorGUI()
	{
		managerSerializedObj.Update();
		
		GUIStyle style = new GUIStyle();
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.white;

		GUILayout.Label(new GUIContent("Enable Game Data Manager", "Check this box to specify whether to enable this component."), style);
		GUILayout.Space(5f);
		
		bool isEnabled = GUILayout.Toggle(enableManager.boolValue, " Enable");
		enableManager.boolValue = isEnabled;
		GUI.enabled = isEnabled;

		DrawDefaultInspector();

		GUILayout.Space(10f);

		// Label configuration.
		GUIContent content = new GUIContent("Encryption", "Check this box to specify whether to encrypt the data or not. " +
														  "Changes will happened immediately if the value of this box is changed.");

		GUILayout.Label(content, style);
		GUILayout.Space(5f);
		
		bool newValue = GUILayout.Toggle(inspectedObject.useEncryption, " Use Encryption");

		if (newValue != inspectedObject.useEncryption)
		{
			if (newValue)
				inspectedObject.EncryptManually();
			else
				inspectedObject.DecryptManually();
		}

		managerSerializedObj.ApplyModifiedProperties();
	}
}
