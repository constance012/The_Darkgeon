using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Represents a UI text when a new item is added to the player's Inventory.
/// </summary>
public class NewItemNotifier : MonoBehaviour
{
	public float aliveTime = 5f;

	public static UnityEvent onLastSiblingDestroy;

	private RectTransform rTransform;
	private TextMeshProUGUI displayText;

	private float disappearSpeed = 70f;

	private void Awake()
	{
		rTransform = GetComponent<RectTransform>();
		displayText = GetComponent<TextMeshProUGUI>();

		if (onLastSiblingDestroy == null)
			onLastSiblingDestroy = new UnityEvent();

		onLastSiblingDestroy.RemoveAllListeners();
		onLastSiblingDestroy.AddListener(() => 
		{
			Animator titleAnimator = transform.parent.parent.Find("Title").GetComponent<Animator>();
			titleAnimator.SetBool("Slide Out", true); 
		});
	}

	private void Update()
	{
		// Begin disposing, one after another.
		if (aliveTime <= 0f && transform.GetSiblingIndex() == 0)
		{
			if (displayText.fontSize >= 10f)
				displayText.GetComponent<Animator>().SetTrigger("Disappear");
			
			float newHeight = rTransform.rect.height;

			newHeight -= disappearSpeed * Time.deltaTime;

			rTransform.sizeDelta = new Vector2(rTransform.rect.width, newHeight);

			if (rTransform.rect.height <= 0f)
			{
				if (transform.parent.childCount == 1)
					onLastSiblingDestroy?.Invoke();

				Destroy(gameObject);
			}

			return;
		}

		if (transform.GetSiblingIndex() < 4 && aliveTime > 0f)
			aliveTime -= Time.deltaTime;
	}

	public static NewItemNotifier Generate(GameObject samplePrefab, Item target)
	{
		Transform parentPanel = GameObject.FindWithTag("UI Canvas").transform.Find("New Items Panel");
		
		// Toggle the title animation.
		GameObject title = parentPanel.Find("Title").gameObject;
		if (!title.activeInHierarchy)
			title.SetActive(true);

		// Only slide in when it's already slid out.
		else if (title.GetComponent<Animator>().GetBool("Slide Out"))
			title.GetComponent<Animator>().SetBool("Slide Out", false);

		// Instantiate the game object.
		GameObject newItemObj = Instantiate(samplePrefab, parentPanel.Find("New Items List"));
		newItemObj.name = samplePrefab.name;

		NewItemNotifier newItem =  newItemObj.GetComponent<NewItemNotifier>();

		// Set the text.
		newItem.displayText.text = target.itemName.ToUpper() + " x" + target.quantity;
		newItem.displayText.color = target.rarity.color;
		
		return newItem;
	}
}