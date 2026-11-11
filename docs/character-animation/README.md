# Character and animation

This covers the player's look and how they move: the Ranger models, their animations, and the
scripts that keep the animations in step with gameplay. Combat has its own notes in
[`docs/combat`](../combat/README.md).

Kartik (`kartikeyapro1`) did this work, for issue
[#23](https://github.com/chamilton06/Portaland-Source/issues/23). It's on the
`feature/combat-animation-weapons` branch and hasn't been merged into main yet.

## What we have

The capsule has been replaced with a Ranger character, in male and female versions, going for
a darker, Elden Ring-style medieval look. Everything the player could already do is now
animated:

- Idle, walk, jog and sprint, plus strafing and walking backwards
- Crouching and crouch-walking (the crouch walk only has a forward animation so far)
- Jumping, falling and landing
- Rolling in whichever direction you're moving
- Sword attacks, blocking, parrying, getting hit and dying (see the combat notes)

The sword sits in the character's right hand and follows every animation.

To see it, open `Assets/Scenes/Tests/RangerMovementTest.unity` (male) or
`RangerMovementTestFemale.unity` (female) and press Play. Prototype still has the old capsule.
We'll bring the character into it as a group.

## Where things are

| Folder | What's in it |
| --- | --- |
| `Assets/Art/Characters/Player` | The male and female Ranger prefabs, `PlayerRangerMale` and `PlayerRangerFemale` |
| `Assets/Animations/Player` | `RangerDirectional.controller`, which the test scenes use, and the upper-body mask for attacks |
| `Assets/Scripts/Animation` | Scripts that drive the Animator from the gameplay scripts |
| `Assets/Editor` | Setup tools, all under **Tools → Portaland** |
| `Assets/ThirdParty` | The downloaded models and animations, with their licences |
| `Assets/Scenes/Tests` | The test scenes |

The models and most animations are from Quaternius, and the sideways and backwards walks are
from KayKit. Both are CC0, so we can use them freely. Details are in
[`Assets/ThirdParty/Quaternius/README.md`](../../Assets/ThirdParty/Quaternius/README.md).

## How it fits together

The gameplay scripts are still in charge. `PlayerMovement` moves the player, and the animation
scripts only watch what it's doing and play the right clip. Root motion is off, so an animation
can never move the player on its own.

- `PlayerLocomotionAnimation` reads speed, direction, crouching, jumping and rolling from
  `PlayerMovement` and passes them to the Animator.
- `PlayerCombatAnimation` does the same for attacks, blocking and getting hit.
- `WeaponHandSocket` moves the weapon into the right hand. It works out the grip from the hand's
  knuckle bones, so it fits any Humanoid model.

The character model sits under a child called `CharacterVisual` on the Player. During a roll
only that child turns, so the camera and controls stay steady.

## Setup tools

These are under **Tools → Portaland**. They built the test scenes and assets, and everything
they made is committed, so you don't need to run them after pulling.

| Tool | What it does |
| --- | --- |
| Prepare Ranger Characters | Builds the male and female Ranger prefabs |
| Prepare Ranger Animation Test | Builds a scene for previewing animations on their own |
| Prepare Ranger Movement Test | Builds the playable test scenes from a copy of the Prototype player |
| Set Up Directional Animations | Builds the directional controller from the chosen strafe and backwards clips |
| Prepare Directional Movement Test | Switches the male test scene over to the directional controller |
| Prepare Ranger Roll Tests | Adds the roll animation and the female test scene |
| Prepare Weapon Hand Test | Puts the weapon in the hand in the test scenes |
| Prepare Combat Animations | Adds all the combat animations, settings and the Dark Ranger enemy |

## Build notes

These were written while building each part. They go into more detail, but some of the
"still to do" sections are out of date now.

- [Art direction](art-direction.md) and [asset selection](asset-selection.md): how we picked the look and the assets
- [Character preparation](character-preparation.md): assembling the head and outfit
- [Animation test](animation-test.md): the standalone animation preview
- [Movement integration](movement-integration.md): getting the character onto the gameplay Player
- [Airborne](airborne-animation.md), [crouch](crouch-animation.md), [directional movement](directional-movement.md) and [roll](roll-and-female-test.md)
- [Guide for combat animations in scenes](guide-for-combat-animations-in-scenes.md): how to put the character and its combat animations into another scene, and what to watch out for when merging
