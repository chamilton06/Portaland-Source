using System.Collections.Generic;
using UnityEngine;

// Souls-style health bars over enemies. Goes on the Player. A thin bar appears over any enemy
// (anything with Health that isn't the player) once it takes damage or is locked onto, and fades
// a few seconds after the last hit. Recent damage shows as a yellow chunk that drains away,
// with the combo's total damage as a number beside the bar.
public class EnemyHealthBars : MonoBehaviour
{
    [Tooltip("Seconds a bar stays after the enemy was last hit (it always shows while locked on).")]
    [Min(0.5f)] public float showTime = 6f;
    [Tooltip("Hits within this many seconds of each other add up in the damage number.")]
    [Min(0.1f)] public float comboTime = 2f;
    [Tooltip("Seconds before the yellow chunk of recent damage drains away.")]
    [Min(0f)] public float chipDelay = 0.6f;
    [Tooltip("Bars are hidden beyond this distance.")]
    [Min(1f)] public float maxDistance = 30f;
    [Tooltip("Bar width as a share of the screen height.")]
    [Range(0.02f, 0.3f)] public float barWidth = 0.11f;
    [Tooltip("Metres above the enemy's head.")]
    public float heightAboveHead = 0.35f;

    private class Bar
    {
        public float lastHealth = -1f;
        public float chip;            // Where the yellow chunk ends, 0 to 1.
        public float lastHitTime = float.NegativeInfinity;
        public int comboDamage;
        public bool dead;
        public float diedAt;
        public Transform head;
    }

    private readonly Dictionary<Health, Bar> bars = new Dictionary<Health, Bar>();
    private float nextRefresh;
    private PlayerLockOn lockOn;
    private Camera viewCamera;
    private GUIStyle numberStyle;

    private void Awake()
    {
        lockOn = GetComponent<PlayerLockOn>();
    }

    private void Update()
    {
        // New enemies can spawn, so look for them about once a second.
        if (Time.time >= nextRefresh)
        {
            nextRefresh = Time.time + 1f;
            foreach (Health health in FindObjectsByType<Health>(FindObjectsSortMode.None))
                if (!bars.ContainsKey(health) && !health.transform.IsChildOf(transform) && !transform.IsChildOf(health.transform))
                    bars.Add(health, NewBar(health));
        }

        foreach (KeyValuePair<Health, Bar> pair in bars)
        {
            Health health = pair.Key;
            Bar bar = pair.Value;
            if (health == null) continue;
            float current = health.CurrentHealth;
            if (bar.lastHealth < 0f)
            {
                // Health fills in on its first frame; start from there.
                if (current > 0f) { bar.lastHealth = current; bar.chip = Fraction(health); }
                continue;
            }

            if (current < bar.lastHealth)
            {
                int damage = Mathf.RoundToInt(bar.lastHealth - current);
                bar.comboDamage = Time.time - bar.lastHitTime <= comboTime ? bar.comboDamage + damage : damage;
                bar.lastHitTime = Time.time;
                if (current <= 0f && !bar.dead) { bar.dead = true; bar.diedAt = Time.time; }
            }
            bar.lastHealth = current;

            // The yellow chunk waits a moment, then drains down to the real health.
            float fraction = Fraction(health);
            if (bar.chip < fraction || Time.time - bar.lastHitTime > chipDelay)
                bar.chip = Mathf.MoveTowards(bar.chip, fraction, Time.deltaTime * 0.8f);
        }
    }

    private static Bar NewBar(Health health)
    {
        Bar bar = new Bar();
        Animator animator = health.GetComponentInChildren<Animator>();
        if (animator != null && animator.isHuman) bar.head = animator.GetBoneTransform(HumanBodyBones.Head);
        return bar;
    }

    private static float Fraction(Health health) =>
        health.maxHealth > 0 ? Mathf.Clamp01((float)health.CurrentHealth / health.maxHealth) : 0f;

    private void OnGUI()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (viewCamera == null) return;

        float width = Screen.height * barWidth;
        float height = Mathf.Max(3f, Screen.height * 0.006f);
        if (numberStyle == null) numberStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
        numberStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.016f);

        Color previous = GUI.color;
        foreach (KeyValuePair<Health, Bar> pair in bars)
        {
            Health health = pair.Key;
            Bar bar = pair.Value;
            if (health == null || !health.isActiveAndEnabled) continue;

            bool locked = lockOn != null && lockOn.Target == health;
            float sinceHit = Time.time - bar.lastHitTime;
            // A killed enemy's bar empties, then goes after a second.
            if (bar.dead && Time.time - bar.diedAt > 1f) continue;
            if (!locked && sinceHit > showTime) continue;
            if (Vector3.Distance(transform.position, health.transform.position) > maxDistance) continue;

            Vector3 anchor = bar.head != null ? bar.head.position + Vector3.up * heightAboveHead
                : health.transform.position + Vector3.up * 2f;
            Vector3 point = viewCamera.WorldToScreenPoint(anchor);
            if (point.z <= 0f) continue;

            // Fade out over the last second.
            float alpha = locked ? 1f : Mathf.Clamp01(showTime - sinceHit);
            Rect frame = new Rect(point.x - width * 0.5f, Screen.height - point.y - height * 0.5f, width, height);
            Draw(new Rect(frame.x - 1f, frame.y - 1f, frame.width + 2f, frame.height + 2f), new Color(0f, 0f, 0f, 0.75f * alpha));
            Draw(frame, new Color(0.12f, 0.12f, 0.12f, 0.8f * alpha));
            Draw(new Rect(frame.x, frame.y, frame.width * bar.chip, frame.height), new Color(0.95f, 0.8f, 0.25f, alpha));
            Draw(new Rect(frame.x, frame.y, frame.width * Fraction(health), frame.height), new Color(0.7f, 0.08f, 0.08f, alpha));

            if (sinceHit <= comboTime + 1f && bar.comboDamage > 0)
            {
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Label(new Rect(frame.xMax + 6f, frame.center.y - numberStyle.fontSize, 100f, numberStyle.fontSize * 2f),
                    bar.comboDamage.ToString(), numberStyle);
            }
        }
        GUI.color = previous;
    }

    private static void Draw(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
    }
}
