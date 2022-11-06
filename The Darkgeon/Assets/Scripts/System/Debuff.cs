using UnityEngine;

[CreateAssetMenu(fileName = "New Debuff")]
public class Debuff : ScriptableObject
{
	public new string name;
	public string description;
	public float duration;

	public Sprite icon;

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
