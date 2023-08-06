using UnityEngine;

[RequireComponent (typeof(Animator), typeof(CanvasGroup))]
public class FadeOutPanel : Singleton<FadeOutPanel>
{
	private CanvasGroup canvasGroup;
	private Animator animator;
	
	// Properties.
	public static float Alpha
	{
		get { return instance.canvasGroup.alpha; }
	}

	protected override void Awake()
	{
		base.Awake();

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