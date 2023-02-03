using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	public Item dragItem;
	
	// A clone that "ships" the current item in this slot.
	[HideInInspector] public GameObject clone;

	private Image icon;

	private void Awake()
	{
		icon = GetComponent<Image>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		dragItem = transform.GetComponentInParent<InventorySlot>().currentItem;

		clone = Instantiate(gameObject, transform.root);
		clone.GetComponent<Image>().raycastTarget = false;
		
		icon.color = new Color(.51f, .51f, .51f);
		
		Debug.Log("You're dragging the " + dragItem.name);
	}

	public void OnDrag(PointerEventData eventData)
	{
		clone.transform.position = Input.mousePosition;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		icon.color = Color.white;
		
		dragItem = null;
		Destroy(clone);
	}
}
