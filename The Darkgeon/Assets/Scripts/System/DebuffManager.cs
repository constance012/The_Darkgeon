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
	[SerializeField] private CharacterController2D controller;
	[SerializeField] private Animator playerAnim;

	[Header("UIs")]
	[Space]
	[SerializeField] private Transform debuffPanel;
	[SerializeField] private GameObject debuffUIPrefab;

	// Private fields.
	private List<Debuff> debuffList = new List<Debuff>();
	private Debuff currentDebuff;
	private ParticleSystem currentEffect;

	private delegate void DebuffDelegate();

	private DebuffDelegate handler = null;

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
		// If the player is killed by any debuff.
		if (player.currentHP <= 0 && !deathByDebuff)
		{
			playerAnim.SetTrigger("TakingDamage");
			deathByDebuff = true;
			return;
		}

		if (debuffList.Count == 0)
		{
			currentDebuff = null;
			currentEffect = null;
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
		if (debuffPanel.Find(target.name) == null)
		{
			debuffList.Add(target);

			// Get the corresponding private and non-static method using reflection.
			MethodInfo methodName = this.GetType().GetMethod(target.name, BindingFlags.NonPublic | BindingFlags.Instance);

			// Create a delegate holding that private and non-static method.
			DebuffDelegate debuffMethod = (DebuffDelegate)Delegate.CreateDelegate(typeof(DebuffDelegate), this, methodName);

			ManageHandler(debuffMethod, DebuffManageAction.Add);


			GameObject debuffUIObj = Instantiate(debuffUIPrefab, debuffPanel);

			debuffUIObj.name = target.name;
			debuffUIObj.GetComponent<Image>().sprite = target.icon;
			debuffUIObj.transform.Find("Duration").GetComponent<TextMeshProUGUI>().text = target.duration.ToString();

			TooltipTrigger tooltip = debuffUIObj.GetComponent<TooltipTrigger>();
			tooltip.header = target.name;
			tooltip.content = target.description;
		}

		// Otherwise, reset its duration if the current duration is less than the target's.
		else
		{
			Debuff existingDebuff = debuffList.Find(debuff => debuff.name.Equals(target.name));
			
			if (existingDebuff.duration < target.duration)
				existingDebuff.duration = target.duration;
		}
	}

	private void RemoveDebuff(Debuff target)
	{
		// Remove debuff from the list.
		debuffList.Remove(target);

		// Remove its method from the handler.
		MethodInfo methodName = this.GetType().GetMethod(target.name, BindingFlags.NonPublic | BindingFlags.Instance);
		DebuffDelegate debuffMethod = (DebuffDelegate)Delegate.CreateDelegate(typeof(DebuffDelegate), this, methodName);

		ManageHandler(debuffMethod, DebuffManageAction.Remove);

		// Hide the tooltip and destroy the UI Game Object.
		Transform targetUI = debuffPanel.Find(target.name);
		
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

	#region Debuff Types Method
	private void Bleeding()
	{
		
		currentDebuff = GetDebuff("bleeding");
		
		// Clone the particle system if the player doesn't contain its game object already and play it once.
		if (currentDebuff.visualEffect != null && player.transform.Find(currentDebuff.visualEffect.name + "(Clone)") == null)
		{
			currentEffect = Instantiate(currentDebuff.visualEffect, player.transform).GetComponent<ParticleSystem>();
			currentEffect.Play();
		}

		currentDebuff.duration -= Time.deltaTime;
		currentDebuff.hpLossDelay -= Time.deltaTime;

		if (currentDebuff.duration <= 0f || player.currentHP <= 0)
		{
			Destroy(currentEffect.gameObject);
			RemoveDebuff(currentDebuff);
			player.canRegen = true;
			return;
		}

		player.canRegen = currentDebuff.allowRegenerate;

		// If the player moves, she'll lose health.
		if (playerAnim.GetFloat("Speed") > .01f && player.currentHP > 0 && currentDebuff.hpLossDelay <= 0f)
		{
			player.currentHP -= currentDebuff.healthLossRate;
			player.currentHP = Mathf.Clamp(player.currentHP, 0, player.maxHP);

			player.hpBar.SetCurrentHealth(player.currentHP);

			DamageText.Generate(player.dmgTextPrefab, player.dmgTextLoc.position, currentDebuff.healthLossRate.ToString());

			currentDebuff.hpLossDelay = currentDebuff.baseHpLossDelay;
		}

		// Update the UI.
		debuffPanel.Find(currentDebuff.name + "/Duration").GetComponent<TextMeshProUGUI>().text = currentDebuff.duration.ToString("0");
	}

	private void Poisoned()
	{
		currentDebuff = GetDebuff("poisoned");

		currentDebuff.duration -= Time.deltaTime;
		currentDebuff.hpLossDelay -= Time.deltaTime;

		if (currentDebuff.duration <= 0f || player.currentHP <= 0)
		{
			RemoveDebuff(currentDebuff);
			player.canRegen = true;
			return;
		}

		player.canRegen = currentDebuff.allowRegenerate;

		// The player loses health over time.
		if (player.currentHP > 0 && currentDebuff.hpLossDelay <= 0f)
		{
			player.currentHP -= currentDebuff.healthLossRate;
			player.currentHP = Mathf.Clamp(player.currentHP, 0, player.maxHP);

			player.hpBar.SetCurrentHealth(player.currentHP);

			DamageText.Generate(player.dmgTextPrefab, player.dmgTextLoc.position, currentDebuff.healthLossRate.ToString());

			currentDebuff.hpLossDelay = currentDebuff.baseHpLossDelay;
		}
		// Update the UI.
		debuffPanel.Find(currentDebuff.name + "/Duration").GetComponent<TextMeshProUGUI>().text = currentDebuff.duration.ToString("0");
	}

	private void Slowness()
	{
		currentDebuff = GetDebuff("slowness");

		currentDebuff.duration -= Time.deltaTime;
		
		if (currentDebuff.duration <= 0f || player.currentHP <= 0)
		{
			RemoveDebuff(currentDebuff);
			controller.m_MoveSpeed *= 1 / (1 - currentDebuff.speedReduceFactor);  // Reverse the speed nerf.
			return;
		}

		// The player's movement speed is reduced.
		if (!currentDebuff.isSpeedReduced)
		{
			controller.m_MoveSpeed *= (1 - currentDebuff.speedReduceFactor);
			currentDebuff.isSpeedReduced = true;
		}

		// Update the UI.
		debuffPanel.Find(currentDebuff.name + "/Duration").GetComponent<TextMeshProUGUI>().text = currentDebuff.duration.ToString("0");
	}
	#endregion
}

internal enum DebuffManageAction
{
	Add,
	Remove
}
