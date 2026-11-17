using System.Collections.Generic;
using UnityEngine;

// Turns an enemy into a boss fight. Put it on the enemy in the scene, not on its prefab, so
// the same enemy can still be used as a normal one elsewhere.
//
// The boss waits until the player comes close on the same floor (or hits it). Then its name and
// a big health bar appear at the bottom of the screen and the music starts. When it dies a
// banner shows and the music fades. If the player dies the scene reloads, which resets the fight.
[RequireComponent(typeof(Health))]
public class BossEncounter : MonoBehaviour
{
    [Tooltip("Shown above the health bar.")]
    public string bossName = "Servant of Dracula";

    [Header("Waking up")]
    [Tooltip("The fight starts when the player is this close.")]
    [Min(1f)] public float wakeRange = 10f;
    [Tooltip("Only counts if the player is within this many metres up or down, so walking under the floor or past the outside wall doesn't start it.")]
    [Min(0.5f)] public float sameFloorHeight = 2.5f;

    [Header("Arena")]
    [Tooltip("Optional. Switched on when the fight starts and off when the boss dies, e.g. something blocking the door.")]
    public GameObject barrier;

    [Header("Music")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Min(0f)] public float musicFade = 2f;
    [Tooltip("Played once when the boss dies.")]
    public AudioClip victorySound;

    [Header("Victory")]
    public string victoryText = "VANQUISHED";
    [Min(0.5f)] public float bannerTime = 4f;

    [Header("Health bar")]
    [Tooltip("Width as a share of the screen.")]
    [Range(0.2f, 0.9f)] public float barWidth = 0.55f;
    [Tooltip("How far down the screen the bar sits (0 = top, 1 = bottom). Kept above the hotbar.")]
    [Range(0.1f, 0.95f)] public float barHeight = 0.74f;
    [Tooltip("Seconds before the yellow chunk of recent damage drains away.")]
    [Min(0f)] public float chipDelay = 0.6f;

    // Health bars over enemies leave bosses out, since they have this bar instead.
    private static readonly HashSet<Health> bosses = new HashSet<Health>();
    public static bool IsBoss(Health health) => health != null && bosses.Contains(health);

    private enum Phase { Waiting, Fighting, Won }
    private Phase phase;
    private Health health;
    private EnemyMelee enemy;
    private PlayerStats player;
    private AudioSource musicSource;
    private float shownAt = float.NegativeInfinity;
    private float wonAt;
    private float chip = 1f;
    private float lastHealth;
    private float lastHitTime = float.NegativeInfinity;
    private GUIStyle nameStyle, bannerStyle;

    private void Awake()
    {
        health = GetComponent<Health>();
        enemy = GetComponent<EnemyMelee>();
        if (enemy != null) enemy.waitToBeWoken = true;
        bosses.Add(health);
        health.onDeath.AddListener(Win);
        if (barrier != null) barrier.SetActive(false);
    }

    private void OnDestroy()
    {
        bosses.Remove(health);
        if (health != null) health.onDeath.RemoveListener(Win);
    }

    private void Start()
    {
        player = FindFirstObjectByType<PlayerStats>();
        lastHealth = health.maxHealth;
    }

    private void Update()
    {
        switch (phase)
        {
            case Phase.Waiting:
                // Hitting it first counts too.
                bool hurt = health.CurrentHealth > 0 && health.CurrentHealth < health.maxHealth;
                if (hurt || PlayerInRange()) Begin();
                break;

            case Phase.Fighting:
                UpdateChip();
                if (player != null && player.IsDead()) FadeMusicTo(0f);
                break;

            case Phase.Won:
                UpdateChip();
                break;
        }

        if (musicSource != null && musicSource.isPlaying && musicSource.volume <= 0.001f && phase != Phase.Fighting)
            musicSource.Stop();
    }

    private bool PlayerInRange()
    {
        if (player == null || player.IsDead()) return false;
        Vector3 offset = player.transform.position - transform.position;
        if (Mathf.Abs(offset.y) > sameFloorHeight) return false;
        offset.y = 0f;
        return offset.magnitude <= wakeRange;
    }

    private void Begin()
    {
        phase = Phase.Fighting;
        shownAt = Time.time;
        if (enemy != null) enemy.WakeUp();
        if (barrier != null) barrier.SetActive(true);
        if (music != null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f; // Plays at the same volume wherever the camera is.
            musicSource.volume = 0f;
            musicSource.Play();
            FadeMusicTo(musicVolume);
        }
    }

    private void Win()
    {
        if (phase == Phase.Won) return;
        if (phase == Phase.Waiting) shownAt = Time.time;
        phase = Phase.Won;
        wonAt = Time.time;
        if (barrier != null) barrier.SetActive(false);
        FadeMusicTo(0f);
        if (victorySound != null) AudioSource.PlayClipAtPoint(victorySound, Camera.main != null ? Camera.main.transform.position : transform.position);
    }

    private float musicTarget;
    private void FadeMusicTo(float volume) => musicTarget = volume;

    private void LateUpdate()
    {
        if (musicSource == null) return;
        float speed = musicFade > 0f ? musicVolume / musicFade : 100f;
        musicSource.volume = Mathf.MoveTowards(musicSource.volume, musicTarget, speed * Time.deltaTime);
    }

    // The yellow chunk waits a moment after a hit, then drains down to the real health.
    private void UpdateChip()
    {
        float current = health.CurrentHealth;
        if (current < lastHealth) lastHitTime = Time.time;
        lastHealth = current;
        float fraction = Fraction();
        if (chip < fraction || Time.time - lastHitTime > chipDelay)
            chip = Mathf.MoveTowards(chip, fraction, Time.deltaTime * 0.5f);
    }

    private float Fraction() =>
        health.maxHealth > 0 ? Mathf.Clamp01((float)health.CurrentHealth / health.maxHealth) : 0f;

    // ------------------------------------------------------------------------
    // Drawing
    // ------------------------------------------------------------------------

    private void OnGUI()
    {
        if (phase == Phase.Waiting) return;
        if (nameStyle == null)
        {
            nameStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Normal, alignment = TextAnchor.LowerLeft };
            bannerStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        }
        Color previous = GUI.color;

        // The bar fades in when the fight starts, and out a couple of seconds after the boss dies.
        float alpha = Mathf.Clamp01((Time.time - shownAt) / 0.6f);
        if (phase == Phase.Won) alpha *= Mathf.Clamp01(1f - (Time.time - wonAt - 2f));
        if (alpha > 0f) DrawBar(alpha);

        if (phase == Phase.Won) DrawBanner(Time.time - wonAt);
        GUI.color = previous;
    }

    private void DrawBar(float alpha)
    {
        float width = Screen.width * barWidth;
        float height = Mathf.Max(6f, Screen.height * 0.012f);
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * barHeight;

        nameStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.024f);
        GUI.color = new Color(0f, 0f, 0f, 0.8f * alpha);
        GUI.Label(new Rect(x + 1f, y - nameStyle.fontSize * 1.6f + 1f, width, nameStyle.fontSize * 1.5f), bossName, nameStyle);
        GUI.color = new Color(0.93f, 0.9f, 0.82f, alpha);
        GUI.Label(new Rect(x, y - nameStyle.fontSize * 1.6f, width, nameStyle.fontSize * 1.5f), bossName, nameStyle);

        Rect frame = new Rect(x, y, width, height);
        Draw(new Rect(frame.x - 2f, frame.y - 2f, frame.width + 4f, frame.height + 4f), new Color(0.55f, 0.47f, 0.3f, 0.9f * alpha));
        Draw(new Rect(frame.x - 1f, frame.y - 1f, frame.width + 2f, frame.height + 2f), new Color(0f, 0f, 0f, 0.9f * alpha));
        Draw(frame, new Color(0.1f, 0.1f, 0.1f, 0.85f * alpha));
        Draw(new Rect(frame.x, frame.y, frame.width * chip, frame.height), new Color(0.95f, 0.8f, 0.25f, alpha));
        Draw(new Rect(frame.x, frame.y, frame.width * Fraction(), frame.height), new Color(0.6f, 0.05f, 0.05f, alpha));
    }

    // Fades in over a second, holds, then fades out.
    private void DrawBanner(float time)
    {
        float alpha = Mathf.Clamp01(time) * Mathf.Clamp01(bannerTime + 1f - time);
        if (alpha <= 0f) return;

        float stripe = Screen.height * 0.14f;
        Rect band = new Rect(0f, Screen.height * 0.42f - stripe * 0.5f, Screen.width, stripe);
        Draw(band, new Color(0f, 0f, 0f, 0.55f * alpha));

        bannerStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.07f);
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.Label(new Rect(band.x + 2f, band.y + 2f, band.width, band.height), victoryText, bannerStyle);
        GUI.color = new Color(0.92f, 0.76f, 0.38f, alpha);
        GUI.Label(band, victoryText, bannerStyle);
    }

    private static void Draw(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
    }
}
