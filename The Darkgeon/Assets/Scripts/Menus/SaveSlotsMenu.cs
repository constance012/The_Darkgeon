using UnityEngine;
using CSTGames.DataPersistence;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class SaveSlotsMenu : MonoBehaviour
{
	[Header("Menu Navigation")]
	[Space]
	[SerializeField] private MainMenu mainMenu;

	[Space]
	[SerializeField] private SaveSlot[] saveSlots;

	private void Awake()
	{
		mainMenu = transform.root.GetComponentInChildren<MainMenu>(true);
		saveSlots = GetComponentsInChildren<SaveSlot>();
	}

	public void OnSaveSlotClicked(SaveSlot slot)
	{
		GameDataManager.instance.SelectedSaveSlotID = string.Copy(slot.SaveSlotID);
		
		// TODO - Continue from the existing save if the slot contains data.
		if (slot.hasData)
		{
			GameDataManager.instance.LoadGame(false);
		}

		// TODO - Start a new game if the slot is empty.
		else
		{
			GameDataManager.instance.NewGame();
			GameDataManager.instance.SaveGame();
		}

		mainMenu.gameObject.SetActive(true);
		mainMenu.loadGameSceneEvent?.Invoke();

		this.gameObject.SetActive(false);
	}

	/// <summary>
	/// Callback method for the back button.
	/// </summary>
	public void BackToMainMenu()
	{
		mainMenu.gameObject.SetActive(true);
		this.gameObject.SetActive(false);
	}

	/// <summary>
	/// Activate this save slots menu and fetch all data for all slots.
	/// </summary>
	public void ActivateMenu()
	{
		this.gameObject.SetActive(true);

		// TODO - Load data of all save slots that exist.
		Dictionary<string, GameData> saveSlotsData = GameDataManager.instance.LoadAllSaveSlotsData();

		foreach (SaveSlot slot in saveSlots)
		{
			GameData data = new GameData(true);
			saveSlotsData.TryGetValue(slot.SaveSlotID, out data);
			
			slot.SetData(data);
		}
	}
}
