using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ClickableObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
	[Header("Slot Type")]
	[Space]
	public bool isChestSlot;

	[Header("References")]
	[Space]

	public Item dragItem;
	[SerializeField] private GameObject droppedItemPrefab;
	[SerializeField] private Transform player;

	// Private fields.
	private InventorySlot currentSlot;
	private ChestSlot currentChestSlot;
	private Image icon;
	private TooltipTrigger tooltip;

	private bool isLeftAltHeld;
	private bool isLeftShiftHeld;
	private bool isLeftControlHeld;

	private float mouseHoldTimer = 1f;
	private bool isMouseButtonHeld;

	private bool isCoroutineRunning;

	// Static fields.

	// A clone that "ships" the current item in this slot.
	public static GameObject clone { get; set; }
	// Singleton reference to the sender script.
	public static ClickableObject sender { get; private set; }
	public static bool holdingItem { get; set; }
	public static bool splittingItem { get; set; }

	private void Awake()
	{
		if (!isChestSlot)
			currentSlot = transform.GetComponentInParent<InventorySlot>();
		else
			currentChestSlot = transform.GetComponentInParent<ChestSlot>();

		icon = transform.Find("Icon").GetComponent<Image>();
		tooltip = GetComponentInParent<TooltipTrigger>();
		player = GameObject.FindWithTag("Player").transform;
	}

	private void Update()
	{
		if (isCoroutineRunning)
			Debug.Log("Coroutine is running.");

		isLeftAltHeld = Input.GetKey(KeyCode.LeftAlt);
		isLeftShiftHeld = Input.GetKey(KeyCode.LeftShift);
		isLeftControlHeld = Input.GetKey(KeyCode.LeftControl);

		if (splittingItem && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
		{
			mouseHoldTimer -= Time.deltaTime;

			if (mouseHoldTimer <= 0f)
				isMouseButtonHeld = true;
		}

		else
		{
			mouseHoldTimer = 1f;
			isMouseButtonHeld = false;
		}

		if (clone != null)
			clone.transform.position = Input.mousePosition;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		dragItem = isChestSlot ? currentChestSlot.currentItem : currentSlot.currentItem;
		tooltip.HideTooltip();

		if (dragItem == null && !holdingItem)
			return;

		if (eventData.button == PointerEventData.InputButton.Left)
		{
			// Split a stack in half.
			if (isLeftShiftHeld)
			{
				if (!isCoroutineRunning)
					StartCoroutine(ContinueSplitting(eventData.button));

				SplitItemInHalf();
				return;
			}
			
			// Set an item as favorite.
			if (isLeftAltHeld && !isChestSlot)
			{
				bool favorite = !dragItem.isFavorite;
				Inventory.instance.SetFavorite(dragItem.id, favorite);
				Debug.Log(dragItem + " is " + (favorite ? "favorite" : "not favorite"));
				return;
			}

			// Quick deposit an item between the currently opening chest and the inventory.
			if (isLeftControlHeld && ChestStorage.instance.openedChest != null)
			{
				QuickDeposit();
				return;
			}

			// Pick item up.
			if (!holdingItem)
			{
				BeginDragItem();
				return;
			}

			// Drop item down.
			if (holdingItem)
			{
				if (!isChestSlot)
					currentSlot.OnDrop(clone);
				else
					currentChestSlot.OnDrop(clone);

				ClearSingleton();
				return;
			}
		}

		else if (eventData.button == PointerEventData.InputButton.Right)
		{
			if (isLeftShiftHeld)
			{
				if (!isCoroutineRunning)
					StartCoroutine(ContinueSplitting(eventData.button));

				SplitItemOneByOne();
				return;
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && !isLeftAltHeld && !isLeftShiftHeld)
			currentSlot?.UseItem();
	}

	// This method is called from an event trigger.
	public void DisposeItem()
	{
		Debug.Log("Outside of inventory area.");

		if (clone != null)
		{
			Item disposeItem = clone.GetComponent<ClickableObject>().dragItem;

			if (!disposeItem.isFavorite || sender.isChestSlot)
			{
				// Set up the drop.
				ItemPickup droppedItem = droppedItemPrefab.GetComponent<ItemPickup>();

				droppedItem.itemPrefab = disposeItem;
				droppedItem.itemPrefab.slotIndex = -1;
				droppedItem.itemPrefab.isFavorite = false;
				droppedItem.player = player;

				// Make the drop.
				GameObject droppedItemObj = Instantiate(droppedItemPrefab, player.position, Quaternion.identity);

				droppedItemObj.name = disposeItem.name;
				droppedItemObj.transform.SetParent(GameObject.Find("Items").transform);

				// Add force to the dropped item.
				Rigidbody2D rb2d = droppedItemObj.GetComponent<Rigidbody2D>();

				Vector3 screenPlayerPos = Camera.main.WorldToScreenPoint(player.position);
				Vector3 aimingDir = Input.mousePosition - screenPlayerPos;

				rb2d.AddForce(5f * aimingDir.normalized, ForceMode2D.Impulse);

				if (!splittingItem)
				{
					if (!sender.isChestSlot)
						Inventory.instance.Remove(disposeItem);
					else
						ChestStorage.instance.Remove(disposeItem);
				}
			}

			// Update the quantity if we dispose a favorite item via splitting.
			else if (disposeItem.isFavorite && splittingItem)
			{
				if (sender.isChestSlot)
					ChestStorage.instance.UpdateQuantity(disposeItem.id, disposeItem.quantity);
				else
					Inventory.instance.UpdateQuantity(disposeItem.id, disposeItem.quantity);
			}

			ClearSingleton();
		}
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

		Inventory.instance.onItemChanged?.Invoke();
		ChestStorage.instance.onItemChanged?.Invoke();
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

	private void QuickDeposit()
	{
		if (dragItem.isFavorite)
			return;

		dragItem.slotIndex = -1;

		holdingItem = true;
		sender = this;

		if (!sender.isChestSlot)
		{
			ChestStorage.instance.Add(sender.dragItem);
			Inventory.instance.Remove(sender.dragItem);
		}
		else
		{
			Inventory.instance.Add(sender.dragItem);
			ChestStorage.instance.Remove(sender.dragItem);
		}

		holdingItem = false;
		sender = null;
	}

	private void SplitItemInHalf()
	{	
		// Create a clone if it doesn't already exist.
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
				
			if (!isChestSlot)
				Inventory.instance.UpdateQuantity(dragItem.id, -half1);
			else
				ChestStorage.instance.UpdateQuantity(dragItem.id, -half1);
			return;
		}

		// Can only split at 1 slot or the item is existing.
		if (sender != this || dragItem.quantity == 0)
			return;

		Item holdItem = clone.GetComponent<ClickableObject>().dragItem;

		int half2 = dragItem.quantity / 2;
		holdItem.quantity += half2;

		clone.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = holdItem.quantity.ToString();

		if (dragItem.quantity <= 1)
		{
			if (!isChestSlot)
			{
				Inventory.instance.items.Remove(dragItem);
				currentSlot.currentItem = null;
			}
			else
			{
				ChestStorage.instance.openedChest.storedItem.Remove(dragItem);
				currentChestSlot.currentItem = null;
			}

			sender.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = "0";
			icon.color = new Color(.51f, .51f, .51f);

			holdItem.quantity++;  // Because 1 / 2 == 0, we need to increase the quantity by 1.
			clone.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = holdItem.quantity.ToString();
				
			return;
		}

		if (!isChestSlot)
			Inventory.instance.UpdateQuantity(dragItem.id, -half2);
		else
			ChestStorage.instance.UpdateQuantity(dragItem.id, -half2);
	}

	private void SplitItemOneByOne()
	{
		// Create a clone if it doesn't already exist.
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

			if (!isChestSlot)
				Inventory.instance.UpdateQuantity(dragItem.id, -1);
			else
				ChestStorage.instance.UpdateQuantity(dragItem.id, -1);

			return;
		}

		// Can only split at 1 slot or the item is existing.
		if (sender != this || dragItem.quantity == 0)
			return;

		Item holdItem = clone.GetComponent<ClickableObject>().dragItem;
		holdItem.quantity++;

		clone.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = holdItem.quantity.ToString();

		if (dragItem.quantity == 1)
		{
			if (!isChestSlot)
			{
				Inventory.instance.items.Remove(dragItem);
				currentSlot.currentItem = null;
			}
			else
			{
				ChestStorage.instance.openedChest.storedItem.Remove(dragItem);
				currentChestSlot.currentItem = null;
			}

			sender.transform.Find("Quantity").GetComponent<TextMeshProUGUI>().text = "0";
			icon.color = new Color(.51f, .51f, .51f);

			return;
		}

		if (!isChestSlot)
			Inventory.instance.UpdateQuantity(dragItem.id, -1);
		else
			ChestStorage.instance.UpdateQuantity(dragItem.id, -1);
	}

	private IEnumerator ContinueSplitting(PointerEventData.InputButton heldMouseButton)
	{
		isCoroutineRunning = true;

		yield return new WaitForSeconds(1f);

		if (!isMouseButtonHeld || !isLeftShiftHeld)
		{
			isCoroutineRunning = false;
			yield break;
		}

		Debug.Log("Continue splitting");
		
		while (isMouseButtonHeld && isLeftShiftHeld)
		{
			if (!isChestSlot && !Inventory.instance.IsExisting(dragItem.id))
			{
				isCoroutineRunning = false;
				yield break;
			}

			if (isChestSlot && !ChestStorage.instance.IsExisting(dragItem.id))
			{
				isCoroutineRunning = false;
				yield break;
			}

			switch (heldMouseButton)
			{
				case PointerEventData.InputButton.Left:
					SplitItemInHalf();
					Debug.Log("Splitting by a half");
					break;

				case PointerEventData.InputButton.Right:
					SplitItemOneByOne();
					Debug.Log("Splitting one by one");
					break;
			}

			yield return new WaitForSeconds(.5f);
		}

		isCoroutineRunning = false;
	}
}
