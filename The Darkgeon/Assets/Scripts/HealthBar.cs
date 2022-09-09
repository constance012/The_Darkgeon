using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	// References.
	[SerializeField] private Slider slider;
	[SerializeField] private Image fillRect;
	public Gradient gradient;

	void Awake()
	{
		slider = GetComponent<Slider>();
		fillRect = transform.Find("Fill").GetComponent<Image>();
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
	}
}
