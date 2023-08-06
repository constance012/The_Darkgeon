using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeleteSaveSlotButton : Button
{
	[Header("Target Save Slot ID")]
	[Space]
	public int currentSlotIndex = -1;
	public string currentSlotID = "";

	[HideInInspector] public Rect rect;
	[SerializeField] private TextMeshProUGUI warningText;

	[ReadOnly] public bool isClicked;

	protected override void Awake()
	{
		base.Awake();
		rect = GetComponent<RectTransform>().rect;
		onClick.AddListener(UpdateWarningText);
	}

	public void UpdateWarningText()
	{
		isClicked = true;
		warningText.text = $"PERMANENTLY DELETE DATA ON SLOT #{currentSlotIndex + 1}?\n" +
						   $"THIS ACTION CAN NOT BE UNDONE.";
	}

	public void SetYPosition(float y)
	{
		transform.position = new Vector3(transform.position.x, y, transform.position.z);
	}

	public void DeactivateSelf()
	{
		interactable = false;
		isClicked = false;

		currentSlotIndex = -1;
		currentSlotID = "";
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (isClicked)
			return;

		base.OnPointerExit(eventData);
		DeactivateSelf();
	}
}
