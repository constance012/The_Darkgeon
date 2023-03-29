using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using CSTGames.CommonEnums;

/// <summary>
/// A core manager for the current game's session.
/// </summary>
public class GameManager : MonoBehaviour
{
	public static GameManager instance { get; private set; }

	[Header("Player References")]
	[Space]

	[SerializeField] private Animator playerAnim;
	[SerializeField] private Rigidbody2D rb2d;
	[SerializeField] private PlayerStats playerStats;

	[SerializeField] private PlayerMovement moveScript;
	[SerializeField] private PlayerActions actionsScript;

	[Header("UI Elements")]
	[Space]
	[SerializeField] private TextMeshProUGUI deathMessageText;
	[SerializeField] private TextMeshProUGUI killSourceText;
	[SerializeField] private TextMeshProUGUI countdownText;

	[SerializeField] private GameObject deathPanel;
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject playerUI;
	[SerializeField] private GameObject inventory;

	public static bool isPlayerDeath { get; private set; } = false;
	public static bool isPause { get; private set; }

	private string[] deathMessages = new string[] { "YOUR SOUL HAS BEEN CONSUMED", "YOUR HEAD WAS DETACHED", 
													"YOUR FACE WAS RIPPED OFF", "YOUR BODY WAS EVISCERATED", 
													"THEY SPLIT YOU IN TWO", "YOUR FATE WAS SHATTERED" };

	[Header("Fields")]
	[Space]
	public float m_RespawnTimer;
	private float respawnTimer;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Debug.LogWarning("More than one instance of Game Manager found!!");
			instance = null;
			this.enabled = false;
			return;
		}

		playerAnim = GameObject.FindWithTag("Player").GetComponent<Animator>();
		playerStats = playerAnim.GetComponent<PlayerStats>();
		rb2d = playerAnim.GetComponent<Rigidbody2D>();

		moveScript = playerAnim.GetComponent<PlayerMovement>();
		actionsScript = playerAnim.GetComponent<PlayerActions>();

		deathPanel = GameObject.FindWithTag("UI Canvas").transform.Find("Death Message").gameObject;
		deathMessageText = deathPanel.transform.Find("Message").GetComponent<TextMeshProUGUI>();
		killSourceText = deathPanel.transform.Find("Kill Source").GetComponent<TextMeshProUGUI>();
		countdownText = deathPanel.transform.Find("Countdown").GetComponent<TextMeshProUGUI>();

		pauseMenu = GameObject.FindWithTag("UI Canvas").transform.Find("Pause Menu").gameObject;
		playerUI = GameObject.FindWithTag("UI Canvas").transform.Find("Player UI").gameObject;
		inventory = GameObject.FindWithTag("Inventory Canvas");
	}

	private void Start()
	{
		deathPanel.SetActive(false);
		pauseMenu.SetActive(false);
		inventory.SetActive(false);

		playerUI.SetActive(true);
		respawnTimer = m_RespawnTimer;
	}

	private void Update()
	{
		if (!isPlayerDeath)
		{
			if (InputManager.instance.GetKeyDown(KeybindingActions.Pause))
				Invoke(!isPause ? nameof(Pause) : nameof(Unpause), 0f);

			if (InputManager.instance.GetKeyDown(KeybindingActions.Inventory))
				inventory.SetActive(!inventory.activeInHierarchy);
		}

		if (Input.GetKeyDown(KeyCode.R))
			LevelsManager.instance.RestartLevel();
		
		if (playerStats.currentHP <= 0 && !isPlayerDeath)
		{
			SetDeathMessage(playerStats.killSource);
			Die();
			isPlayerDeath = true;
		}

		// If the player is death and grounded, then disable the physics simulation.
		if (isPlayerDeath)
		{
			respawnTimer -= Time.deltaTime;

			if (playerAnim.GetBool("Grounded") && !rb2d.simulated)
				rb2d.simulated = false;

			if (!Physics2D.GetIgnoreLayerCollision(3, 8))
				Physics2D.IgnoreLayerCollision(3, 8, true);

			countdownText.text = respawnTimer.ToString("0");

			if (respawnTimer <= 0f)
			{
				Respawn();
				respawnTimer = m_RespawnTimer;
			}
		}
	}

	#region Player States
	public void Respawn()
	{
		deathPanel.SetActive(false);
		playerStats.currentHP = playerStats.maxHP.Value;
		playerStats.hpBar.SetMaxHealth(playerStats.maxHP.Value);

		moveScript.enabled = actionsScript.enabled = true;
		rb2d.simulated = true;

		Physics2D.IgnoreLayerCollision(3, 8, false);

		playerAnim.SetTrigger("Respawn");
		playerAnim.SetBool("IsDeath", false);

		playerStats.transform.position = playerStats.respawnPos;
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
	public void ReturnToMenu()
	{
		AsyncOperation op = SceneManager.LoadSceneAsync("Scenes/Menu");
		op.allowSceneActivation = false;

		Time.timeScale = 1f;
		isPause = false;

		op.allowSceneActivation = true;
	}

	private void SetDeathMessage(KillSources sources)
	{
		int randomIndex = Random.Range(0, deathMessages.Length);
		deathMessageText.text = deathMessages[randomIndex];

		killSourceText.text = "KILL BY: " + StringUtility.AddWhitespaceBeforeCapital(sources.ToString()).ToUpper();
	}

	private void Pause()
	{
		pauseMenu.SetActive(true);
		playerUI.SetActive(false);
		Time.timeScale = 0f;
		isPause = true;
	}

	public void Unpause()
	{
		pauseMenu.SetActive(false);
		playerUI.SetActive(true);
		Time.timeScale = 1f;
		isPause = false;
	}
	#endregion
}
