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
	[SerializeField] private PlayerStats player;
	[SerializeField] private CharacterController2D controller;
	[SerializeField] private Animator playerAnim;
	[SerializeField] private Transform debuffPanel;
	[SerializeField] private GameObject debuffUIPrefab;

	private Debuff currentDebuff;
	private List<Debuff> debuffList = new List<Debuff>();

	private delegate void DebuffHandler();

	DebuffHandler handler = null;
	//DebuffHandler debuffMethod;

	public static bool deathByDebuff { get; set; }

	private void Awake()
	{
		player = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
		controller = player.GetComponent<CharacterController2D>();
		playerAnim = player.GetComponent<Animator>();
		debuffPanel = GameObject.Find("Debuff Panel").transform;
	}

	private void Update()
	{
		// If the player bleeds to death.
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
	public void ApplyDebuff(Debuff target)
	{
		// If the debuff hasn't been apllied yet.
		if (debuffPanel.Find(target.name) == null)
		{
			debuffList.Add(target);

			// Get the corresponding private and non-static method using reflection.
			MethodInfo methodName = this.GetType().GetMethod(target.name, BindingFlags.NonPublic | BindingFlags.Instance);
			
			// Create a delegate holding that private and non-static method.
			DebuffHandler debuffMethod = (DebuffHandler) Delegate.CreateDelegate(typeof(DebuffHandler), this, methodName);

			ManageHandler(debuffMethod, DelegateAction.Add);

			GameObject debuffUIObj = Instantiate(debuffUIPrefab, debuffPanel);

			debuffUIObj.name = target.name;
			debuffUIObj.GetComponent<Image>().sprite = target.icon;
			debuffUIObj.transform.Find("Duration").GetComponent<TextMeshProUGUI>().text = target.duration.ToString();
			
			TooltipTrigger tooltip = debuffUIObj.GetComponent<TooltipTrigger>();
			tooltip.header = target.name;
			tooltip.content = target.description;
		}

		// Otherwise, just reset its duration.
		else
			debuffList.Find(debuff => debuff.name == target.name).duration = target.duration;
	}

	public void RemoveDebuff(Debuff target)
	{
		debuffList.Remove(target);

		MethodInfo methodName = this.GetType().GetMethod(target.name, BindingFlags.NonPublic | BindingFlags.Instance);
		DebuffHandler debuffMethod = (DebuffHandler)Delegate.CreateDelegate(typeof(DebuffHandler), this, methodName);

		ManageHandler(debuffMethod, DelegateAction.Remove);

		Destroy(debuffPanel.Find(target.name).gameObject);
	}

	public void ClearAllDebuff()
	{
		debuffList.Clear();
		handler = null;
		foreach (Transform debuff in debuffPanel)
			Destroy(debuff.gameObject);
	}

	private void ManageHandler(DebuffHandler method, DelegateAction action)
	{
		if (action == DelegateAction.Add)
		{
			handler -= method;  // Remove first to ensure no duplication.
			handler += method;
		}

		else
			handler -= method;

		//debuffMethod = null;
	}
	#endregion

	#region Debuff Types Method
	private void Bleeding()
	{
		currentDebuff = debuffList.Find(debuff => debuff.name == "Bleeding");
		
		currentDebuff.duration -= Time.deltaTime;
		currentDebuff.hpLossDelay -= Time.deltaTime;

		if (currentDebuff.duration <= 0f || player.currentHP <= 0)
		{
			RemoveDebuff(currentDebuff);
			player.canRegen = true;
			return;
		}

		player.canRegen = currentDebuff.allowRegenerate;

		// Update the UI.
		debuffPanel.Find(currentDebuff.name + "/Duration").GetComponent<TextMeshProUGUI>().text = Mathf.Round(currentDebuff.duration).ToString();

		// If the player moves, she'll lose health.
		if (playerAnim.GetFloat("Speed") > .01f && player.currentHP > 0 && currentDebuff.hpLossDelay <= 0f)
		{
			player.currentHP -= currentDebuff.healthLossRate;
			player.currentHP = Mathf.Clamp(player.currentHP, 0, player.maxHP);
			player.hpBar.SetCurrentHealth(player.currentHP);
			DamageText.Generate(player.dmgTextPrefab, player.dmgTextLoc.position, currentDebuff.healthLossRate.ToString());

			currentDebuff.hpLossDelay = currentDebuff.baseHpLossDelay;
		}
	}

	private void Slowness()
	{
		currentDebuff = debuffList.Find(debuff => debuff.name == "Slowness");
		
		currentDebuff.duration -= Time.deltaTime;
		
		if (currentDebuff.duration <= 0f || player.currentHP <= 0)
		{
			RemoveDebuff(currentDebuff);
			controller.m_MoveSpeed *= 1 / (1 - currentDebuff.speedReduceFactor);  // Reverse the speed nerf.
			return;
		}

		if (!currentDebuff.isSpeedReduced)
		{
			controller.m_MoveSpeed *= (1 - currentDebuff.speedReduceFactor);
			currentDebuff.isSpeedReduced = true;
		}

		debuffPanel.Find(currentDebuff.name + "/Duration").GetComponent<TextMeshProUGUI>().text = Mathf.Round(currentDebuff.duration).ToString();
	}
	#endregion
}

internal enum DelegateAction
{
	Add,
	Remove
}
