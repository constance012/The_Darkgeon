using UnityEngine;
using UnityEngine.EventSystems;
using UnityRandom = UnityEngine.Random;
using CSTGames.DataPersistence;
using TMPro;

public class SaveSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[field: SerializeField]
	public string SaveSlotID { get; private set; }

	[Header("Content")]
	[Space]
	[SerializeField] private GameObject noDataContent;
	[SerializeField] private GameObject hasDataContent;
	[SerializeField] private TextMeshProUGUI emptyText;
	[SerializeField] private TextMeshProUGUI levelText;
	[SerializeField] private TextMeshProUGUI playedTimeText;
	
	public bool HasData { get; private set; }
	public static DeleteSaveSlotButton DeleteButton { get; set; }

	private const string charsList = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

	[ContextMenu("Generate Slot ID")]
	private void GenerateSlotID()
	{
		// abcdefgh-abcd-ab0def - the '0' is the slot index in the hierarchy.
		char[] stringOfChars = new char[20];

		for (int i = 0; i < stringOfChars.Length; i++)
		{
			if (i == 8 || i == 13)
			{
				stringOfChars[i] = '-';
				continue;
			}
			
			if (i == 16)
			{
				stringOfChars[i] = (char)('0' | transform.GetSiblingIndex());
				continue;
			}

			int randomIndex = UnityRandom.Range(0, charsList.Length);
			stringOfChars[i] = charsList[randomIndex];
		}

		SaveSlotID = new string(stringOfChars);
	}

	private void Awake()
	{
		noDataContent = transform.Find("No Data Content").gameObject;
		hasDataContent = transform.Find("Has Data Content").gameObject;
		emptyText = noDataContent.transform.GetComponentInChildren<TextMeshProUGUI>("Empty Text");
		levelText = hasDataContent.transform.GetComponentInChildren<TextMeshProUGUI>("Level Text");
		playedTimeText = hasDataContent.transform.GetComponentInChildren<TextMeshProUGUI>("Played Time Text");

		DeleteButton = transform.parent.GetComponentInChildren<DeleteSaveSlotButton>("Delete Button");
	}

	private void Start()
	{
		emptyText.text = $"EMPTY SLOT {transform.GetSiblingIndex() + 1}";
	}

	public void SetData(GameData gameData)
	{
		if (gameData == null || !gameData.allDataLoadedSuccessfully)
		{
			HasData = false;
			noDataContent.SetActive(true);
			hasDataContent.SetActive(false);
		}
		else
		{
			HasData = true;
			noDataContent.SetActive(false);
			hasDataContent.SetActive(true);

			levelText.text = gameData.levelData.levelName.ToUpper();

			Vector3Int totalPlayedTime = gameData.playerData.TotalPlayedTime;

			playedTimeText.text = $"PLAYED TIME: {totalPlayedTime.x}:{totalPlayedTime.y}:{totalPlayedTime.z}";
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		emptyText.text = "START A NEW GAME?";

		if (HasData)
		{
			DeleteButton.currentSlotIndex = this.transform.GetSiblingIndex();
			DeleteButton.currentSlotID = this.SaveSlotID;
			DeleteButton.SetYPosition(this.transform.position.y);
			DeleteButton.interactable = true;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		emptyText.text = $"EMPTY SLOT {transform.GetSiblingIndex() + 1}";
		Vector2 localMousePos = DeleteButton.transform.InverseTransformPoint(Input.mousePosition);

		if (!DeleteButton.rect.Contains(localMousePos))
		{
			DeleteButton.OnPointerExit(eventData);
		}
	}
}