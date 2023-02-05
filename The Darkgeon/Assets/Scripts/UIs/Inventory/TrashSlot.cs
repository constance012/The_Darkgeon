using UnityEngine;
using UnityEngine.EventSystems;

public class TrashSlot : MonoBehaviour, IPointerDownHandler
{
	public void OnPointerDown(PointerEventData eventData)
	{
		if (ClickableObject.holdingItem)
		{
			Item trash = ClickableObject.clone.GetComponent<ClickableObject>().dragItem;

			if (ClickableObject.splittingItem && trash.isFavorite)
				Inventory.instance.UpdateQuantity(ClickableObject.sender.dragItem.id, trash.quantity);
			else
				Inventory.instance.Remove(trash);

			ClickableObject.ClearSingleton();
		}
	}
}
