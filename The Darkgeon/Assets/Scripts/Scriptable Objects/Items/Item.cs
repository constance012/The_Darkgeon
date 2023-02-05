using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/New Base Item")]
public class Item : ScriptableObject
{
	[HideInInspector] public string id;
	
	[Header("General")]
	[Space]

	public int slotIndex = -1;

	public string itemName;
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
	public bool canbeUsed;

	public virtual void Use()
	{
		Debug.Log("Using " + itemName);
	}
}
