using UnityEngine;

/// <summary>
/// A scriptable object to create a debuff.
/// </summary>
[CreateAssetMenu(fileName = "New Debuff", menuName = "Debuff")]
public class Debuff : ScriptableObject
{
	public new string name;
	[TextArea(5, 10)] public string description;
	public float duration;

	public Sprite icon;
	public GameObject visualEffect;

	[Header("Health Related Debuff")]
	[Space]
	public int healthLossRate;
	public float baseHpLossDelay;
	public float hpLossDelay;
	public bool allowRegenerate;

	[Header("Movement Related Debuff")]
	[Space]
	[Range(.2f, .9f)] public float speedReduceFactor;
	[HideInInspector] public bool isSpeedReduced;
}
