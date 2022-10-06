using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private Slider hpSlider;
	[SerializeField] private Slider fxSlider;
	[SerializeField] private Image fillRect;
	[SerializeField] private TextMeshProUGUI healthText;

	[Header("Color")]
	[Space]
	public Gradient gradient;

	[Header("Fields")]
	[Space]
	public float fxTime = .2f;
	float smoothVel = .0f;

	void Awake()
	{
		hpSlider = GetComponent<Slider>();
		fxSlider = transform.Find("Deplete Effect").GetComponent<Slider>();

		fillRect = transform.Find("Fill").GetComponent<Image>();
		healthText = transform.Find("Text Background").Find("Health Text").GetComponent<TextMeshProUGUI>();
	}

	public void SetMaxHealth(int maxHP)
	{
		hpSlider.maxValue = maxHP;
		hpSlider.value = maxHP;

		fxSlider.maxValue = maxHP;
		fxSlider.value = maxHP;
		
		fillRect.color = gradient.Evaluate(1f);
		healthText.text = maxHP + " / " + hpSlider.maxValue;
	}

	public void SetCurrentHealth(int currentHP)
	{
		hpSlider.value = currentHP;
		fillRect.color = gradient.Evaluate(hpSlider.normalizedValue);  // Return the value between 0f and 1f.
		healthText.text = currentHP + " / " + hpSlider.maxValue;
	}

	public void PerformEffect()
	{
		fxSlider.value = Mathf.SmoothDamp(fxSlider.value, hpSlider.value, ref smoothVel, fxTime);
	}
}
