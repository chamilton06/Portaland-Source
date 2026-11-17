# Combat

This is our combat system. We're going for something that feels like Elden Ring: attacks cost
stamina, you have to read the enemy, and parrying or getting behind someone pays off.

Kartik (`kartikeyapro1`) looks after combat and makes the bosses. The work is on the
`feature/combat-animation-weapons` branch (PR #37).

**Other pages:**

- [The vampire boss](boss-fight.md): how the church fight works and how to change it
- [Notes for Cole and Uliana](notes-for-cole-and-uliana.md): what changed in your bits, and how to hook into it
- [Adding enemies](adding-enemies.md): putting enemies in a level, or making new ones

## Trying it out

- **The real thing:** open `Assets/Scenes/Main Game.unity`, press Play and walk into the
  church. The vampire boss is waiting inside.
- **Quick testing:** open `Assets/Scenes/Archives/Tests/RangerMovementTest.unity` (or
  `RangerMovementTestFemale.unity`). There's a Dark Ranger about 14 metres in front of you and
  two training dummies off to the side.

Prototype hasn't been given the new combat, so it plays like it used to. The one thing that
changed everywhere is stamina (see below).

## Controls

| Input | What it does |
| --- | --- |
| Left click (tap) | Light attack. Keep tapping to chain them |
| Left click (hold) | Heavy attack: slower, twice the damage, more stamina |
| Left click in the air / sprinting / crouched | Jump attack, lunging sprint attack, crouch attack |
| G | Weapon art: a three-hit flurry that costs a lot of stamina |
| Right click (hold) | Block |
| Right click (tap just before a hit) | Parry |
| Q | Roll. You can't be hit during part of it |
| Middle click or C | Lock on, or let go. Flick the mouse sideways to switch target |
| E / R | Give yourself the axe / sword (test keys from the inventory) |

## How a fight works

**Stamina.** Every attack, roll and blocked hit costs stamina. It refills at 40 a second after
a 0.6 second pause, but not while you're attacking or blocking. A roll costs 18, a light attack
10 and a heavy 25. If you block with no stamina left, your guard breaks.

**Parry and riposte.** Tap block within 0.2 seconds of a hit landing and you parry it. The
attacker is knocked off balance. Hit them while they're staggered for a riposte: three times
the damage (more on bosses). The game freezes for a split second when it lands.

**Backstab.** Hit an enemy from close behind while it isn't mid-swing for the same critical.

**Poise.** Some of your attacks carry poise, so you can swing through hits instead of
flinching: the heavy attack 25, the jump attack 15 and the weapon art 30. Enemies have poise
too. Do enough damage to one quickly and it staggers, open to a riposte.

**Lock-on.** Keeps you facing the target, so A and D circle around it. Rolling and sprinting
still go wherever you point.

**Health bars.** A small bar appears over an enemy once you hit it or lock on. Bosses get a big
bar at the bottom of the screen instead.

**Dying.** You get a Souls-style YOU DIED screen, and the scene reloads after 5 seconds. That
resets everything, including enemies and bosses.

## Sounds

Swings, hits, blocks, parries (a bell), rolls, enemy voices, boss music, and the victory and
death stings. All of it is CC0, so we can use it freely. Credits are in
`Assets/ThirdParty/Audio/README.md`.

Most sounds are picked in **`Assets/Audio/CombatSounds.asset`**. To swap one, drag a different
clip into its list. The victory and death stings are in there too, so every boss and every
death sounds the same across the game.

A scene only plays these sounds if its Player has the **Combat Audio** component. Main Game and
the test scenes have it. Prototype doesn't.

## Where the code is

Everything is in `Assets/Scripts/Combat`:

| Folder | What's in it |
| --- | --- |
| `Player/` | `PlayerCombat` (attacks, ripostes, backstabs), `PlayerDefence` (block, parry, getting hit), `PlayerLockOn`, `EnemyHealthBars`, `PlayerDeathHandler` (YOU DIED), `HandAttack` |
| `Enemies/` | `EnemyMelee` (the AI for the Dark Ranger and vampire), `EnemyAttack` (one move), `BloodClaws` (the vampire's glowing claws), `DummyAttacker` |
| `Boss/` | `BossEncounter` (turns an enemy into a boss fight) and `ArenaBarrier` (the red mist that seals the doors) |
| `Audio/` | `CombatAudio`, `CombatSoundBank` and `EnemyVoice` |
| top level | `Health`, `DamageFlash`, `HeldWeaponModel`, and the small interfaces enemies use |

The animation side is in `Assets/Scripts/Animation`. The attack animations play on the
"Combat" layer of `Assets/Animations/Player/RangerDirectional.controller`, which only covers
the upper body, so your legs keep walking while you swing.

## The one tool you might need

**Tools → Portaland → Add Combat Player To Open Scene** gives the Player in whatever scene you
have open everything above: the sword in the hand, the combat animations, lock-on, health bars
and sounds. The Player needs the Ranger character already (Main Game's does). Save the scene
afterwards.

The other Tools → Portaland items built the character and animations at the start. Everything
they made is committed, so you don't need them.

## Tuning

Almost every number has a tooltip in the Inspector. The useful places:

- **Player → Player Combat:** each attack's damage, timing, stamina and poise
- **Player → Player Defence:** parry window, block cost, how long a hit stuns you
- **Player → Player Stats / Player Movement:** stamina, refill speed, roll cost
- **An enemy → Enemy Melee:** speed, poise, moveset
- **The boss → Boss Encounter:** everything about the boss fight (see [boss-fight.md](boss-fight.md))

Changes made in Play mode are thrown away when you stop, so write the numbers down first.

## Known issues and still to do

- The vampire sometimes slides around instead of walking while he chases you. Being looked into.
- Riposte and backstab animations for the player. At the moment they reuse the normal attacks.
- Turning the new combat on in Prototype, if we still need Prototype.
