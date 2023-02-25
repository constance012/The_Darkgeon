using TMPro;
using Unity.Android.Types;
using UnityEngine;
using UnityEngine.Events;

public class NewItemUI : MonoBehaviour
{
	public float aliveTime = 5f;

	public static UnityEvent onLastSiblingDestroy;

	private RectTransform rTransform;
	private TextMeshProUGUI displayText;

	private float disappearSpeed = 50f;
	private float smoothVel;

	private void Awake()
	{
		rTransform = GetComponent<RectTransform>();
		displayText = GetComponent<TextMeshProUGUI>();

		if (onLastSiblingDestroy == null)
			onLastSiblingDestroy = new UnityEvent();

		onLastSiblingDestroy.RemoveAllListeners();
		onLastSiblingDestroy.AddListener(() => transform.parent.parent.Find("Title").GetComponent<Animator>().SetBool("Slide Out", true));
	}

	private void Update()
	{
		if (aliveTime <= 0f)
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

		if (transform.GetSiblingIndex() < 4)
			aliveTime -= Time.deltaTime;
	}

	public static NewItemUI Generate(GameObject samplePrefab, Item target)
	{
		Transform parentPanel = GameObject.Find("UI Canvas").transform.Find("New Items Panel");
		
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

		NewItemUI newItem =  newItemObj.GetComponent<NewItemUI>();

		// Set the text.
		newItem.displayText.text = target.itemName.ToUpper() + " x" + target.quantity;
		newItem.displayText.color = target.rarity.color;
		
		return newItem;
	}
}