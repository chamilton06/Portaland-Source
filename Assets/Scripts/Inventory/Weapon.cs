// ============================================================================
// Weapon.cs
// Defines the Weapon ScriptableObject (an Equipment subclass) and the
// WeaponStats data it holds: damage, attack range, and attack cooldown.
// ============================================================================

using UnityEngine;

// ============================================================================
// Weapon
// ============================================================================

[CreateAssetMenu(fileName = "Weapon", menuName = "New Weapon")]
public class Weapon : Equipment
{
    // ------------------------------------------------------------------------
    // Configuration
    // ------------------------------------------------------------------------
    public WeaponStats stats;
    public GameObject weaponPrefab;     // Weapon model that PlayerCombat swings
}

// ============================================================================
// WeaponStats
// ============================================================================

// Combat numbers for a weapon. Serializable so it shows up in the Weapon inspector.
[System.Serializable]
public class WeaponStats
{
    public int damage = 10;
    public float attackRange = 2.5f;
    public float attackCooldown = 0.5f;
}