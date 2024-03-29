using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.U2D;

public class ClickableObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
	public enum StorageType { Inventory, Chest }

	[Header("Storage Type")]
	[Space]
	public StorageType storageType;
	[ReadOnly] public ItemStorage currentStorage;
	[ReadOnly] public ItemStorage otherStorage;

	[Header("References")]
	[Space]
	public Item dragItem;
	[SerializeField] private GameObject droppedItemPrefab;
	[SerializeField] private Transform player;

	public bool IsChestSlot => storageType == StorageType.Chest;
	public bool IsInventorySlot => storageType == StorageType.Inventory;
	public bool FromSameStorageSlot<TSlot>() where TSlot : StorageSlot => currentSlot.GetType() == typeof(TSlot);

	// Private fields.
	private StorageSlot currentSlot;
	private Image icon;
	private TooltipTrigger tooltip;

	private bool isLeftAltHeld;
	private bool isLeftShiftHeld;
	private bool isLeftControlHeld;

	private float mouseHoldTimer = 1f;
	private bool isMouseButtonHeld;

	private bool isCoroutineRunning;

	// Static properties.

	// A clone that "ships" the current item in this slot.
	public static GameObject clone { get; set; }
	// Singleton reference to the sender script.
	public static ClickableObject sender { get; private set; }
	public static bool holdingItem { get; set; }
	public static bool splittingItem { get; set; }

	private void Awake()
	{
		InitializeItemStorage();

		icon = transform.GetComponentInChildren<Image>("Icon");
		tooltip = GetComponentInParent<TooltipTrigger>();
		player = GameObject.FindWithTag("Player").transform;
	}

	private void Update()
	{
		if (isCoroutineRunning)
			Debug.Log("Coroutine is running.", this);

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
		dragItem = currentSlot.currentItem;
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
			
			// Set an item in Inventory as favorite.
			if (isLeftAltHeld && IsInventorySlot)
			{
				bool favorite = !dragItem.isFavorite;
				currentStorage.SetFavorite(dragItem.id, favorite);
				Debug.Log(dragItem + " is " + (favorite ? "favorite" : "not favorite"));
				return;
			}

			// Quick deposit an item between two storages.
			if (isLeftControlHeld && otherStorage != null)
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
				currentSlot.OnDrop(clone);

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

	#region Item Manipulation.
	// This method is called from an event trigger.
	public void DisposeItem()
	{
		Debug.Log("Outside of inventory area.");

		if (clone == null)
			return;

		Item disposeItem = clone.GetComponent<ClickableObject>().dragItem;

		if (!disposeItem.isFavorite || sender.IsChestSlot)
		{
			// Set up the drop.
			ItemPickup droppedItem = droppedItemPrefab.GetComponent<ItemPickup>();

			droppedItem.itemSO = disposeItem;
			droppedItem.itemSO.slotIndex = -1;
			droppedItem.itemSO.isFavorite = false;
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
				currentStorage.Remove(disposeItem);
		}

		// Update the quantity if we try disposing a favorite item via splitting.
		else if (disposeItem.isFavorite && splittingItem)
		{
			currentStorage.UpdateQuantity(disposeItem.id, disposeItem.quantity);
		}

		ClearSingleton();
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
		CreateClone();

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

		otherStorage.Add(sender.dragItem);
		currentStorage.Remove(sender.dragItem);

		holdingItem = false;
		sender = null;
	}

	private void InitializeItemStorage()
	{
		switch (storageType)
		{
			case StorageType.Inventory:
				currentStorage = GetComponentInParent<Inventory>();
				otherStorage = null;  // TODO - assign this when any storages other than the Invetory is Enable.
				break;

			case StorageType.Chest:
				currentStorage = GetComponentInParent<ChestStorage>();
				otherStorage = transform.root.GetComponentInChildren<Inventory>();
				break;
		}

		currentSlot = GetComponentInParent<StorageSlot>();
	}

	private void CreateClone()
	{
		clone = Instantiate(gameObject, transform.root);

		clone.GetComponent<RectTransform>().pivot = new Vector2(.48f, .55f);

		clone.GetComponent<Image>().enabled = false;
		clone.transform.GetComponentInChildren<Image>("Icon").raycastTarget = false;
		clone.transform.GetComponentInChildren<Image>("Favorite Border").enabled = false;

		ClickableObject cloneData = clone.GetComponent<ClickableObject>();
		cloneData.currentStorage = currentStorage;
		cloneData.otherStorage = otherStorage;
		cloneData.currentSlot = currentSlot;
	}
	#endregion

	#region Item Splitting.
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

			CreateClone();
			clone.GetComponent<ClickableObject>().dragItem = split;
			clone.transform.GetComponentInChildren<TextMeshProUGUI>("Quantity").text = half1.ToString();

			holdingItem = true;
			splittingItem = true;
			sender = this;
				
			currentStorage.UpdateQuantity(dragItem.id, -half1);

			return;
		}

		// Can only split at 1 slot or the item is existing.
		if (sender != this || dragItem.quantity == 0)
			return;

		Item holdItem = clone.GetComponent<ClickableObject>().dragItem;

		int half2 = dragItem.quantity / 2;
		holdItem.quantity += half2;

		clone.transform.GetComponentInChildren<TextMeshProUGUI>("Quantity").text = holdItem.quantity.ToString();

		if (dragItem.quantity <= 1)
		{
			currentStorage.RemoveWithoutNotify(dragItem);
			currentSlot.currentItem = null;

			sender.transform.GetComponentInChildren<TextMeshProUGUI>("Quantity").text = "0";
			icon.color = new Color(.51f, .51f, .51f);

			holdItem.quantity++;  // Because 1 / 2 == 0, we need to increase the quantity by 1.
			clone.transform.GetComponentInChildren<TextMeshProUGUI>("Quantity").text = holdItem.quantity.ToString();
				
			return;
		}

		currentStorage.UpdateQuantity(dragItem.id, -half2);
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

			CreateClone();
			clone.GetComponent<ClickableObject>().dragItem = split;
			clone.transform.GetComponentInChildren<TextMeshProUGUI>("Quantity").text = "1";

			holdingItem = true;
			splittingItem = true;
			sender = this;

			currentStorage.UpdateQuantity(dragItem.id, -1);

			return;
		}

		// Can only split at 1 slot or the item is existing.
		if (sender != this || dragItem.quantity == 0)
			return;

		Item holdItem = clone.GetComponent<ClickableObject>().dragItem;
		holdItem.quantity++;

		clone.transform.GetComponentInChildren<TextMeshProUGUI>("Quantity").text = holdItem.quantity.ToString();

		if (dragItem.quantity == 1)
		{
			currentStorage.RemoveWithoutNotify(dragItem);
			currentSlot.currentItem = null;

			sender.transform.GetComponentInChildren<TextMeshProUGUI>("Quantity").text = "0";
			icon.color = new Color(.51f, .51f, .51f);

			return;
		}

		currentStorage.UpdateQuantity(dragItem.id, -1);
	}

	private IEnumerator ContinueSplitting(PointerEventData.InputButton heldMouseButton)
	{
		isCoroutineRunning = true;

		// Wait for a second, continue splitting if the user holds down the mouse button.
		yield return new WaitForSeconds(1f);

		if (!isMouseButtonHeld || !isLeftShiftHeld)
		{
			isCoroutineRunning = false;
			yield break;
		}

		float delayedTime = .5f;

		while (isMouseButtonHeld && isLeftShiftHeld)
		{
			if (!currentStorage.IsExisting(dragItem.id))
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

			yield return new WaitForSeconds(delayedTime);
			
			delayedTime -= Time.deltaTime;
			delayedTime = Mathf.Max(.1f, delayedTime);  // Clamp the delayed time above 0.1 secs.
		}

		isCoroutineRunning = false;
	}
	#endregion
}
