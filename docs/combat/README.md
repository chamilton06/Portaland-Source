# Combat

This is the combat for Portaland. The idea is for it to feel like Elden Ring or Dark Souls, so
attacks cost stamina, you have to learn what enemies do, and parrying or getting behind
someone is worth it. I (Kartik) look after combat and I'm making the bosses. It's all on the
`feature/combat-animation-weapons` branch, in PR #37.

Other pages in here:

- [boss-fight.md](boss-fight.md) explains the vampire boss in the church
- [notes-for-cole-and-uliana.md](notes-for-cole-and-uliana.md) lists what I changed in your parts
- [adding-enemies.md](adding-enemies.md) covers putting enemies in a level

## Trying it

Open `Assets/Scenes/Main Game.unity`, press Play and walk into the church. The vampire boss is
inside. For quicker testing there's also `Assets/Scenes/Archives/Tests/RangerMovementTest.unity`,
which has a Dark Ranger enemy in front of you and two training dummies.

Prototype doesn't have the new combat. The only thing that changed for the whole game is
stamina.

## Controls

| Input | Does |
| --- | --- |
| Left click | Light attack (keep tapping for a combo) |
| Hold left click | Heavy attack |
| Left click while jumping, sprinting or crouching | Jump, sprint or crouch attack |
| G | Weapon art, a three-hit combo that uses a lot of stamina |
| Hold right click | Block |
| Tap right click just before a hit | Parry |
| Q | Roll |
| Middle click or C | Lock on (flick the mouse to switch target) |
| E / R | Test keys that give you the axe / sword |

## How it works

Everything you do uses stamina: attacking, rolling and blocking hits. It comes back at 40 a
second after a short pause, but not while you're attacking or blocking, so you can't just hold
block forever. A roll costs 18 and a light attack 10. If you block with no stamina left your
guard breaks and you get hit.

If you parry, the enemy gets knocked off balance for a moment. Hitting them then is a riposte,
which does 3x damage (more on bosses) and freezes the game for a split second so it feels
heavy. Hitting an enemy from behind when it isn't attacking is a backstab and does the same.

Your heavy attack, jump attack and weapon art have poise, which means you keep swinging if you
get hit during them. Enemies have poise too. If you do enough damage to one quickly it
staggers and you can riposte it.

Lock-on keeps you facing the enemy so you can circle it with A and D. A small health bar shows
over enemies when you hit them or lock on. Bosses get a big bar at the bottom of the screen.

When you die you get a YOU DIED screen like in Dark Souls and the scene reloads after 5
seconds, which also resets the enemies.

## Sounds

There are sounds for swings, hits, blocks, parries, rolls, the vampire, the boss music and the
victory and death screens. They're all CC0 so we're allowed to use them, and the credits are in
`Assets/ThirdParty/Audio/README.md`.

Most of them are set in `Assets/Audio/CombatSounds.asset`. If you want to change one, drag a
different clip into the list. A scene only has these sounds if the Player has the Combat Audio
component (Main Game and the test scenes do, Prototype doesn't).

## Code

All of it is in `Assets/Scripts/Combat`. Player stuff is in `Player/`, enemies in `Enemies/`,
the boss fight in `Boss/` and the sounds in `Audio/`. Things like `Health` and `DamageFlash`
are at the top because both sides use them. The animation scripts are in
`Assets/Scripts/Animation`.

The attack animations are on a "Combat" layer in
`Assets/Animations/Player/RangerDirectional.controller`. That layer only moves the upper body,
so your legs still walk while you attack.

## Tools

If you make a new scene and want the player to have all of this, open the scene and use
**Tools → Portaland → Add Combat Player To Open Scene**, then save. The player needs to already
have the Ranger character on it. The other options in that menu were for building the character
at the start and you shouldn't need them.

## Changing numbers

Pretty much every number has a tooltip in the Inspector. Attacks are on Player Combat, blocking
and parrying on Player Defence, stamina on Player Stats and Player Movement, enemies on Enemy
Melee, and the boss on Boss Encounter. Anything you change in Play mode resets when you stop,
so write it down first.

## Still to do

- The vampire sometimes slides along instead of walking when he chases you. I'm looking at it.
- Proper riposte and backstab animations for the player (they use the normal attacks for now).
