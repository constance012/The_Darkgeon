using System.Collections;
using UnityEngine;

public class TooltipHandler : MonoBehaviour
{
	private static TooltipHandler instance;

	[SerializeField] private Tooltip tooltip;
	private bool isShowed;
	
	private void Awake()
	{
		tooltip = transform.Find("Tooltip").GetComponent<Tooltip>();
		
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Tooltip Handler found!! Destroy the newest one.");
			Destroy(gameObject);
			return;
		}
	}

	public static IEnumerator Show(string contentText, string headerText = "", float delay = .2f)
	{
		if (!instance.isShowed)
		{
			instance.tooltip.SetText(contentText, headerText);

			yield return new WaitForSeconds(Mathf.Clamp(delay, .2f, delay));

			instance.tooltip.gameObject.SetActive(true);
			instance.isShowed = true;
		}
	}

	public static void Hide()
	{
		if (instance.isShowed)
		{
			instance.tooltip.gameObject.SetActive(false);
			instance.isShowed = false;
		}
	}
}
