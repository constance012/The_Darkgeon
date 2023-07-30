using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A manager for the debuff system.
/// </summary>
public class DebuffManager : Singleton<DebuffManager>
{
	[Header("References")]
	[Space]

	[Header("Player")]
	[Space]
	[SerializeField] private PlayerStats player;
	[SerializeField] private Animator playerAnim;

	[Header("UIs")]
	[Space]
	[SerializeField] private Transform debuffPanel;
	[SerializeField] private GameObject debuffUIPrefab;

	// Private fields.
	private List<Debuff> debuffList = new List<Debuff>();

	private Action debuffHandler = null;

	protected override void Awake()
	{
		base.Awake();

		player = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
		playerAnim = player.GetComponentInChildren<Animator>("Graphic");
		debuffPanel = GameObject.Find("Debuff Panel").transform;
	}

	private void Update()
	{
		// Invoke the debuff handler if not null.
		debuffHandler?.Invoke();
	}

	#region Handling debuff.
	public Debuff GetDebuff(string nameLowered)  => debuffList.Find(debuff => debuff.name.ToLower().Equals(nameLowered));

	public void ApplyDebuff(Debuff target)
	{
		// If the debuff hasn't been apllied yet.
		if (debuffPanel.Find(target.debuffName) == null)
		{
			debuffList.Add(target);

			ManageHandler(target.TakeEffect);

			GameObject debuffUIObj = Instantiate(debuffUIPrefab, debuffPanel);

			debuffUIObj.name = target.debuffName;
			debuffUIObj.GetComponent<Image>().sprite = target.icon;
			debuffUIObj.transform.GetComponentInChildren<TextMeshProUGUI>("Duration").text = target.duration.ToString();

			TooltipTrigger tooltip = debuffUIObj.GetComponent<TooltipTrigger>();
			tooltip.header = target.debuffName;
			tooltip.content = target.description;
		}

		// Otherwise, reset its duration if the current duration is less than the target's.
		else
		{
			Debuff existingDebuff = GetDebuff(target.name.ToLower());
			
			if (existingDebuff.duration < target.duration)
				existingDebuff.duration = target.duration;
		}
	}

	public void RemoveDebuff(Debuff target)
	{
		// Remove debuff from the list.
		debuffList.Remove(target);

		ManageHandler(target.TakeEffect, false);

		// Hide the tooltip and destroy the UI Game Object.
		Transform targetUI = debuffPanel.Find(target.debuffName);
		
		targetUI.GetComponent<TooltipTrigger>().OnMouseExit();
		Destroy(targetUI.gameObject);
	}

	public void ClearAllDebuff()
	{
		debuffList.Clear();
		debuffHandler = null;
		foreach (Transform debuff in debuffPanel)
			Destroy(debuff.gameObject);
	}

	private void ManageHandler(Action method, bool subscribe = true)
	{
		if (subscribe)
		{
			debuffHandler -= method;  // Remove first to ensure no duplication.
			debuffHandler += method;
			return;
		}
		
		debuffHandler -= method;
	}
	#endregion
}
