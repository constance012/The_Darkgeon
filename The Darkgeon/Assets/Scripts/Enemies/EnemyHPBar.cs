using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents an health bar following the enemy around.
/// </summary>
public class EnemyHPBar : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private Slider hpSlider;
	[SerializeField] private Image fillRect;
	public Transform hpBarPos;

	[Header("Color")]
	[Space]
	public Gradient gradient;

	private void Awake()
	{
		hpSlider = GetComponent<Slider>();
		fillRect = transform.Find("Fill").GetComponent<Image>();
		hpBarPos = transform.parent.Find("HP Bar Pos");
	}

	private void Update()
	{
		transform.position = hpBarPos.position;
	}

	public void SetMaxHealth(int maxHP)
	{
		hpSlider.maxValue = maxHP;
		hpSlider.value = maxHP;
		
		fillRect.color = gradient.Evaluate(1f);
	}

	public void SetCurrentHealth(int currentHP)
	{
		hpSlider.value = currentHP;
		fillRect.color = gradient.Evaluate(hpSlider.normalizedValue);  // Return the value between 0f and 1f.
	}
}
