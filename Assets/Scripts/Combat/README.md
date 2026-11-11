# Combat scripts

The full guide is in [`docs/combat`](../../../docs/combat/README.md): controls, how fights
work, the setup tool and tuning. If you're adding enemies to a level, read
[Adding enemies](../../../docs/combat/adding-enemies.md).

To try it, open `Assets/Scenes/Tests/RangerMovementTest.unity` and press Play.

## What's in this folder

- **Player:** `PlayerCombat`, `PlayerDefence`, `PlayerLockOn`, `PlayerDeathHandler`, `HandAttack`
- **Enemies:** `EnemyMelee`, `EnemyAttack`, `EnemyHealthBars`, `DummyAttacker`
- **Shared:** `Health`, `DamageFlash`, `HeldWeaponModel`, and the `IStaggerable`,
  `IBackstabbable` and `IHitReceiver` interfaces
- **Item Drops:** food and item pickups that enemies can drop when they die

## Things worth knowing

The player's attacks hit whatever is straight in front of the player, within the weapon's range.
They don't aim through the camera, so there's no crosshair.

Weapons are equipped through `PlayerCombat.SetEquippedWeapon(Weapon)`. That's what the inventory
calls. In the Ranger test scenes the sword sits in the character's right hand. Prototype still
uses the old floating weapon pivot.

The player and enemies flash red when they take damage. Blocked, parried and dodged hits don't
flash. Health regenerates while hunger is above 90, so if you want to see damage stick, press P
to drop your hunger or set Health Regen to 0 on PlayerStats while you test.

When the player dies, "You died" shows and the scene reloads after 3 seconds. The reload only
works for scenes listed in **File → Build Settings**, so a test scene that isn't listed won't
respawn you.
