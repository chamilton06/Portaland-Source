# Combat prototype

Open `Assets/Scenes/CombatDefenceTest.unity`and press Play. I made this as a dupilcated copy of Prototype for testing combat without changing the shared scene. 

## Controls


- Hold right click to block. There is no stamina cost or time limit for blocking.
- Press right click just before a hit lands to parry. The window is 0.2 seconds, and a successful parry shows a "Parried".
- Q to dodge/roll. Hold a movement direction to roll that way, or stand still to roll forward.


Blocking and parrying only protect the front of the player.

## The two dummies

`StillDummy` is the passive target for checking weapon swings, damage and the red hit flash.

`AttackingDummy` has a weapon and attacks when you get close. It shows "Preparing attack...", then "Striking!" as it swings. Step out of range or move around it during the strike to make it miss. Its facing direction locks during the swing.

## Weapons and rolling

Player attacks check straight ahead from the player, rather than aiming through the camera crosshair. So if you wish you may now remove the crosshair as it is not needed.

The equipped model is created under the player's WeaponPivot using the weapon asset's Weapon Prefab reference. The combat side still uses `SetEquippedWeapon(Weapon)` so inventory can select the weapon through that method. Inventory selection and unequipping interactions are being handled separately by James. New models should have their grip at the prefab origin and use a consistent orientation.



## Taking damage and respawning

Both the player and dummies flash red when they take damage. Blocked, parried and dodged hits do not trigger the player's damage flash. Fall damage and starvation can also make the player flash.

At zero health, player actions stop and "You died" appears. After 3 seconds, the whole test scene reloads. This resets health, stamina, equipment to its scene setup, both dummies and dropped items; it does not preserve progress.

For respawn to work, add `CombatDefenceTest` to File > Build Settings > Scenes In Build and keep it enabled. The saved Build Settings may still point only at the archived movement scene, so check this before testing or building.

Health regenerates while hunger is above 90, so it can recover between enemy hits. For testing, P reduces hunger by 25% and F restores 25 hunger. You can also temporarily set Health Regen to 0 during Play mode to make damage easier to see.



