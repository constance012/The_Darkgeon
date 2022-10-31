using UnityEngine;

[CreateAssetMenu(fileName = "New Debuff")]
public class Debuff : ScriptableObject
{
	public new string name;
	public string description;
	public float duration;

	public Sprite icon;

	[Header("Health Related Debuffs")]
	[Space]
	public int healthLossRate;
	public bool canRegenerate;
}
