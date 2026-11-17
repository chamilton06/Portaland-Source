using UnityEngine;

// One attack the player can do with a hand-held weapon, e.g. a light or heavy attack.
// The name matches the attack's state on the Animator's Combat layer.
[System.Serializable]
public class HandAttack
{
    [Tooltip("The Combat layer state to play. Prepare Combat Animations names them after their clips.")]
    public string name;
    [Tooltip("How long the attack lasts before the next one can start.")]
    [Min(0.05f)] public float duration = 0.4f;
    [Tooltip("Seconds into the attack before the blade connects and damage lands.")]
    [Min(0f)] public float hitDelay = 0.18f;
    [Tooltip("Multiplies the weapon's damage, e.g. 1.5 for a stronger finishing hit.")]
    [Min(0f)] public float damageMultiplier = 1f;
    [Tooltip("Stamina spent when the attack starts. Not enough stamina means no attack.")]
    [Min(0f)] public float staminaCost = 10f;
    [Tooltip("Poise: damage the player can take during this attack without flinching, so a heavy swing " +
             "carries on through light hits. 0 = any hit interrupts it (needs Hit Stun on PlayerDefence).")]
    [Min(0f)] public float poise = 0f;
    [Tooltip("For multi-hit attacks: seconds into the attack for each hit after the first. Leave empty for a single hit.")]
    public float[] extraHitTimes;
}
