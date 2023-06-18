using UnityEngine;

[RequireComponent (typeof(Animator), typeof(CanvasGroup))]
public class FadeOutPanel : MonoBehaviour
{
	private static FadeOutPanel instance;

	private CanvasGroup canvasGroup;
	private Animator animator;
	
	// Properties.
	public static float alpha
	{
		get { return instance.canvasGroup.alpha; }
	}

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than 1 instance of Fade Out Panel found!! Destroy the newest one.");
			Destroy(gameObject);
			return;
		}

		canvasGroup = GetComponent<CanvasGroup>();
		animator = GetComponent<Animator>();
	}

	public static void FadeOut()
	{
		instance.animator.Play("Increase Alpha");
	}

	public static void FadeIn()
	{
		instance.animator.Play("Decrease Alpha");
	}
}