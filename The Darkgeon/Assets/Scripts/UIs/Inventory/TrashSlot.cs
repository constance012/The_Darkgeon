using UnityEngine;
using UnityEngine.EventSystems;

public class TrashSlot : MonoBehaviour, IDropHandler
{
	public void OnDrop(PointerEventData eventData)
	{
		if (eventData.pointerDrag.GetComponent<DraggableItem>() != null)
		{
			DraggableItem trash = eventData.pointerDrag.GetComponent<DraggableItem>();

			Inventory.instance.Remove(trash.dragItem);
			
			eventData.pointerDrag = null;
			Destroy(trash.clone);
		}
	}
}
