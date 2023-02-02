using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public string header;

	[Space]

	[TextArea(5, 10)] public string content;

	[Space]

	public float popupDelay;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(header) || !string.IsNullOrEmpty(content))
			StartCoroutine(TooltipHandler.Show(content, header, popupDelay));
	}
	
	public void OnPointerExit(PointerEventData eventData)
	{
		HideTooltip();
	}

	public void OnMouseEnter()
	{
		if (!string.IsNullOrEmpty(header) || !string.IsNullOrEmpty(content))
			StartCoroutine(TooltipHandler.Show(content, header, popupDelay));
	}

	public void OnMouseExit()
	{
		HideTooltip();
	}

	public void HideTooltip()
	{
		StopAllCoroutines();
		TooltipHandler.Hide();
	}
}
