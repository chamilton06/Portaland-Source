using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Shows hunger, stamina and movement state as text, updated every frame.

public class StatsUI : MonoBehaviour
{
    public PlayerStats playerStats;
    public PlayerMovement playerMovement;

    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI stateText;

    public float lowThreshold = 25f;   // text turns red below this

    void Update()
    {
        if (playerStats == null) return;

        float hunger = playerStats.CurrentHunger;
        float stamina = playerStats.CurrentStamina;

        hungerText.text = "Hunger: " + Mathf.CeilToInt(hunger) + " / " + playerStats.maxHunger
                          + (playerStats.IsStarving ? "  (STARVING - press E to eat)" : "");
        hungerText.color = hunger <= lowThreshold ? Color.red : Color.white;

        staminaText.text = "Stamina: " + Mathf.CeilToInt(stamina) + " / " + playerStats.maxStamina
                           + (playerStats.IsExhausted ? "  (EXHAUSTED)" : "");
        staminaText.color = stamina <= lowThreshold ? Color.red : Color.white;

        if (playerMovement != null)
            stateText.text = "State: " + playerMovement.CurrentState;
    }
}