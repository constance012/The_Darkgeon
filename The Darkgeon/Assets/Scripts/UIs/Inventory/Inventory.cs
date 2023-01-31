using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	public static Inventory instance { get; private set; }

	public delegate void OnItemChanged();
	public OnItemChanged onItemChanged;

	// Fields
	public List<Item> items = new List<Item>();
	private InventorySlot[] slots;

	public int space = 20;

	private void OnEnable()
	{
		ReloadUI();
	}

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

		slots = transform.Find("Slots").GetComponentsInChildren<InventorySlot>();
	}

	private void Start()
	{
		instance.onItemChanged += ReloadUI;
	}

	public bool Add(Item target)
	{
		if (items.Count >= space)
		{
			Debug.Log("Inventory Full.");
			return false;
		}

		// Add to the list if it's not a default item.
		if (!target.isDefaultItem)
		{
			// Loop through the list to check if there's the same item.
			for (int i = 0; i < items.Count; i++)
			{
				if (!items[i].name.Equals(target.name))
					continue;

				if (items[i].name.Equals(target.name) && items[i].quantity == items[i].maxPerStack)
					continue;

				// If the item is stackable and hasn't reached its max per stack yet.
				if (items[i].stackable && items[i].quantity < items[i].maxPerStack)
				{
					int totalQuantity = items[i].quantity + target.quantity;

					// If the new total quantity exceeds the maximum amount.
					// Then set the current one's quantity to max, set the new one's quantity to the residue amount and add to the next slot.
					if (totalQuantity > items[i].maxPerStack)
					{
						int residue = totalQuantity - items[i].maxPerStack;

						items[i].quantity = items[i].maxPerStack;
						target.quantity = residue;

						items.Add(target);
					}

					else if (totalQuantity == items[i].maxPerStack)
						items[i].quantity = totalQuantity;

					// Otherwise, just increase the quantity of the current one.
					else
						items[i].quantity += target.quantity;
				}

				// If it's not stackable or has reached the max per stack.
				// Then add it to the next element.
				else if (!items[i].stackable || items[i].quantity == items[i].maxPerStack)
					items.Add(target);

				onItemChanged?.Invoke();
				return true;
			}

			// If it's a completely new item, then just add it into the list.
			items.Add(target);
			onItemChanged?.Invoke();
			return true;
		}

		return false;
	}

	public void Remove(Item target)
	{
		items.Remove(target);

		onItemChanged?.Invoke();
	}

	private void ReloadUI()
	{
		for (int i = 0; i < slots.Length; i++)
		{
			// Add item to the slot if there's any.
			if (i < items.Count)
				slots[i].AddItem(items[i]);

			// Otherwise, clear the slot.
			else
				slots[i].ClearItem();
		}
	}
}
