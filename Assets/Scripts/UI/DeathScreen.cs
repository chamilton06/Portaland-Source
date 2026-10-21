using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    public PlayerStats player;
    public CanvasGroup deathPanel;              // panel with the you died text
    public float fadeDuration = 1.5f;
    public Button restartButton;                // optional

    public PlayerTurn playerTurn;
    public PlayerCombat playerCombat;
    public Inventory inventory;
    public JournalUI journal;
    public MapUI map;

    public KeyCode debugKillKey = KeyCode.K;    // insta kill for testing

    void Awake()
    {
        deathPanel.alpha = 0f;
        deathPanel.gameObject.SetActive(false);

        player.onDeath.AddListener(OnPlayerDied);

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(Restart);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(debugKillKey) && !player.IsDead())
        {
            player.TakeDamage(player.maxHealth);
        }
    }

    void OnPlayerDied()
    {
        if (inventory != null && inventory.IsOpen())
        {
            inventory.SetOpen(false);
        }
        if (journal != null && journal.IsOpen())
        {
            journal.Close();
        }
        if (map != null)
        {
            map.SetOpen(false);
        }

        Behaviour[] toDisable = { playerTurn, playerCombat, inventory, journal, map };
        foreach (Behaviour b in toDisable)
        {
            if (b != null)
            {
                b.enabled = false;
            }
        }

        MenuState.SetOpen(this, true);   // stops looking and attacking
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        deathPanel.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            deathPanel.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        deathPanel.alpha = 1f;
    }

    void Restart()
    {
        Time.timeScale = 1f;
        MenuState.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
