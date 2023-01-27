using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
	public static Inventory instance { get; private set; }

	public delegate void OnItemChanged();
	public OnItemChanged onItemChanged;

	// Fields
	public List<Item> items = new List<Item>();

	public int space = 20;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Inventory found!!");
			Destroy(gameObject);
			return;
		}
	}

	public void Add(Item target)
	{
		if (items.Count >= space)
		{
			Debug.Log("Inventory Full");
			return;
		}

		if (!target.isDefaultItem)
		{
			items.Add(target);

			onItemChanged?.Invoke();
		}
	}

	public void Remove(Item target)
	{
		items.Remove(target);

		onItemChanged?.Invoke();
	}
}
