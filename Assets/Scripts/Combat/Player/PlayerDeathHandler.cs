using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// When the player dies: a Souls-style YOU DIED screen, then the scene reloads, which resets
// everything including enemies and loot.
[RequireComponent(typeof(PlayerStats), typeof(DamageFlash))]
public class PlayerDeathHandler : MonoBehaviour
{
    [Tooltip("Seconds from death until the scene reloads.")]
    [Min(1f)] public float respawnDelay = 5f;
    [Tooltip("Font for YOU DIED. A classical serif such as Cinzel looks right.")]
    public Font font;
    public string deathText = "YOU DIED";
    public Color textColour = new Color(0.62f, 0.06f, 0.06f);
    private PlayerStats stats;
    private bool dead;
    private float diedAt;
    private GUIStyle textStyle;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        stats.onDeath.AddListener(HandleDeath);
    }

    private void HandleDeath()
    {
        if (dead) return;
        dead = true;
        diedAt = Time.unscaledTime;
        CombatAudio.Play(CombatSound.Death, transform.position);

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
        foreach (Inventory inventory in FindObjectsByType<Inventory>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (inventory.playerCombat == combat) inventory.enabled = false;

        CameraFollow cameraFollow = Camera.main != null
            ? Camera.main.GetComponent<CameraFollow>() : null;
        if (cameraFollow != null) cameraFollow.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(Respawn());
    }

    // Respawn by reloading the scene rather than restoring a saved world state.
    // Scene objects are recreated, but character choices in PlayerPrefs remain.
    private IEnumerator Respawn()
    {
        yield return new WaitForSecondsRealtime(respawnDelay);
        Scene scene = gameObject.scene;
        if (!Application.CanStreamedLevelBeLoaded(scene.path))
        {
#if UNITY_EDITOR
            // In the editor, test scenes respawn without having to be added to Build Settings.
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Debug.LogError("Add " + scene.path + " to enabled Scenes In Build for respawn.", this);
#endif
            yield break;
        }
        SceneManager.LoadScene(scene.path);
    }

    // Darken, a dark band across the middle, then YOU DIED fading in and slowly growing.
    // Just before the reload everything fades to black.
    private void OnGUI()
    {
        if (!dead) return;
        GUI.depth = -100;
        float t = Time.unscaledTime - diedAt;
        float toBlack = Mathf.Clamp01((t - (respawnDelay - 1f)) / 0.9f);
        Color previous = GUI.color;
        Matrix4x4 previousMatrix = GUI.matrix;

        float dim = Mathf.Lerp(0f, 0.55f, Mathf.Clamp01(t / 1.2f));
        Fill(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, Mathf.Max(dim, toBlack)));

        float bandAlpha = Mathf.Clamp01((t - 0.6f) / 1f) * 0.75f;
        float bandHeight = Screen.height * 0.17f;
        Rect band = new Rect(0f, Screen.height * 0.5f - bandHeight * 0.5f, Screen.width, bandHeight);
        Fill(band, new Color(0f, 0f, 0f, bandAlpha));

        float textAlpha = Mathf.Clamp01((t - 0.9f) / 1.6f) * (1f - toBlack);
        if (textAlpha > 0f)
        {
            if (textStyle == null) textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, font = font };
            textStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.09f);
            // A slow zoom, the way the Souls games present it.
            float scale = Mathf.Lerp(0.94f, 1.06f, Mathf.Clamp01((t - 0.9f) / respawnDelay));
            Vector2 centre = band.center;
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), centre);
            GUI.color = new Color(0f, 0f, 0f, textAlpha * 0.8f);
            GUI.Label(new Rect(band.x + 3f, band.y + 3f, band.width, band.height), deathText, textStyle);
            GUI.color = new Color(textColour.r, textColour.g, textColour.b, textAlpha);
            GUI.Label(band, deathText, textStyle);
            GUI.matrix = previousMatrix;
        }
        GUI.color = previous;
    }

    private static void Fill(Rect rect, Color colour)
    {
        GUI.color = colour;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
    }

    private void OnDestroy()
    {
        if (stats != null) stats.onDeath.RemoveListener(HandleDeath);
    }
}
