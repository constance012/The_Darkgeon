using TMPro;
using UnityEngine;

/// <summary>
/// A base scriptable object to create a debuff.
/// </summary>
public class Debuff : ScriptableObject
{
	[Header("Base Info")]
	[Space]
	public string debuffName;
	[TextArea(5, 10)] public string description;
	public float duration;

	public Sprite icon;
	public GameObject visualEffect;

	protected PlayerStats player;
	protected Animator playerAnim;
	protected TextMeshProUGUI durationUI;

	public virtual void TakeEffect()
	{
		if (player == null)
		{
			player = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
			
			playerAnim = player.GetComponent<Animator>();
			
			durationUI = GameObject.FindWithTag("UI Canvas").transform.
									Find($"Player UI/Debuff Panel/{debuffName}/Duration").GetComponent<TextMeshProUGUI>();
		}
	}
}
