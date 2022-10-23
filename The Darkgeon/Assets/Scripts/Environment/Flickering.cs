using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Flickering : MonoBehaviour
{
	// Reference.
	[SerializeField] private Light2D light2D;
	public float flickerTime = 1f;

	private void Awake()
	{
		light2D = GetComponent<Light2D>();
	}

	private void Start()
	{
		StartCoroutine(Flicker());
	}

	private IEnumerator Flicker()
	{
		while (true)
		{
			light2D.intensity = Random.Range(.9f, 1.3f);

			yield return new WaitForSeconds(flickerTime);
		}
	}

	// A custom random method that ultilizes the original System.Random class of C#.
	public static float NextFloatInRange(System.Random rnd, double min, double max)
	{
		return (float)(rnd.NextDouble() * (max - min) + min);
	}
}
