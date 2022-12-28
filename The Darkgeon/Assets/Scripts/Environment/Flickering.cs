using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The torches' flickering effect.
/// </summary>
public class Flickering : MonoBehaviour
{
	// Reference.
	[SerializeField] private Light2D light2D;
	public float delayTime;
	public float maximumIntensity;

	private bool isFlickering = false;
	private float targetIntensity;
	private float smoothTime;
	private float smoothVel = 0f;

	private void Awake()
	{
		light2D = GetComponent<Light2D>();
	}

	private void Start()
	{
		light2D.intensity = 0f;
	}

	private void Update()
	{
		if (!isFlickering)
			StartCoroutine(Flicker());

		light2D.intensity = Mathf.SmoothDamp(light2D.intensity, targetIntensity, ref smoothVel, smoothTime);
	}

	private IEnumerator Flicker()
	{
		isFlickering = true;
		
		targetIntensity = Random.Range(.6f, maximumIntensity);
		delayTime = Random.Range(.3f, 1f);
		smoothTime = Random.Range(.1f, delayTime);
		
		yield return new WaitForSeconds(delayTime);

		targetIntensity = Random.Range(.6f, maximumIntensity);
		delayTime = Random.Range(.3f, 1f);
		smoothTime = Random.Range(.1f, delayTime);

		yield return new WaitForSeconds(delayTime);

		isFlickering = false;
	}

	// A custom random method that ultilizes the original System.Random class of C#.
	public static float NextFloatInRange(System.Random rnd, double min, double max)
	{
		return (float)(rnd.NextDouble() * (max - min) + min);
	}
}
