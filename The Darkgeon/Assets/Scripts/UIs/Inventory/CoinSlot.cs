using System;
using TMPro;
using UnityEngine;

public class CoinSlot : MonoBehaviour
{
	private TooltipTrigger tooltipTrigger;
	private TextMeshProUGUI quantityText;

	private void Awake()
	{
		tooltipTrigger = GetComponent<TooltipTrigger>();
		quantityText = transform.Find("Quantity").GetComponent<TextMeshProUGUI>();
	}

	private void Start()
	{
		quantityText.text = "0";
	}

	public void AddCoin(int quantity)
	{
		tooltipTrigger.content = "Quantity: " + quantity + "\n\nA currency used for trading with merchants and other purposes.";
		quantityText.text = quantity.ToString();

		if (quantity > 999999)
			quantityText.text = Math.Round((float)quantity / 1000000f, 2) + "M";
	}
}
