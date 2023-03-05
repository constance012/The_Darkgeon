using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System;

/// <summary>
/// A manager for the debuff system.
/// </summary>
public class DebuffManager : MonoBehaviour
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

	private delegate void DebuffDelegate();

	private DebuffDelegate handler = null;

	public static bool deathByDebuff { get; set; }

	private void Awake()
	{
		player = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
		playerAnim = player.GetComponent<Animator>();
		debuffPanel = GameObject.Find("Debuff Panel").transform;
	}

	private void Update()
	{
		// If the player is killed by any debuff.
		if (player.currentHP <= 0 && !deathByDebuff)
		{
			playerAnim.SetTrigger("TakingDamage");
			deathByDebuff = true;
			return;
		}

		// Invoke the debuff handler if not null.
		handler?.Invoke();
	}

	#region Handling debuff.
	public Debuff GetDebuff(string nameLowered)
	{
		return debuffList.Find(debuff => debuff.name.ToLower().Equals(nameLowered));
	}

	public void ApplyDebuff(Debuff target)
	{
		// If the debuff hasn't been apllied yet.
		if (debuffPanel.Find(target.debuffName) == null)
		{
			debuffList.Add(target);

			ManageHandler(target.TakeEffect, DebuffManageAction.Add);

			GameObject debuffUIObj = Instantiate(debuffUIPrefab, debuffPanel);

			debuffUIObj.name = target.debuffName;
			debuffUIObj.GetComponent<Image>().sprite = target.icon;
			debuffUIObj.transform.Find("Duration").GetComponent<TextMeshProUGUI>().text = target.duration.ToString();

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

		ManageHandler(target.TakeEffect, DebuffManageAction.Remove);

		// Hide the tooltip and destroy the UI Game Object.
		Transform targetUI = debuffPanel.Find(target.debuffName);
		
		targetUI.GetComponent<TooltipTrigger>().OnMouseExit();
		Destroy(targetUI.gameObject);
	}

	public void ClearAllDebuff()
	{
		debuffList.Clear();
		handler = null;
		foreach (Transform debuff in debuffPanel)
			Destroy(debuff.gameObject);
	}

	private void ManageHandler(DebuffDelegate method, DebuffManageAction action)
	{
		if (action == DebuffManageAction.Add)
		{
			handler -= method;  // Remove first to ensure no duplication.
			handler += method;
		}

		else
			handler -= method;
	}
	#endregion
}

internal enum DebuffManageAction
{
	Add,
	Remove
}
