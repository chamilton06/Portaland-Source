using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Prototype respawn resets the entire test scene, including opponents and loot.
[RequireComponent(typeof(PlayerStats), typeof(DamageFlash))]
public class PlayerDeathHandler : MonoBehaviour
{
    [Min(0.1f)] public float respawnDelay = 3f;
    private PlayerStats stats;
    private bool dead;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        stats.onDeath.AddListener(HandleDeath);
    }

    private void HandleDeath()
    {
        if (dead) return;
        dead = true;

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.CancelAttack();
            combat.enabled = false;
        }
        PlayerDefence defence = GetComponent<PlayerDefence>();
        if (defence != null) defence.enabled = false;
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;
        PlayerTurn turn = GetComponent<PlayerTurn>();
        if (turn != null) turn.enabled = false;

        // Inventory may live on the Canvas rather than Player.
        foreach (Inventory inventory in FindObjectsOfType<Inventory>(true))
            if (inventory.playerCombat == combat) inventory.enabled = false;

        CameraFollow cameraFollow = Camera.main != null
            ? Camera.main.GetComponent<CameraFollow>() : null;
        if (cameraFollow != null) cameraFollow.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSecondsRealtime(respawnDelay);
        Scene scene = gameObject.scene;
        if (!Application.CanStreamedLevelBeLoaded(scene.path))
        {
            Debug.LogError("Add " + scene.path + " to enabled Scenes In Build for respawn.", this);
            yield break;
        }
        SceneManager.LoadScene(scene.path);
    }

    private void OnGUI()
    {
        if (!dead) return;
        GUI.depth = -100;
        Color previous = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previous;
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 60,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(0f, Screen.height * 0.4f, Screen.width, 100f), "You died", style);
    }

    private void OnDestroy()
    {
        if (stats != null) stats.onDeath.RemoveListener(HandleDeath);
    }
}
