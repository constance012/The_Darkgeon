using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Torch : Interactable
{
	[Header("References")]
	[Space]
	[SerializeField] private ParticleSystem effects;
	[SerializeField] private Flickering pointLight;

	public bool PlayOnAwake
	{
		get { return effects.main.playOnAwake; }
		set
		{
			MainModule main = effects.main;
			main.playOnAwake = value;
		}
	}

	protected override void Awake()
	{
		effects = GetComponent<ParticleSystem>();
		pointLight = GetComponentInChildren<Flickering>();
	}

	public override void ExecuteRemoteLogic(bool state)
	{
		if (state)
			LightUp();
		else
			Extinguish();
	}

	private void LightUp()
	{
		effects.Play();
		pointLight.gameObject.SetActive(true);
	}

	private void Extinguish()
	{
		effects.Stop();
		pointLight.gameObject.SetActive(false);
	}
}
