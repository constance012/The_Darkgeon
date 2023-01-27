using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Create New Item")]
public class Item : ScriptableObject
{
	public new string name;
	[TextArea(5, 10)] public string description;

	public Sprite icon;

	public bool canBeTrash;
	public bool isDefaultItem;
}
