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
	[SerializeField] private CharacterController2D controller;
	[SerializeField] private Animator playerAnim;
	[SerializeField] private Transform debuffPanel;
	[SerializeField] private GameObject debuffPrefab;

	private Debuff currentDebuff;
	private List<Debuff> debuffList = new List<Debuff>();

	[Header("Fields")]
	[Space]
	private int navIndex = 0;

	private void Awake()
	{
		player = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
		controller = player.GetComponent<CharacterController2D>();
		playerAnim = player.GetComponent<Animator>();
		debuffPanel = GameObject.Find("Debuff Panel").transform;
	}

	private void Start()
	{
		StartCoroutine(HandleDebuffs());
	}

	private void Update()
	{
		if (currentDebuff != null)
			currentDebuff.duration -= Time.deltaTime;
	}

	//private void Update()
	//{
	//	if (debuffList.Count == 0)
	//		return;

	//	navIndex++;

	//	if (navIndex >= debuffList.Count)
	//		navIndex = 0;

	//	currentDebuff = debuffList[navIndex];

	//	// If the current debuff is active, then just decrease its duration overtime.
	//	if (currentDebuff.isActive)
	//	{
	//		currentDebuff.duration -= Time.deltaTime;
	//		return;
	//	}

	//	// Only start the coroutine once.
	//	else if (currentDebuff.duration == currentDebuff.baseDuration)
	//	{
	//		StartCoroutine(currentDebuff.name);
	//		currentDebuff.isActive = true;
	//	}
	//}

	public void ApplyDebuff(Debuff debuff)
	{
		// If the debuff hasn't been apllied yet.
		if (debuffPanel.Find(debuff.name) == null)
		{
			debuffList.Add(debuff);

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
		StopAllCoroutines();
		debuffList.Clear();
		foreach (Transform debuff in debuffPanel)
			Destroy(debuff.gameObject);
	}

	private void Bleeding()
	{
		if (currentDebuff.duration <= 0f)
		{
			currentDebuff.isActive = false;
			RemoveDebuff(currentDebuff);
			player.canRegen = true;
			return;
		}

		player.canRegen = currentDebuff.canRegenerate;

		// Update the UI.
		debuffPanel.Find(currentDebuff.name).Find("Duration").GetComponent<TextMeshProUGUI>().text = Mathf.Round(currentDebuff.duration).ToString();

		// If the player moves, she'll lose health.
		if (playerAnim.GetFloat("Speed") > 0.01f && player.currentHP > 0)
		{
			player.currentHP -= currentDebuff.healthLossRate;
			player.currentHP = Mathf.Clamp(player.currentHP, 0, player.maxHP);
			player.hpBar.SetCurrentHealth(player.currentHP);
			DamageText.Generate(player.dmgTextPrefab, player.dmgTextLoc.position, currentDebuff.healthLossRate.ToString());
		}

		currentDebuff.isActive = false;
	}

	private void Slowness()
	{
		if (!currentDebuff.isSpeedReduced)
		{
			controller.m_MoveSpeed *= currentDebuff.speedReduceFactor;
			currentDebuff.isSpeedReduced = true;
		}

		if (currentDebuff.duration <= 0f)
		{
			currentDebuff.isActive = false;
			RemoveDebuff(currentDebuff);
			controller.m_MoveSpeed *= 1 / currentDebuff.speedReduceFactor;
			return;
		}

		debuffPanel.Find(currentDebuff.name).Find("Duration").GetComponent<TextMeshProUGUI>().text = Mathf.Round(currentDebuff.duration).ToString();

		currentDebuff.isActive = false;
	}

	private IEnumerator HandleDebuffs()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);

			if (debuffList.Count == 0)
				yield return new WaitUntil(() => debuffList.Count != 0);

			navIndex++;

			if (navIndex >= debuffList.Count)
				navIndex = 0;

			currentDebuff = debuffList[navIndex];

			// Only start the coroutine once.
			if (!currentDebuff.isActive)
			{
				currentDebuff.isActive = true;
				Invoke(currentDebuff.name, 0f);
			}

			yield return new WaitUntil(() => currentDebuff.isActive == false);
			Debug.Log(currentDebuff.name + " executed.");
		}
	}
}
