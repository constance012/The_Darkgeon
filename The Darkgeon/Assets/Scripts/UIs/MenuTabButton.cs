using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class MenuTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField] private TabGroup tabGroup;
	public TextMeshProUGUI text;

	private void Awake()
	{
		tabGroup = GetComponentInParent<TabGroup>();
		text = GetComponent<TextMeshProUGUI>();
	}

	private void Start()
	{
		tabGroup.Subscribe(this);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		tabGroup.OnTabEnter(this);
	}
	
	public void OnPointerExit(PointerEventData eventData)
	{
		tabGroup.OnTabExit(this);
	}
	
	public void OnPointerClick(PointerEventData eventData)
	{
		tabGroup.OnTabSelected(this);
	}
}
