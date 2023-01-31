using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Create New Item")]
public class Item : ScriptableObject
{
	[Header("General")]
	[Space]
	public new string name;
	[TextArea(5, 10)] public string description;

	public Sprite icon;

	[Header("Quantity and Stack")]
	[Space]

	public bool stackable;
	public int maxPerStack = 1;
	public int quantity = 1;

	[Header("Specials")]
	[Space]

	public bool isFavorite;
	public bool isDefaultItem;

	public virtual void Use() { }
}
