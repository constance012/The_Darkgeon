using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	public Item dragItem;
	
	// A clone that "ships" the current item in this slot.
	[HideInInspector] public GameObject clone;

	public void OnBeginDrag(PointerEventData eventData)
	{
		dragItem = transform.GetComponentInParent<InventorySlot>().currentItem;

		clone = Instantiate(gameObject, transform.root);
		clone.GetComponent<Image>().raycastTarget = false;
		
		Debug.Log("You're dragging the " + dragItem.name);
	}

	public void OnDrag(PointerEventData eventData)
	{
		//Debug.Log("On Drag");
		clone.transform.position = Input.mousePosition;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		Item itemAfterDrag = clone.GetComponent<DraggableItem>().dragItem;

		if (itemAfterDrag != null)
		{
			Debug.Log("You received a " + itemAfterDrag.name);
			transform.GetComponentInParent<InventorySlot>().AddItem(itemAfterDrag);
		}
		else
		{
			Debug.Log("You received nothing.");
			transform.GetComponentInParent<InventorySlot>().ClearItem();
		}

		dragItem = null;
		Destroy(clone);
	}
}
