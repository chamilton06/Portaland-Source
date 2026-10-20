// ============================================================================
// PlayerStats.cs
// Tracks the player's hunger, stamina, and health. Drains hunger over time
// (faster when sprinting), spends and regenerates stamina, applies starvation
// and fall damage, regenerates health while well fed, and fires onDeath.
// Exposes 0-1 percentages for the UI bars.
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Configuration
    // ------------------------------------------------------------------------

    public float maxHunger = 100f;
    public float maxStamina = 100f;
    public float maxHealth = 100f;

    // hunger
    public float hungerLossPerMin = 10f;   // lost every 60 sec
    public float sprintHungerMult = 3f;    // drains faster when sprinting

    // stamina
    public float sprintStaminaCost = 20f;  // per sec
    public float jumpStaminaCost = 10f;
    public float staminaRegen = 15f;       // per sec
    public float regenDelay = 1f;          // wait before refill
    public float sprintAgainAt = 20f;      // needed to sprint again after running out

    // health
    public float starveDamage = 1f;        // per sec when hunger is 0
    public float safeFallSpeed = 10f;      // landing any faster than this hurts
    public float lethalFallSpeed = 20f;    // landing this fast removes kills the player
    public float healthRegen = 2f;         // per sec while well fed
    public float regenHungerThreshold = 90f;    // min hunger level necessary for regen

    // ------------------------------------------------------------------------
    // Debug
    // ------------------------------------------------------------------------

    public KeyCode debugEatKey = KeyCode.F;     // restores hunger
    public KeyCode debugQuarterHealth = KeyCode.P; // cuts hunger by 25%
    public float debugEatAmount = 25f;

    // ------------------------------------------------------------------------
    // State & Events
    // ------------------------------------------------------------------------
    public UnityEvent onDeath = new UnityEvent();

    private float hunger;
    private float stamina;
    private float health;
    private float regenTimer = 0f;
    private bool exhausted = false;
    private bool dead = false;

    private PlayerMovement movement;

    // ------------------------------------------------------------------------
    // Unity Callbacks
    // ------------------------------------------------------------------------

    // Fills all stats to max and caches PlayerMovement.
    void Start()
    {
        hunger = maxHunger;
        stamina = maxStamina;
        health = maxHealth;
        movement = GetComponent<PlayerMovement>();
    }

    // Drains hunger, spends/regenerates stamina, applies starvation or health regen, handles debug keys.
    void Update()
    {
        if (dead)
        {
            return;
        }

        bool sprinting = movement != null && movement.IsSprinting;

        // hunger
        float hungerLoss = hungerLossPerMin / 60f;
        if (sprinting)
        {
            hungerLoss *= sprintHungerMult;
        }
        hunger = Mathf.Max(0f, hunger - hungerLoss * Time.deltaTime);

        // stamina
        if (sprinting)
        {
            stamina = Mathf.Max(0f, stamina - sprintStaminaCost * Time.deltaTime);
            regenTimer = regenDelay;

            if (stamina <= 0f)
            {
                exhausted = true;
            }
        }
        else
        {
            regenTimer -= Time.deltaTime;
            if (regenTimer <= 0f)
            {
                stamina = Mathf.Min(maxStamina, stamina + staminaRegen * Time.deltaTime);
            }
        }

        if (exhausted && stamina >= sprintAgainAt)
        {
            exhausted = false;
        }

        // health, starving or regenerating
        if (hunger <= 0f)
        {
            TakeDamage(starveDamage * Time.deltaTime);
        }
        else if (hunger > regenHungerThreshold && health < maxHealth)
        {
            health = Mathf.Min(maxHealth, health + healthRegen * Time.deltaTime);
        }

        // debug refill
        if (Input.GetKeyDown(debugEatKey))
        {
            Eat(debugEatAmount);
        }
        if (Input.GetKeyDown(debugQuarterHealth))
        {
            hunger *= 0.75f;
        }
    }

    // ------------------------------------------------------------------------
    // Damage & Death
    // ------------------------------------------------------------------------

    // player takes damage when they fall
    public void ApplyFallDamage (float impactSpeed)
    {
        if (impactSpeed <= safeFallSpeed) return;   // don't trigger if the player is at a safe height
        float t = Mathf.InverseLerp(safeFallSpeed, lethalFallSpeed, impactSpeed);
        TakeDamage(t * maxHealth);
    }

    // Reduces health and triggers Die at zero. Ignored if dead or damage is zero.
    public void TakeDamage(float damage)
    {
        if (dead || damage <= 0f)   //already dead or no damage, stop
        {
            return;
        }

        health = Mathf.Max(0f, health - damage);

        if (health <= 0f)
        {
            Die();
        }
    }

    // Marks the player dead, disables movement, and fires onDeath.
    void Die()
    {
        dead = true;
        Debug.Log(gameObject.name + " died");

        if (movement != null)
        {
            movement.enabled = false;
        }

        onDeath.Invoke();
    }

    // ------------------------------------------------------------------------
    // Hunger & Stamina
    // ------------------------------------------------------------------------

    // Restores hunger, capped at max.
    public void Eat(float amount)
    {
        if (dead)
        {
            return;
        }
        hunger = Mathf.Min(maxHunger, hunger + amount);
    }

    // Returns true if the player is allowed to start/continue sprinting.
    public bool CanSprint()
    {
        return !dead && !exhausted && stamina > 0f;
    }

    // Spends the jump's stamina cost if available.
    public bool TryUseJumpStamina()
    {
        return TryUseStamina(jumpStaminaCost);
    }

    // spends stamina if theres enough, use for attacks too
    public bool TryUseStamina(float amount)
    {
        if (dead || stamina < amount)
        {
            return false;
        }

        stamina -= amount;
        regenTimer = regenDelay;
        return true;
    }

    // ------------------------------------------------------------------------
    // UI & Status Queries
    // ------------------------------------------------------------------------

    // 0 to 1 values for ui bars
    public float GetHungerPercent() { return hunger / maxHunger; }
    public float GetStaminaPercent() { return stamina / maxStamina; }
    public float GetHealthPercent() { return health / maxHealth; }

    // Returns true once the player has died.
    public bool IsDead() { return dead; }
}