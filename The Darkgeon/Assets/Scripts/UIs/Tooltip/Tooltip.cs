using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip : MonoBehaviour
{
	[Header("References")]
	[Space]

	[Header("Texts")]
	[Space]
	[SerializeField] private TextMeshProUGUI header;
	[SerializeField] private TextMeshProUGUI content;

	[Header("Others")]
	[Space]
	[SerializeField] private LayoutElement layoutElement;
	[SerializeField] private RectTransform rectTransform;

	private void Awake()
	{
		header = transform.Find("Header").GetComponent<TextMeshProUGUI>();
		content = transform.Find("Content").GetComponent<TextMeshProUGUI>();
		layoutElement = GetComponent<LayoutElement>();
		rectTransform = GetComponent<RectTransform>();
	}

	// Update is called once per frame
	private void Update()
	{
		Vector2 mousePos = Input.mousePosition;
		
		float pivotX = mousePos.x / Screen.width;
		float pivotY = mousePos.y / Screen.height;

		// Move the tooltip up if the mouse position is at the bottom half of the screen.
		// Otherwise, move the tooltip down.
		mousePos.y += pivotY < 0.5f ? 20f : -30f;
		
		rectTransform.pivot = new Vector2(pivotX, pivotY);
		transform.position = mousePos;
	}

	public void SetText(string contentText, string headerText = "")
	{
		// Hide the header gameobject if the header text is null or empty.
		if (string.IsNullOrEmpty(headerText))
			header.gameObject.SetActive(false);
		
		else
		{
			header.gameObject.SetActive(true);
			header.text = headerText.ToUpper();
		}

		content.text = contentText.ToUpper();

		// And finally, toggle the Layout Element depending on the width.
		layoutElement.enabled = (header.preferredWidth > layoutElement.preferredWidth ||
									content.preferredWidth > layoutElement.preferredWidth);
	}
}
