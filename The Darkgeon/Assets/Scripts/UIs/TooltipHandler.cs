using System.Collections;
using UnityEngine;

public class TooltipHandler : MonoBehaviour
{
	private static TooltipHandler instance;

	[SerializeField] private Tooltip tooltip;
	
	private void Awake()
	{
		tooltip = transform.Find("Tooltip").GetComponent<Tooltip>();
		
		if (instance == null)
			instance = this;
		else
		{
			Destroy(gameObject);
			return;
		}
	}

	public static IEnumerator Show(string contentText, string headerText = "")
	{
		instance.tooltip.SetText(contentText, headerText);

		yield return new WaitForSeconds(1.5f);

		instance.tooltip.gameObject.SetActive(true);
	}

	public static void Hide()
	{
		instance.tooltip.gameObject.SetActive(false);
	}
}
