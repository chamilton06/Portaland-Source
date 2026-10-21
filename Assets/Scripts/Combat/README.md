# Combat prototype

Open `Assets/Scenes/CombatDefenceTest.unity` in Unity 2023.1.22f1 and press Play. This is a separate copy of Prototype for testing combat without changing the shared scene. Click inside the Game view to control the player. If the inventory is covering the screen, press Tab to close it.

## Controls

- WASD to move, mouse to look around.
- Left Shift to sprint, Space to jump, Left Ctrl to crouch.
- Left click to attack with the equipped weapon.
- Hold right click to block. There is no stamina cost or time limit for blocking in this prototype.
- Press right click just before a hit lands to parry. The window is 0.2 seconds, and a successful parry briefly shows "Parried". Holding the button does not refresh the window; release and press again for another attempt.
- Q to dodge roll. Hold a movement direction to roll that way, or stand still to roll forward.
- Tab to open or close the inventory. Escape frees the cursor.

Blocking and parrying only protect the front of the player. They currently use the same simple raised-weapon animation. Parrying does not stun the dummy yet. You cannot attack while guarding or rolling, and opening the inventory cancels those actions. After rolling, release and press right click again to guard.

## The two dummies

`StillDummy` is the passive target for checking weapon swings, damage and the red hit flash.

`AttackingDummy` has a weapon and attacks when you get close. It shows "Preparing attack...", then "Striking!" as it swings. Step out of range or move around it during the strike to make it miss. Its facing direction locks during the swing, and each attack can damage you only once. Killing it cancels any pending attack.

The starting attack settings are 10 damage, 2.5 units of range, a 0.6-second wind-up and 1 second of recovery. These can be adjusted on its Dummy Attacker component. Optional Console logs show whether an attack damaged, blocked, parried, dodged or missed.

## Weapons and rolling

The axe deals 10 damage and the sword deals 20. Both currently have a range of 2.5 units and a 0.5-second attack cooldown. Player attacks check straight ahead from the player, rather than aiming through the camera crosshair.

The equipped model is created under the player's WeaponPivot using the weapon asset's Weapon Prefab reference. The combat side still uses `SetEquippedWeapon(Weapon)` so inventory can select the weapon through that method. Inventory selection and unequipping interactions are being handled separately by James. New models should have their grip at the prefab origin and use a consistent orientation.

Rolling costs 25 stamina, lasts 0.45 seconds and has a 0.35-second cooldown afterward. Only the part from 0.08 to 0.28 seconds avoids incoming combat damage. You can also avoid an attack by rolling out of reach. Rolls require being grounded and standing, and cannot start during an attack or with the inventory open. Walls still block movement, and rolling off a ledge does not prevent fall damage.

## Taking damage and respawning

Both the player and dummies flash red when they take damage. Blocked, parried and dodged hits do not trigger the player's damage flash. Fall damage and starvation can also make the player flash.

At zero health, player actions stop and "You died" appears. After 3 seconds, the whole test scene reloads. This resets health, stamina, equipment to its scene setup, both dummies and dropped items; it does not preserve progress.

For respawn to work, add `CombatDefenceTest` to File > Build Settings > Scenes In Build and keep it enabled. The saved Build Settings may still point only at the archived movement scene, so check this before testing or building.

Health regenerates while hunger is above 90, so it can recover between enemy hits. For testing, P reduces hunger by 25% and F restores 25 hunger. You can also temporarily set Health Regen to 0 during Play mode to make damage easier to see.

## Things to check after changes

- Both weapon models appear and swing, with different damage values.
- Hits flash red without changing other objects' materials, and dummy death still drops food.
- Holding right click blocks repeated frontal hits without spending stamina; rear hits still hurt.
- A timed press shows "Parried", but continuing to hold does not keep parrying.
- Rolls cost stamina, respect walls and only dodge during their short protection window.
- Opening inventory, changing equipment or dying does not leave a stuck weapon pose.
- Killing the attacking dummy during preparation prevents its strike.
- Death during an attack or roll still resets cleanly. Test more than one respawn.
- The Console has no errors.

These are prototype animations and timings. The test scene still needs to be integrated with the team's main playable scene, and the death handler should be coordinated with the Journal/Map/Death work.
