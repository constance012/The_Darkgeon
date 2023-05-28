using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using CSTGames.Utility;

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

	[Header("Effects")]
	[Space]
	public float fxTime = .2f;
	public float fxDelay;
	
	// Private fields
	private Image fillRect;
	private Image fxFillRect;
	private Image screenBloodBorder;

	private Color decreaseColor = new Color(.78f, 0f, 0f);
	private Color increaseColor = new Color(.2f, .81f, .15f, .39f);

	private float smoothVel = 0f;

	private void Awake()
	{
		hpSlider = GetComponent<Slider>();
		fxSlider = transform.Find("Deplete Effect").GetComponent<Slider>();

		fillRect = transform.Find("Fill").GetComponent<Image>();
		fxFillRect = transform.Find("Deplete Effect/Effect Fill").GetComponent<Image>();
		screenBloodBorder = transform.root.Find("Screen Blood").GetComponent<Image>();

		healthText = transform.Find("Text Display/Health Text").GetComponent<TextMeshProUGUI>();
	}

	public void OnHealthChanged()
	{
		fillRect.color = gradient.Evaluate(hpSlider.normalizedValue);  // Return the value between 0f and 1f.

		float alpha = 1f - hpSlider.normalizedValue;
		screenBloodBorder.color = new Color(1f, 1f, 1f, alpha);
	}

	public void SetMaxHealth(float maxHP, bool initialize = true)
	{
		hpSlider.maxValue = maxHP;
		fxSlider.maxValue = maxHP;

		if (initialize)
		{
			hpSlider.value = maxHP;
			fxSlider.value = maxHP;
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
		yield return new WaitForSeconds(fxDelay);
		
		if (fxFillRect.color == decreaseColor)
			while (fxSlider.value != hpSlider.value)
			{
				yield return null;

				fxSlider.value = Mathf.SmoothDamp(fxSlider.value, hpSlider.value, ref smoothVel, fxTime);
			}

		else if (fxFillRect.color == increaseColor)
			while (fxSlider.value != hpSlider.value)
			{
				yield return null;

				hpSlider.value = Mathf.SmoothDamp(hpSlider.value, fxSlider.value, ref smoothVel, fxTime);
			}
	}
}
