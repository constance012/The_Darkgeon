using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TooltipTrigger))]
public class CoinSlot : MonoBehaviour
{
	private TooltipTrigger tooltipTrigger;
	private TextMeshProUGUI quantityText;

	private bool isAwoken;

	private void Awake()
	{
		tooltipTrigger = GetComponent<TooltipTrigger>();
		quantityText = transform.GetComponentInChildren<TextMeshProUGUI>("Quantity");

		isAwoken = true;
	}

	private void Start()
	{
		quantityText.text = "0";
	}

	public void AddCoin(int quantity)
	{
		if (!isAwoken)
			return;

		tooltipTrigger.content = "Quantity: " + quantity + "\n\nA currency used for trading with merchants and other purposes.";
		quantityText.text = quantity.ToString();

		if (quantity > 999999)
			quantityText.text = Math.Round((float)quantity / 1000000f, 2) + "M";
	}
}
