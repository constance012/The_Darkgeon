using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private Slider slider;
	[SerializeField] private Image fillRect;
	[SerializeField] private TextMeshProUGUI healthText;

	[Header("Color")]
	[Space]
	public Gradient gradient;

	void Awake()
	{
		slider = GetComponent<Slider>();
		fillRect = transform.Find("Fill").GetComponent<Image>();
		healthText = transform.Find("Text Background").Find("Health Text").GetComponent<TextMeshProUGUI>();
	}

	public void SetMaxHealth(int maxHP)
	{
		slider.maxValue = maxHP;
		slider.value = maxHP;
		fillRect.color = gradient.Evaluate(1f);
	}

	public void SetCurrentHealth(int currentHP)
	{
		slider.value = currentHP;
		fillRect.color = gradient.Evaluate(slider.normalizedValue);  // Return the value between 0f and 1f.
		healthText.text = currentHP + " / " + slider.maxValue;
	}
}
