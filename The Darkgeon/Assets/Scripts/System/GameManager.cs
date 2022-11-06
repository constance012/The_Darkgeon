using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
	[Header("Player References")]
	[Space]

	[SerializeField] private Animator playerAnim;
	[SerializeField] private PlayerStats playerStats;

	[SerializeField] private PlayerMovement moveScript;
	[SerializeField] private PlayerActions actionsScript;

	[Header("UI Elements")]
	[Space]
	[SerializeField] private TextMeshProUGUI deathMessageText;
	[SerializeField] private TextMeshProUGUI killSourceText;

	[SerializeField] private GameObject deathPanel;
	[SerializeField] private GameObject pauseMenu;

	[Header("Fields")]
	[Space]
	public static bool isPlayerDeath = false;

	private string[] deathMessages = new string[] { "YOUR SOUL HAS BEEN CONSUMED", "YOUR HEAD WAS DETACHED", 
													"YOUR FACE WAS RIPPED OFF", "YOUR BODY WAS EVISCERATED", 
													"THEY SPLIT YOU IN TWO", "YOUR FATE WAS SHATTERED" };

	public static bool isPause;

	private void Awake()
	{
		playerAnim = GameObject.FindWithTag("Player").GetComponent<Animator>();
		playerStats = playerAnim.GetComponent<PlayerStats>();

		moveScript = playerAnim.GetComponent<PlayerMovement>();
		actionsScript = playerAnim.GetComponent<PlayerActions>();

		deathPanel = GameObject.Find("Death Message");
		deathMessageText = deathPanel.transform.Find("Message").GetComponent<TextMeshProUGUI>();
		killSourceText = deathPanel.transform.Find("Kill Source").GetComponent<TextMeshProUGUI>();

		pauseMenu = GameObject.Find("Pause Menu");
	}

	private void Start()
	{
		deathPanel.SetActive(false);
		pauseMenu.SetActive(false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && !isPlayerDeath)
			Invoke(!isPause ? nameof(Pause) : nameof(Unpause), 0f);
		
		if (playerStats.currentHP <= 0 && !isPlayerDeath)
		{
			SetDeathMessage(playerStats.killSource);
			Die();
			isPlayerDeath = true;
		}

		// If the player is death and grounded, then disable the physics simulation.
		if (isPlayerDeath && playerAnim.GetBool("Grounded"))
			playerStats.GetComponent<Rigidbody2D>().simulated = false;
	}

	#region Player States
	public void Respawn()
	{
		deathPanel.SetActive(false);
		playerStats.currentHP = playerStats.maxHP;
		playerStats.hpBar.SetMaxHealth(playerStats.maxHP);

		moveScript.enabled = actionsScript.enabled = true;
		playerStats.GetComponent<Rigidbody2D>().simulated = true;

		playerAnim.SetTrigger("Respawn");
		playerAnim.SetBool("IsDeath", false);

		transform.position = playerStats.respawnPos;
		isPlayerDeath = false;
		DebuffManager.deathByDebuff = false;
	}

	private void Die()
	{
		playerAnim.SetBool("IsDeath", true);

		deathPanel.SetActive(true);
		
		moveScript.enabled = actionsScript.enabled = false;
	}
	#endregion

	#region UI Controls
	private void SetDeathMessage(KillSources sources)
	{
		int randomIndex = Random.Range(0, deathMessages.Length);
		deathMessageText.text = deathMessages[randomIndex];

		switch (sources)
		{
			case KillSources.Environment:
				killSourceText.text = "Kill By: Environment";
				break;
			case KillSources.Bat:
				killSourceText.text = "Kill By: Bat";
				break;
			case KillSources.Crab:
				killSourceText.text = "Kill By: Crab";
				break;
			case KillSources.Golem:
				killSourceText.text = "Kill By: Golem";
				break;
			case KillSources.ReinforcedGolem:
				killSourceText.text = "Kill By: Reinforced Golem";
				break;
			case KillSources.Rat:
				killSourceText.text = "Kill By: Rat";
				break;
			case KillSources.Skull:
				killSourceText.text = "Kill By: Floating Skull";
				break;
			case KillSources.SpikedSlime:
				killSourceText.text = "Kill By: Spiked Slime";
				break;
			default:
				killSourceText.text = "Kill By: Unknown";
				break;
		}
	}

	private void Pause()
	{
		pauseMenu.SetActive(true);
		Time.timeScale = 0f;
		isPause = true;
	}

	public void Unpause()
	{
		pauseMenu.SetActive(false);
		Time.timeScale = 1f;
		isPause = false;
	}
	#endregion
}
