using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebuffManager : MonoBehaviour
{
	[Header("References")]
	[Space]
	private List<Debuff> debuffList = new List<Debuff>();

	[SerializeField] private PlayerStats player;
	[SerializeField] private GameObject debuffPanel;
	[SerializeField] private GameObject debuffPrefab;

	private void Awake()
	{
		player = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
		debuffPanel = GameObject.Find("Debuff Panel");
	}

	private void Start()
	{
		
	}

	private void Update()
	{
		
	}

	public void ApplyDebuff(Debuff debuff)
	{
		if (debuffPanel.transform.Find(debuff.name) == null)
		{
			debuffList.Add(debuff);

			GameObject debuffUIObj = Instantiate(debuffPrefab, debuffPanel.transform);

			debuffUIObj.GetComponent<Image>().sprite = debuff.icon;
			debuffUIObj.transform.Find("Duration").GetComponent<TextMeshProUGUI>().text = debuff.duration.ToString();
			debuffUIObj.name = debuff.name;
		}
	}

	public void RemoveDebuff(string name)
	{
		Debuff target = debuffList.Find(debuff => debuff.name == name);
		debuffList.Remove(target);

		Destroy(debuffPanel.transform.Find(name));
	}

	private void Bleeding()
	{

	}
}
