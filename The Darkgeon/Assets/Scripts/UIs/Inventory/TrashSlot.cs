using UnityEngine;
using UnityEngine.EventSystems;

public class TrashSlot : MonoBehaviour, IDropHandler
{
	public void OnDrop(PointerEventData eventData)
	{
		if (eventData.pointerDrag.GetComponent<DraggableItem>() != null)
		{
			Item trash = eventData.pointerDrag.GetComponent<DraggableItem>().dragItem;

			Inventory.instance.Remove(trash);
		}
	}
}
