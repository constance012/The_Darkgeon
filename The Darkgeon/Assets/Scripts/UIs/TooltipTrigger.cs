using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public string header;
	[TextArea(5, 10)] public string content;

	public void OnPointerEnter(PointerEventData eventData)
	{
		StartCoroutine(TooltipHandler.Show(content, header));
	}
	
	public void OnPointerExit(PointerEventData eventData)
	{
		StopAllCoroutines();
		TooltipHandler.Hide();
	}

	public void OnMouseEnter()
	{
		StartCoroutine(TooltipHandler.Show(content, header));
	}

	public void OnMouseExit()
	{
		StopAllCoroutines();
		TooltipHandler.Hide();
	}
}
