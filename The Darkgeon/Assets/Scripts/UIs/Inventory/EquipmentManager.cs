using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
	public static EquipmentManager instance { get; private set; }

	[SerializeField] private Equipment[] currentEquipments;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Equipment Manager found!!");
			instance = null;
			this.enabled = false;
			return;
		}
	}

	private void Start()
	{
		int numberOfSlots = System.Enum.GetNames(typeof(EquipmentType)).Length;
		currentEquipments = new Equipment[numberOfSlots];
	}

	public void Equip(Equipment target)
	{

	}
}
