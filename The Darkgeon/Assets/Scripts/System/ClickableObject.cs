using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
	private InventorySlot currentSlot;

	private bool isLeftAltHeld;

	private void Awake()
	{
		currentSlot = transform.GetComponentInParent<InventorySlot>();
	}

	private void Update()
	{
		isLeftAltHeld = Input.GetKey(KeyCode.LeftAlt);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (isLeftAltHeld && eventData.button == PointerEventData.InputButton.Left)
		{
			bool favorite = !currentSlot.currentItem.isFavorite;
			Inventory.instance.SetFavorite(currentSlot.currentItem.id, favorite);
			Debug.Log(currentSlot.currentItem + " is " + (favorite ? "favorite" : "not favorite"));
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
			currentSlot.UseItem();
	}
}
