using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler
{
	[Header("Equipment Type")]
	[Space]
	public Equipment.EquipmentType slotType;

	[Header("Current Equipment")]
	[Space]
	public Equipment currentEquipment;
	public Sprite placeholder;

	private Image icon;

	private TooltipTrigger trigger;

	private void Awake()
	{
		icon = transform.Find("Icon").GetComponent<Image>();
		trigger = GetComponent<TooltipTrigger>();
	}

	private void Start()
	{
		icon.sprite = placeholder;
	}

	public void AddEquipment(Equipment target)
	{
		currentEquipment = target;

		icon.sprite = currentEquipment.icon;

		trigger.header = currentEquipment.itemName;
		trigger.content = currentEquipment.ToString();
		trigger.popupDelay = 1f;
	}

	public void ClearEquipment()
	{
		currentEquipment = null;
		icon.sprite = placeholder;

		trigger.header = slotType.ToString();
		trigger.content = "";
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && currentEquipment != null)
			EquipmentManager.instance.Unequip(currentEquipment);
	}
}
