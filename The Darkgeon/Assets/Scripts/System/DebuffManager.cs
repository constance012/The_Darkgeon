using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebuffManager : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private PlayerStats player;
	[SerializeField] private CharacterController2D controller;
	[SerializeField] private Animator playerAnim;
	[SerializeField] private Transform debuffPanel;
	[SerializeField] private GameObject debuffPrefab;

	private Debuff currentDebuff;
	private List<Debuff> debuffList = new List<Debuff>();

	public delegate void DebuffHandler();
	DebuffHandler handler = null;

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
	public void ApplyDebuff(Debuff debuff)
	{
		// If the debuff hasn't been apllied yet.
		if (debuffPanel.Find(debuff.name) == null)
		{
			debuffList.Add(debuff);
			ManageHandler(debuff.name);

			GameObject debuffUIObj = Instantiate(debuffPrefab, debuffPanel);

			debuffUIObj.name = debuff.name;
			debuffUIObj.GetComponent<Image>().sprite = debuff.icon;
			debuffUIObj.transform.Find("Duration").GetComponent<TextMeshProUGUI>().text = debuff.duration.ToString();
		}

		// Otherwise, just reset its duration.
		else
			debuffList.Find(target => target.name == debuff.name).duration = debuff.duration;
	}

	public void RemoveDebuff(Debuff target)
	{
		debuffList.Remove(target);
		ManageHandler(target.name, DelegateAction.Remove);

		Destroy(debuffPanel.Find(target.name).gameObject);
	}

	public void ClearAllDebuff()
	{
		debuffList.Clear();
		handler = null;
		foreach (Transform debuff in debuffPanel)
			Destroy(debuff.gameObject);
	}

	private void ManageHandler(string methodName, DelegateAction action = DelegateAction.Add)
	{
		if (action == DelegateAction.Add)
			switch (methodName)
			{
				case "Bleeding":
					handler -= Bleeding;  // Remove first to ensure no duplication.
					handler += Bleeding;
					break;
				case "Slowness":
					handler -= Slowness;
					handler += Slowness;
					break;
				default:
					Debug.LogWarning("Method: " + methodName + " not found!!");
					return;
			}

		else
			switch (methodName)
			{
				case "Bleeding":
					handler -= Bleeding;
					break;
				case "Slowness":
					handler -= Slowness;
					break;
				default:
					Debug.LogWarning("Method: " + methodName + " not found!!");
					return;
			}
	}
	#endregion

	#region Debuff Types
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
		debuffPanel.Find(currentDebuff.name).Find("Duration").GetComponent<TextMeshProUGUI>().text = Mathf.Round(currentDebuff.duration).ToString();

		// If the player moves, she'll lose health.
		if (playerAnim.GetFloat("Speed") > 0.01f && player.currentHP > 0 && currentDebuff.hpLossDelay <= 0f)
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

		debuffPanel.Find(currentDebuff.name).Find("Duration").GetComponent<TextMeshProUGUI>().text = Mathf.Round(currentDebuff.duration).ToString();
	}
	#endregion
}

internal enum DelegateAction
{
	Add,
	Remove
}
