using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Holds the player's survival stats (hunger + stamina) and updates them over time.
// Other scripts READ these values (UI) or CHANGE them (movement).
public class PlayerStats : MonoBehaviour
{
    [Header("Hunger")]
    public float maxHunger = 100f;
    public float hungerDrainPerSecond = 2f;      // goes down constantly
    public float foodRestoreAmount = 25f;        // how much "Eat" gives back

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 25f;    // while sprinting
    public float staminaRegenPerSecond = 15f;    // while not sprinting
    public float staminaRegenDelay = 1f;         // wait before regen starts
    public float jumpStaminaCost = 15f;
    public float exhaustedRecoverAt = 25f;       // must regen to this before sprinting again

    public float CurrentHunger { get; private set; }
    public float CurrentStamina { get; private set; }
    public bool IsExhausted { get; private set; }
    public bool IsStarving => CurrentHunger <= 0f;

    private float regenTimer;

    void Start()
    {
        CurrentHunger = maxHunger;
        CurrentStamina = maxStamina;
    }

    void Update()
    {
        // Hunger always goes down
        CurrentHunger = Mathf.Max(0f, CurrentHunger - hungerDrainPerSecond * Time.deltaTime);

        // Stamina regenerates after a short delay (slower when starving)
        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
        }
        else if (CurrentStamina < maxStamina)
        {
            float regen = IsStarving ? staminaRegenPerSecond * 0.25f : staminaRegenPerSecond;
            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + regen * Time.deltaTime);
        }

        if (IsExhausted && CurrentStamina >= exhaustedRecoverAt)
            IsExhausted = false;
    }

    public bool HasStamina(float amount)
    {
        return CurrentStamina >= amount;
    }

    public void DrainStamina(float amount)
    {
        CurrentStamina = Mathf.Max(0f, CurrentStamina - amount);
        regenTimer = staminaRegenDelay;
        if (CurrentStamina <= 0f) IsExhausted = true;
    }

    public void Eat()
    {
        CurrentHunger = Mathf.Min(maxHunger, CurrentHunger + foodRestoreAmount);
    }
}