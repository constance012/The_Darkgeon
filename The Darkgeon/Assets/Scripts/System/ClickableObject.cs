using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEditor.Rendering;

public class ClickableObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
	public Item dragItem;
	
	// Private fields.
	private InventorySlot currentSlot;
	private Image icon;

	private bool isLeftAltHeld;
	private bool isLeftShiftHeld;

	// Static fields.

	// A clone that "ships" the current item in this slot.
	public static GameObject clone { get; set; }
	// Singleton reference to the sender script.
	public static ClickableObject sender { get; private set; }
	public static bool holdingItem { get; set; }
	public static bool splittingItem { get; set; }

	private void Awake()
	{
		currentSlot = transform.GetComponentInParent<InventorySlot>();
		icon = transform.Find("Icon").GetComponent<Image>();
	}

	private void Update()
	{
		isLeftAltHeld = Input.GetKey(KeyCode.LeftAlt);
		isLeftShiftHeld = Input.GetKey(KeyCode.LeftShift);

		if (clone != null)
			clone.transform.position = Input.mousePosition;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		dragItem = currentSlot.currentItem;
		
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (isLeftShiftHeld && dragItem != null)
			{
				SplitItemInHalf();
				return;
			}
			
			if (isLeftAltHeld && dragItem != null)
			{
				bool favorite = !dragItem.isFavorite;
				Inventory.instance.SetFavorite(dragItem.id, favorite);
				Debug.Log(dragItem + " is " + (favorite ? "favorite" : "not favorite"));
				return;
			}

			// Pick item up.
			if (!holdingItem && dragItem != null)
			{
				BeginDragItem();
				return;
			}

			// Drop item down.
			if (holdingItem)
			{
				// Drop if the other slot's item is null or has different id.
				if ((splittingItem && (sender == this || dragItem != null)) || !splittingItem)
					currentSlot.OnDrop(clone);
				
				else if (splittingItem && dragItem == null)
				{
					Inventory.instance.Add(clone.GetComponent<ClickableObject>().dragItem, true);
					currentSlot.OnDrop(clone);
				}

				ClearSingleton();
				return;
			}
		}

		else if (eventData.button == PointerEventData.InputButton.Right)
		{
			if (isLeftShiftHeld && dragItem != null)
			{
				SplitItemOneByOne();
				return;
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && !isLeftAltHeld && !isLeftShiftHeld)
			currentSlot.UseItem();
	}

	public static void ClearSingleton()
	{
		if (sender != null && clone != null)
		{
			sender.icon.color = Color.white;

			sender.dragItem = null;

			sender = null;
			holdingItem = false;
			splittingItem = false;

			Destroy(clone);
		}
	}

	private void BeginDragItem()
	{
		clone = Instantiate(gameObject, transform.root);
		clone.GetComponent<Image>().enabled = false;
		clone.transform.Find("Icon").GetComponent<Image>().raycastTarget = false;
		clone.transform.Find("Favorite Border").GetComponent<Image>().enabled = false;

		icon.color = new Color(.51f, .51f, .51f);

		holdingItem = true;
		sender = this;

		Debug.Log("You're dragging the " + dragItem.itemName);
	}

	private void SplitItemInHalf()
	{
		if (dragItem != null)
		{
			if (clone == null)
			{
				if (dragItem.quantity == 1)
				{
					BeginDragItem();
					return;
				}
				
				int half1 = dragItem.quantity / 2;
			
				Item split = Instantiate(dragItem);
				split.quantity = half1;
				split.name = dragItem.name;

				clone = Instantiate(gameObject, transform.root);
				clone.GetComponent<ClickableObject>().dragItem = split;

				clone.GetComponent<Image>().enabled = false;
				clone.transform.Find("Icon").GetComponent<Image>().raycastTarget = false;
				clone.transform.Find("Favorite Border").GetComponent<Image>().enabled = false;
				clone.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = half1.ToString();

				holdingItem = true;
				splittingItem = true;
				sender = this;
				
				Inventory.instance.UpdateQuantity(dragItem.id, -half1);
				return;
			}

			Item holdItem = clone.GetComponent<ClickableObject>().dragItem;

			int half2 = dragItem.quantity / 2;
			holdItem.quantity += half2;

			clone.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = holdItem.quantity.ToString();

			if (dragItem.quantity <= 1)
			{
				Inventory.instance.Remove(dragItem, true);
				holdItem.quantity++;  // Because 1 / 2 == 0, we need to increase the quantity by 1.
				clone.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = holdItem.quantity.ToString();
				return;
			}

			Inventory.instance.UpdateQuantity(dragItem.id, -half2);
		}
	}

	private void SplitItemOneByOne()
	{
		if (dragItem != null)
		{
			if (clone == null)
			{
				if (dragItem.quantity == 1)
				{
					BeginDragItem();
					return;
				}
				
				Item split = Instantiate(dragItem);
				split.quantity = 1;
				split.name = dragItem.name;

				clone = Instantiate(gameObject, transform.root);
				clone.GetComponent<ClickableObject>().dragItem = split;

				clone.GetComponent<Image>().enabled = false;
				clone.transform.Find("Icon").GetComponent<Image>().raycastTarget = false;
				clone.transform.Find("Favorite Border").GetComponent<Image>().enabled = false;
				clone.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = "1";

				holdingItem = true;
				splittingItem = true;
				sender = this;

				Inventory.instance.UpdateQuantity(dragItem.id, -1);
				return;
			}

			Item holdItem = clone.GetComponent<ClickableObject>().dragItem;
			holdItem.quantity++;
			clone.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = holdItem.quantity.ToString();

			if (dragItem.quantity == 1)
			{
				Inventory.instance.Remove(dragItem, true);
				return;
			}

			Inventory.instance.UpdateQuantity(dragItem.id, -1);
		}
	}
}
