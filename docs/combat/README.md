# Combat

This is the combat system. We're aiming for something that feels like Elden Ring: attacks cost
stamina, you have to read the enemy, and parrying or getting behind someone pays off.

Kartik (`kartikeyapro1`) looks after combat. The work lives on `feature/combat-animation-weapons`
and hasn't been merged into main yet.

If you want to add an enemy, skip ahead to [Adding enemies](adding-enemies.md).

## Trying it out

Open `Assets/Scenes/Tests/RangerMovementTest.unity` (or `RangerMovementTestFemale.unity` for the
female Ranger) and press Play. There's a Dark Ranger enemy about 14 metres in front of you and
the two training dummies off to the side.

Everything below is switched on in those two test scenes only. **Prototype hasn't changed**: the
new features all default to off, so the shared scene plays exactly as it did before. When we're
happy with it we'll turn things on in Prototype as a group.

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

## How a fight works

**Stamina.** Every attack and every blocked hit costs stamina, and it doesn't refill while you're
attacking or blocking. If you block with no stamina left your guard breaks: you take the hit and
stagger for a second.

**Parry and riposte.** Tap block within 0.2 seconds of a hit landing and you parry it. The
attacker is knocked off balance for 1.5 seconds. Hit them in that time for a riposte, which does
three times the damage.

**Backstab.** Hit an enemy from close behind while it isn't mid-swing and you get the same 3x
critical. Sneaking up on one, or rolling behind it after it whiffs, are the easy ways in.

**Poise.** Some attacks carry poise, which lets you keep swinging through hits instead of
flinching. The heavy attack can take 25 damage, the jump attack 15 and the weapon art 30. Light
attacks have none, so getting hit interrupts them. Enemies have poise too: hit the Dark Ranger
for 45 damage within a few seconds and it staggers, open to a riposte.

**Lock-on.** Locking on keeps you facing the target, so A and D circle around it and the camera
keeps it in view. Rolling and sprinting still go wherever you point.

**Health bars.** An enemy's health bar appears over its head once you hit it or lock on, and
fades a few seconds after. The yellow part is the damage you just did, and the number next to it
adds up your combo.

## The Dark Ranger

The Dark Ranger is our first proper enemy, styled after the invaders in the Souls games. It runs
at you when you get within 12 metres. At about 3.5 metres it circles you for a second or two,
then commits to an attack.

| Attack | Damage | Tip |
| --- | --- | --- |
| Quick slash | 12 | Fast. Half the time it carries straight on into a backhand |
| Backhand slash | 12 | Also used on its own |
| Overhead smash | 22 | Slow and lunges in. Roll to the side once it stops turning |
| Delayed slash | 18 | Holds its sword up for a moment. Rolling on reflex gets you hit |

After every attack it stands still for a moment, which is your chance to punish it. It flinches
if you hit it between attacks but swings through your hits while attacking. It has 250 health.

## Where the code is

Combat scripts are in `Assets/Scripts/Combat`: player scripts in `Player/`, enemy scripts in
`Enemies/`, and the shared ones at the top. The animation side is in `Assets/Scripts/Animation`.

| Script | What it does |
| --- | --- |
| `PlayerCombat` | Picks and times the player's attacks, deals damage, ripostes, backstabs |
| `PlayerDefence` | Blocking, parrying, guard breaks and getting hit |
| `PlayerLockOn` | Lock-on and the lock-on dot |
| `EnemyHealthBars` | The health bars over enemies. Goes on the Player |
| `HandAttack` | The settings for one player attack (timing, damage, stamina, poise) |
| `EnemyMelee` | The Dark Ranger's AI. Works for any sword-fighting humanoid |
| `EnemyAttack` | The settings for one enemy attack |
| `Health` | Hit points for anything that can be damaged |
| `IStaggerable`, `IBackstabbable`, `IHitReceiver` | Small interfaces so the player's hits can talk to any enemy |
| `DummyAttacker` | The stationary training dummy that swings at you |
| `PlayerCombatAnimation` | Plays the attack, guard, parry, hit and death animations |
| `WeaponHandSocket`, `FistGrip` | Put the sword in the character's hand |

All the attack animations come from the free Quaternius Universal Animation Library and play on
a "Combat" layer of `Assets/Animations/Player/RangerDirectional.controller`. That layer only
covers the upper body, so your legs keep walking while you swing. A separate full-body "Death"
layer handles dying and the backstab stumble.

## The setup tool

**Tools → Portaland → Prepare Combat Animations** builds all of this. It adds the animation
states, fills in the attacks on the test scene Players, builds the Dark Ranger prefab and places
one in each test scene.

You don't need to run it after pulling, because everything it makes is already committed. Run
it if you've reset something or after we change the tool. Stop Play mode first.

It only ever fills in things that are missing or still zero, so anything you've tuned in the
Inspector is left alone. To get the defaults back, clear the field (or delete
`Assets/Prefabs/Combat/DarkRanger.prefab`) and run it again.

## Tuning

Almost every number is on a component in the Inspector, with a tooltip explaining it. The
useful ones:

- **Player → Player Combat:** each attack's damage, timing, stamina cost and poise. Also the
  riposte and backstab multipliers.
- **Player → Player Defence:** parry window, block stamina cost, how long a hit stuns you.
- **Player → Player Lock On:** lock range, how hard you flick to switch, dot size.
- **DarkRanger → Enemy Melee:** speeds, circling distance, poise, stagger length and the moveset.

If you change something in Play mode, Unity throws it away when you stop. Write the numbers
down and put them back in afterwards.

## Still to do

- A boss enemy, once Cole has built the combat environment.
- Turning all of this on in Prototype.
- Riposte and backstab animations for the player. At the moment they reuse the normal attacks.
