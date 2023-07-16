using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Represents a UI text notifier when a new item is added to the player's Inventory.
/// </summary>
public class NewItemNotifier : MonoBehaviour
{
	public float aliveTime = 5f;

	public static UnityEvent onLastSiblingDestroy = new UnityEvent();

	// Private fields.
	private static Transform _parentPanel;
	private static Animator _title;
	private static bool _isTitleSlidOut = true;

	private TextMeshProUGUI _displayText;

	private void Awake()
	{
		_displayText = GetComponent<TextMeshProUGUI>();

		onLastSiblingDestroy.RemoveAllListeners();
		onLastSiblingDestroy.AddListener(HideTitle);
	}

	private void Update()
	{
		// Begin disposing, one after another.
		if (aliveTime <= 0f && transform.GetSiblingIndex() == 0)
		{
			BeginDisposing();
			return;
		}

		if (transform.GetSiblingIndex() < 4 && aliveTime > 0f)
			aliveTime -= Time.deltaTime;
	}

	public static NewItemNotifier Generate(GameObject samplePrefab, Item target)
	{
		if (_parentPanel == null)
		{
			_parentPanel = GameObject.FindWithTag("UI Canvas").transform.Find("New Items Panel");
			_title = _parentPanel.GetComponentInChildren<Animator>("Title");
		}

		// Only slide in when it's already slid out.
		if (_isTitleSlidOut)
		{
			_title.Play("Slide In");
			_isTitleSlidOut = false;
		}

		// Instantiate the game object.
		GameObject newItemObj = Instantiate(samplePrefab, _parentPanel.Find("New Items List"));
		newItemObj.name = samplePrefab.name;

		NewItemNotifier newItem =  newItemObj.GetComponent<NewItemNotifier>();

		// Set the text.
		newItem._displayText.text = target.itemName.ToUpper() + " x" + target.quantity;
		newItem._displayText.color = target.rarity.color;
		
		return newItem;
	}

	private void BeginDisposing()
	{
		if (_displayText.fontSize >= 10f)
			_displayText.GetComponent<Animator>().Play("Get Smaller");

		if (_displayText.fontSize <= 0f)
		{
			if (transform.parent.childCount == 1)
				onLastSiblingDestroy?.Invoke();

			Destroy(gameObject);
		}
	}

	private void HideTitle()
	{
		_title.Play("Slide Out");
		_isTitleSlidOut = true;
	}
}