using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
	public Item currentItem;
	private Image icon;
	private TextMeshProUGUI quantity;

	private void Awake()
	{
		icon = transform.Find("Item Button/Icon").GetComponent<Image>();
		quantity = transform.Find("Item Button/Quantity").GetComponent<TextMeshProUGUI>();
	}

	public void AddItem(Item newItem)
	{
		currentItem = newItem;

		icon.sprite = currentItem.icon;
		icon.enabled = true;

		if (currentItem.stackable)
		{
			quantity.text = currentItem.quantity.ToString();
			quantity.enabled = true;
		}
		else
		{
			quantity.text = "1";
			quantity.enabled = false;
		}
	}

	public void ClearItem()
	{
		currentItem = null;

		icon.sprite = null;
		icon.enabled = false;

		quantity.text = "";
		quantity.enabled = false;
	}
}
