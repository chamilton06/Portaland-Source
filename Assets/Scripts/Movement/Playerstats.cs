using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
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
    public float safeFallSpeed = 14f;      // landing any faster than this hurts
    public float lethalFallSpeed = 20f;    // landing this fast removes kills the player

    // debug
    public KeyCode debugEatKey = KeyCode.F;
    public float debugEatAmount = 25f;

    public UnityEvent onDeath = new UnityEvent();

    private float hunger;
    private float stamina;
    private float health;
    private float regenTimer = 0f;
    private bool exhausted = false;
    private bool dead = false;

    private PlayerMovement movement;

    void Start()
    {
        hunger = maxHunger;
        stamina = maxStamina;
        health = maxHealth;
        movement = GetComponent<PlayerMovement>();
    }

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

        // health, starving
        if (hunger <= 0f)
        {
            TakeDamage(starveDamage * Time.deltaTime);
        }

        // debug refill
        if (Input.GetKeyDown(debugEatKey))
        {
            Eat(debugEatAmount);
        }
    }

    // triggered when player falls
    public void ApplyFallDamage (float impactSpeed)
    {
        if (impactSpeed <= safeFallSpeed) return;   // don't trigger if the player is at a safe height
        Debug.Log("Fall impact: " + impactSpeed);
        float t = Mathf.InverseLerp(safeFallSpeed, lethalFallSpeed, impactSpeed);
        TakeDamage(t * maxHealth);
    }

    public bool CanSprint()
    {
        return !dead && !exhausted && stamina > 0f;
    }

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

    public void Eat(float amount)
    {
        if (dead)
        {
            return;
        }
        hunger = Mathf.Min(maxHunger, hunger + amount);
    }

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

    // 0 to 1 values for ui bars
    public float GetHungerPercent()
    {
        return hunger / maxHunger;
    }

    public float GetStaminaPercent()
    {
        return stamina / maxStamina;
    }

    public float GetHealthPercent()
    {
        return health / maxHealth;
    }

    public bool IsDead()
    {
        return dead;
    }
}