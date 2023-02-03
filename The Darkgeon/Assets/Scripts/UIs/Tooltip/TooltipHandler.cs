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
			Debug.LogWarning("More than one instance of Tooltip Handler found!!");
			Destroy(gameObject);
			return;
		}
	}

	public static IEnumerator Show(string contentText, string headerText = "", float delay = .2f)
	{
		instance.tooltip.SetText(contentText, headerText);

		if (delay < .2f)
			yield return new WaitForSeconds(.2f);
		else
			yield return new WaitForSeconds(delay);

		instance.tooltip.gameObject.SetActive(true);
	}

	public static void Hide()
	{
		instance.tooltip.gameObject.SetActive(false);
	}
}