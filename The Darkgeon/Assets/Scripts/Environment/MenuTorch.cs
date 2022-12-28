using UnityEngine;

public class MenuTorch : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private ParticleSystem fire;
	[SerializeField] private ParticleSystem spark;
	[SerializeField] private ParticleSystem glow;
	[SerializeField] private GameObject pointLight2D;

	private void Awake()
	{
		fire = transform.Find("Fire").GetComponent<ParticleSystem>();
		spark = transform.Find("Spark").GetComponent<ParticleSystem>();
		glow = transform.Find("Glow").GetComponent<ParticleSystem>();
		pointLight2D = transform.Find("Point Light").gameObject;
	}

	private void Start()
	{
		Extinguish();
	}

	private void Update()
	{
		if (fire.particleCount == 1 && !pointLight2D.activeInHierarchy)
		{
			spark.Play();
			glow.Play();
			pointLight2D.SetActive(true);
		}
	}

	public void LightUp()
	{
		fire.Play();
	}

	public void Extinguish()
	{
		fire.Stop();
		spark.Stop();
		glow.Stop();
		fire.Clear();
		glow.Clear();
		pointLight2D.SetActive(false);
	}
}
