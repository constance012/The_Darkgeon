using UnityEngine;

public class ResetPage : MonoBehaviour
{
	[Header("Page References")]
	[Space]

	[SerializeField] private AudioOptionPage audioPage;
	[SerializeField] private GraphicsOptionPage graphicsPage;
	[SerializeField] private ControlsOptionPage controlsPage;

	private void Awake()
	{
		audioPage = transform.parent.Find("Audio Page").GetComponent<AudioOptionPage>();
		graphicsPage = transform.parent.Find("Graphics Page").GetComponent<GraphicsOptionPage>();
		controlsPage = transform.parent.Find("Controls Page").GetComponent<ControlsOptionPage>();
	}

	public void ConfirmReset()
	{
		if (audioPage.gameObject.activeInHierarchy)
			audioPage.ResetToDefault();

		else if (graphicsPage.gameObject.activeInHierarchy)
			graphicsPage.ResetToDefault();

		else
			controlsPage.ResetToDefault();
	}
}
