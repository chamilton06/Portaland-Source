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
    [Tooltip("Font for the boss name, VANQUISHED and the reward popup. A classical serif such as Cinzel looks right.")]
    public Font font;

    [Header("Waking up")]
    [Tooltip("The fight starts when the player is this close.")]
    [Min(1f)] public float wakeRange = 10f;
    [Tooltip("Only counts if the player is within this many metres up or down, so walking under the floor or past the outside wall doesn't start it.")]
    [Min(0.5f)] public float sameFloorHeight = 2.5f;

    [Header("Difficulty")]
    [Tooltip("Multiplies the boss's attack damage.")]
    [Min(0f)] public float damageMultiplier = 1.2f;
    [Tooltip("Multiplies how long it circles before attacking. Lower = attacks more often.")]
    [Range(0.1f, 1f)] public float circleTimeScale = 0.6f;
    [Tooltip("Multiplies how long it stands open after attacking. Lower = more relentless, but smaller openings to punish.")]
    [Range(0.1f, 1f)] public float recoveryScale = 1f;
    [Tooltip("Damage it takes (within a few seconds) before its poise breaks and it staggers. Light sword hits do 20.")]
    [Min(1f)] public float poise = 100f;
    [Tooltip("Seconds after a poise break before it can be broken again, so it can't be stun-locked.")]
    [Min(0f)] public float poiseBreakCooldown = 8f;
    [Tooltip("Hits in a row (within 2.5 s) before it cuts into the player's combo. 0 = never.")]
    [Min(0)] public int retaliateAfterHits = 3;

    [Header("Arena")]
    [Tooltip("Optional. Switched on when the fight starts (when the boss bar appears) and off when the boss dies, e.g. an Arena Barrier in each doorway. Hidden until then.")]
    public GameObject[] barriers;

    [Header("Music")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Min(0f)] public float musicFade = 2f;
    [Tooltip("Leave empty to use the game's victory sound (Victory on Assets/Audio/CombatSounds). Only set it to give this one boss a different sound.")]
    public AudioClip victorySound;

    [Header("Victory")]
    public string victoryText = "VANQUISHED";
    [Min(0.5f)] public float bannerTime = 4f;

    [Header("Reward")]
    [Tooltip("Given straight to the player when the boss dies, with an 'obtained' popup. No pickup to walk to.")]
    public Item reward;
    [Min(1)] public int rewardAmount = 1;
    [Tooltip("Seconds after the death before the reward is given, so it follows the banner.")]
    [Min(0f)] public float rewardDelay = 2.5f;
    [Min(0.5f)] public float rewardPopupTime = 3.5f;

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
    private GUIStyle nameStyle, bannerStyle, rewardStyle;
    private float rewardShownAt = float.NegativeInfinity;

    private void Awake()
    {
        health = GetComponent<Health>();
        enemy = GetComponent<EnemyMelee>();
        if (enemy != null)
        {
            enemy.waitToBeWoken = true;
            enemy.damageMultiplier = damageMultiplier;
            enemy.recoveryScale = recoveryScale;
            enemy.circleTime *= circleTimeScale;
            // Bosses have hyper armour: hits don't flinch them, only a poise break staggers them.
            enemy.flinches = false;
            enemy.poise = poise;
            enemy.poiseBreakCooldown = poiseBreakCooldown;
            enemy.retaliateAfterHits = retaliateAfterHits;
        }
        bosses.Add(health);
        health.onDeath.AddListener(Win);
        foreach (GameObject barrier in Barriers()) barrier.SetActive(false);
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

    private System.Collections.Generic.IEnumerable<GameObject> Barriers()
    {
        if (barriers == null) yield break;
        foreach (GameObject barrier in barriers)
            if (barrier != null) yield return barrier;
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
        foreach (GameObject barrier in Barriers()) barrier.SetActive(true);
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
        foreach (GameObject barrier in Barriers())
        {
            ArenaBarrier mist = barrier.GetComponent<ArenaBarrier>();
            if (mist != null) mist.Clear();       // Fades away; the way out opens straight away.
            else barrier.SetActive(false);
        }
        FadeMusicTo(0f);
        if (victorySound != null) AudioSource.PlayClipAtPoint(victorySound, Camera.main != null ? Camera.main.transform.position : transform.position);
        else CombatAudio.Play(CombatSound.Victory, transform.position);
        if (reward != null) StartCoroutine(GiveReward());
    }

    private System.Collections.IEnumerator GiveReward()
    {
        yield return new WaitForSeconds(rewardDelay);
        Inventory inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning(bossName + ": no Inventory in the scene, so the reward (" + reward.itemName + ") couldn't be given.", this);
            yield break;
        }
        inventory.AddItem(reward, rewardAmount);
        rewardShownAt = Time.time;
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
            nameStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Normal, alignment = TextAnchor.LowerLeft, font = font };
            bannerStyle = new GUIStyle(GUI.skin.label) { fontStyle = font != null ? FontStyle.Normal : FontStyle.Bold, alignment = TextAnchor.MiddleCenter, font = font };
        }
        Color previous = GUI.color;

        // The bar fades in when the fight starts, and out a couple of seconds after the boss dies.
        float alpha = Mathf.Clamp01((Time.time - shownAt) / 0.6f);
        if (phase == Phase.Won) alpha *= Mathf.Clamp01(1f - (Time.time - wonAt - 2f));
        if (alpha > 0f) DrawBar(alpha);

        if (phase == Phase.Won) DrawBanner(Time.time - wonAt);
        if (reward != null) DrawReward(Time.time - rewardShownAt);
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

    // "Item obtained": the reward's icon and name in a dark box, fading in and out.
    private void DrawReward(float time)
    {
        float alpha = Mathf.Clamp01(time / 0.3f) * Mathf.Clamp01(rewardPopupTime - time);
        if (alpha <= 0f) return;
        if (rewardStyle == null) rewardStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, font = font };
        rewardStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.026f);

        float height = Screen.height * 0.08f;
        float width = Screen.height * 0.42f;
        Rect box = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.58f, width, height);
        Draw(box, new Color(0f, 0f, 0f, 0.7f * alpha));
        Draw(new Rect(box.x, box.y, box.width, 1f), new Color(0.55f, 0.47f, 0.3f, 0.8f * alpha));
        Draw(new Rect(box.x, box.yMax - 1f, box.width, 1f), new Color(0.55f, 0.47f, 0.3f, 0.8f * alpha));

        float pad = height * 0.15f;
        Rect iconRect = new Rect(box.x + pad, box.y + pad, height - pad * 2f, height - pad * 2f);
        if (reward.icon != null)
        {
            Sprite icon = reward.icon;
            Rect uv = new Rect(icon.textureRect.x / icon.texture.width, icon.textureRect.y / icon.texture.height,
                icon.textureRect.width / icon.texture.width, icon.textureRect.height / icon.texture.height);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTextureWithTexCoords(iconRect, icon.texture, uv);
        }
        string label = reward.itemName + (rewardAmount > 1 ? "  x" + rewardAmount : "");
        GUI.color = new Color(0.93f, 0.9f, 0.82f, alpha);
        GUI.Label(new Rect(iconRect.xMax + pad, box.y, box.width - iconRect.width - pad * 3f, box.height), label, rewardStyle);
    }

    private static void Draw(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
    }
}
