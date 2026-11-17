# The vampire boss

The first boss of the game: the **Servant of Dracula**, in the church in Main Game. He's meant
to be like Soldier of Godrick in Elden Ring: a first boss that teaches you to roll, punish and
parry, and that's hard but beatable in a few tries.

Kartik makes the bosses, so this page is mainly so everyone knows how it works and what to
leave alone.

## How the fight goes

1. You walk into the church. He stands still until you're within 10 metres of him on his
   floor. Hitting him first also starts the fight.
2. His name and a big health bar appear at the bottom of the screen, the boss music starts,
   and **red mist seals both church doors**.
3. He circles, attacks, and you punish the gaps. Below half health he enters a second phase:
   a short roar, his claws flare up, and he gets a bit faster.
4. When he dies, "VANQUISHED" appears with the victory sting, the mist fades and the doors
   open, and a **Blood Vial** goes straight into your inventory with a little popup.
5. If you die instead, the scene reloads and everything resets.

## His moves

| Move | Damage | How to deal with it |
| --- | --- | --- |
| Claw swipe | 17 | Fast. Often carries straight on into the backhand |
| Backhand claw | 17 | Also used on its own |
| Overhead rend | 31 | Slow and lunges in. Roll sideways once he stops turning |
| Delayed rake | 24 | Holds his claws back. Rolling on reflex gets you hit |
| Blood lunge | 26 | Rushes you from up to 9 m away, so backing off to heal isn't always safe |

Damage already includes his boss multiplier (x1.2). He doesn't flinch when you hit him. If you
land 3 hits within 2.5 seconds, he cuts into your combo with a swipe. Hit two or three times,
then back off.

**Parrying him is the big reward.** He's staggered for 2.2 seconds and a riposte does 4.5x
damage.

## Where things are in the scene

In Main Game's Hierarchy:

- **Vampire (Boss)**: the boss. Its **Boss Encounter** component has all the boss settings.
- **Boss Barrier** and **Boss Barrier (1)**: the red mist in the two doorways. Each one is an
  **Arena Barrier** with a Box Collider, which is the actual wall. They're hidden until the
  fight starts.

The vampire itself is `Assets/Prefabs/Combat/Vampire.prefab`. The boss settings are only on
the copy in the church, so the prefab can be reused as a normal vampire later.

## Changing the fight

Everything is on **Vampire (Boss) → Boss Encounter**:

| Setting | What it does |
| --- | --- |
| Boss Name | The name above the bar |
| Wake Range / Same Floor Height | How close you get before the fight starts |
| Damage Multiplier | His damage (1.2) |
| Circle Time Scale / Recovery Scale | How long he waits between attacks, and how long he's open after one |
| Poise / Poise Break Cooldown | How much damage staggers him (100), and how long before it can happen again (8 s) |
| Retaliate After Hits | How many hits in a row before he cuts into your combo (3) |
| Barriers | The doorway barriers it turns on and off |
| Music / Victory Sound | Leave Victory Sound empty to use the game's shared one |
| Reward / Reward Amount | What you get for beating him (1 Blood Vial) |

His health is on **Health** (350), and his moves, second phase and riposte bonus are on
**Enemy Melee**.

## The barriers

To add another doorway: make an empty GameObject, add **Arena Barrier** (it brings a Box
Collider), and size the box to fill the doorway. Something like X 2.4, Y 3.2, Z 0.3, with
Center Y at half the height so it sits on the floor. Then add it to the boss's **Barriers**
list. **Opacity**, **Brightness** and **Swirl Speed** change how it looks.

## Where his model and sounds came from

- Model: "Vampire Rigged 3D" by Zain.2407 on Sketchfab, **CC BY 4.0**, so he needs to be
  in the game's credits. Details are in `Assets/ThirdParty/Sketchfab/Vampire/README.md`.
- Sounds and music: all CC0. Details are in `Assets/ThirdParty/Audio/README.md`.
