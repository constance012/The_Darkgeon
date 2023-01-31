using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
	[Header("Current Item")]
	[Space]

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

	public void OnDrop(PointerEventData eventData)
	{
		if (eventData.pointerDrag.GetComponent<DraggableItem>() != null)
		{
			GameObject droppedObj = eventData.pointerDrag.GetComponent<DraggableItem>().clone;
			DraggableItem droppedItem = droppedObj.GetComponent<DraggableItem>();

			Item temp = currentItem;  // This item is null if the slot doesn't already hold an item.
			AddItem(droppedItem.dragItem);

			droppedItem.dragItem = temp;
		}
	}
}
