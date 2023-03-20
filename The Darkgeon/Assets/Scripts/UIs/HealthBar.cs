using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Represents the player's health bar.
/// </summary>
public class HealthBar : MonoBehaviour
{
	// References.
	[Header("References")]
	[Space]
	[SerializeField] private Slider hpSlider;
	[SerializeField] private Slider fxSlider;

	[SerializeField] private TextMeshProUGUI healthText;

	[Header("Color")]
	[Space]
	public Gradient gradient;

	[Header("Fields")]
	[Space]
	public float fxTime = .2f;
	
	// Private fields
	private Image fillRect;
	private Image fxFillRect;

	private Color decreaseColor = new Color(.78f, 0f, 0f);
	private Color increaseColor = new Color(.2f, .81f, .15f, .39f);

	private float smoothVel = 0f;

	private void Awake()
	{
		hpSlider = GetComponent<Slider>();
		fxSlider = transform.Find("Deplete Effect").GetComponent<Slider>();

		fillRect = transform.Find("Fill").GetComponent<Image>();
		fxFillRect = transform.Find("Deplete Effect/Effect Fill").GetComponent<Image>();

		healthText = transform.Find("Text Display/Health Text").GetComponent<TextMeshProUGUI>();
	}

	public void OnHealthChanged()
	{
		fillRect.color = gradient.Evaluate(hpSlider.normalizedValue);  // Return the value between 0f and 1f.
	}

	public void SetMaxHealth(float maxHP, bool initialize = true)
	{
		hpSlider.maxValue = maxHP;
		fxSlider.maxValue = maxHP;

		if (initialize)
		{
			hpSlider.value = maxHP;
			fxSlider.value = maxHP;
			fillRect.color = gradient.Evaluate(1f);
			healthText.text = Mathf.Round(maxHP) + " / " + hpSlider.maxValue;
		}		
	}

	public void SetCurrentHealth(float currentHP)
	{
		StopAllCoroutines();

		// If health is decreasing.
		if (hpSlider.value >= currentHP)
		{
			fxFillRect.color = decreaseColor;

			hpSlider.value = currentHP;
			healthText.text = Mathf.Round(hpSlider.value) + " / " + hpSlider.maxValue;
		}

		// If health is increasing.
		else
		{
			fxFillRect.color = increaseColor;

			fxSlider.value = currentHP;
			healthText.text = Mathf.Round(fxSlider.value) + " / " + hpSlider.maxValue;
		}

		StartCoroutine(PerformEffect());
	}

	private IEnumerator PerformEffect()
	{
		yield return new WaitForSeconds(.2f);
		
		if (fxFillRect.color == decreaseColor)
			while (fxSlider.value != hpSlider.value)
			{
				yield return new WaitForEndOfFrame();

				fxSlider.value = Mathf.SmoothDamp(fxSlider.value, hpSlider.value, ref smoothVel, fxTime);
			}

		else if (fxFillRect.color == increaseColor)
			while (fxSlider.value != hpSlider.value)
			{
				yield return new WaitForEndOfFrame();

				hpSlider.value = Mathf.SmoothDamp(hpSlider.value, fxSlider.value, ref smoothVel, fxTime);
			}
	}
}
