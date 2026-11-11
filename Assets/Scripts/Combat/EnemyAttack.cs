using UnityEngine;

// One attack in an enemy's moveset (see EnemyMelee). Times are in animation seconds,
// so they still line up when a delayed attack slows its wind-up.
[System.Serializable]
public class EnemyAttack
{
    [Tooltip("Name shown in the Console when the enemy uses it.")]
    public string label = "Slash";
    [Tooltip("The Combat layer state to play, e.g. Sword_Regular_A.")]
    public string state;
    [Min(0f)] public float damage = 12f;
    [Tooltip("The enemy closes to this distance before it starts the attack. It's also how far the strike reaches.")]
    [Min(0.5f)] public float reach = 2.3f;
    [Tooltip("Seconds into the animation when the blade connects.")]
    [Min(0f)] public float hitTime = 0.4f;
    [Tooltip("Seconds of animation before the attack is over.")]
    [Min(0.1f)] public float duration = 0.9f;
    [Tooltip("Seconds into the animation that the enemy keeps turning to follow the player. After this, side-stepping can make it miss.")]
    [Min(0f)] public float trackUntil = 0.25f;
    [Tooltip("Metres the enemy lunges forward during the wind-up.")]
    [Min(0f)] public float lunge = 0.6f;
    [Header("Delayed swing")]
    [Tooltip("Seconds into the animation to hold the raised weapon. 0 = no delay.")]
    [Min(0f)] public float holdAt = 0f;
    [Tooltip("Real seconds the wind-up is held, to catch players who roll too early.")]
    [Min(0f)] public float holdFor = 0f;
    [Header("Afterwards")]
    [Tooltip("Seconds the enemy stands open after the attack: the time to punish it.")]
    [Min(0f)] public float recovery = 0.6f;
    [Tooltip("Index of an attack in the moveset that can follow straight on as a combo. -1 = none.")]
    public int followUp = -1;
    [Range(0f, 1f)] public float followUpChance = 0.5f;
}
