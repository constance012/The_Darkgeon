using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebuffManager : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private PlayerStats player;
	[SerializeField] private Animator playerAnim;
	[SerializeField] private Transform debuffPanel;
	[SerializeField] private GameObject debuffPrefab;

	private Debuff currentDebuff;
	private List<Debuff> debuffList = new List<Debuff>();

	[Header("Fields")]
	[Space]
	[HideInInspector] public float healthLossDelayOrigin;
	private int navIndex = 0;

	private void Awake()
	{
		player = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
		playerAnim = player.GetComponent<Animator>();
		debuffPanel = GameObject.Find("Debuff Panel").transform;
	}

	private void Start()
	{
		
	}

	private void Update()
	{
		if (debuffList.Count == 0)
			return;

		navIndex++;

		if (navIndex >= debuffList.Count)
			navIndex = 0;

		currentDebuff = debuffList[navIndex];

		Invoke(debuffList[navIndex].name, 0f);
	}

	public void ApplyDebuff(Debuff debuff)
	{
		// If the debuff hasn't been apllied yet.
		if (debuffPanel.Find(debuff.name) == null)
		{
			debuffList.Add(debuff);
			healthLossDelayOrigin = debuff.healthLossDelay;

			GameObject debuffUIObj = Instantiate(debuffPrefab, debuffPanel);

			debuffUIObj.name = debuff.name;
			debuffUIObj.GetComponent<Image>().sprite = debuff.icon;
			debuffUIObj.transform.Find("Duration").GetComponent<TextMeshProUGUI>().text = debuff.duration.ToString();
		}

		// Otherwise, just reset its duration.
		else
			currentDebuff.duration = debuff.duration;
	}

	public void RemoveDebuff(Debuff target)
	{
		debuffList.Remove(target);

		Destroy(debuffPanel.Find(target.name).gameObject);
	}

	public void ClearAllDebuff()
	{
		debuffList.Clear();
		foreach (Transform debuff in debuffPanel)
			Destroy(debuff.gameObject);
	}

	private void Bleeding()
	{
		currentDebuff.duration -= Time.deltaTime;
		currentDebuff.healthLossDelay -= Time.deltaTime;

		if (currentDebuff.duration <= 0f)
		{
			RemoveDebuff(currentDebuff);
			player.canRegen = true;
			return;
		}

		player.canRegen = currentDebuff.canRegenerate;
		debuffPanel.Find(currentDebuff.name).Find("Duration").GetComponent<TextMeshProUGUI>().text = Mathf.Round(currentDebuff.duration).ToString();

		// If the player moves, she'll lose health.
		if (playerAnim.GetFloat("Speed") > 0.01f && player.currentHP > 0 && currentDebuff.healthLossDelay < 0f)
		{
			Debug.Log(healthLossDelayOrigin);
			player.currentHP -= currentDebuff.healthLossRate;
			player.currentHP = Mathf.Clamp(player.currentHP, 0, player.maxHP);
			player.hpBar.SetCurrentHealth(player.currentHP);
			DamageText.Generate(player.dmgTextPrefab, player.dmgTextLoc.position, currentDebuff.healthLossRate.ToString());

			currentDebuff.healthLossDelay = healthLossDelayOrigin;
		}
	}
}
