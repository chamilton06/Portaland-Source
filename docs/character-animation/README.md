# Character appearance and animation

Issue: [#23](https://github.com/chamilton06/Portaland-Source/issues/23)
Owner: Kartikeya (`kartikeyapro1`)
Branch: `feature/player-character-animation-23`

## Where we're up to

Repository inspection and initial asset research are done. As of 2 November 2026, we're trying
Quaternius's free Standard packs for a male and female character. The Unity FBXs, textures
and original licences are now in `Assets/ThirdParty/Quaternius`; the in-place Standard
animation file is included. No gameplay or scene changes have been made. Editor import,
head/outfit assembly, materials and rig checks still need doing.

These notes record what was found on 1 November 2026 at local commit `c37c85c`. They describe the files on disk, not a completed Play mode test. Remote changes have not been fetched.

The aim is to replace the visible capsule and animate the movements we already have. New combat mechanics and new movement abilities are outside this issue.

The updated visual reference is Elden Ring: darker medieval fantasy, with a more naturally
proportioned adventurer. The [art direction notes](art-direction.md) list the group's possible
assets and how we'll compare them. KayKit is no longer our first character recommendation.

## Finding things in Unity

For assembling the head and outfit, see [character preparation](character-preparation.md).
For the separate idle/walk/jog/sprint preview, see [animation testing](animation-test.md).
For the first playable copy of Prototype, see [movement integration](movement-integration.md).
For the next jump/fall/landing setup, see [airborne animation](airborne-animation.md).
For animated crouching and the optional collider change, see [crouch setup](crouch-animation.md).
For the directional blend setup and missing clips, see [directional movement](directional-movement.md).
For using the characters in other scenes and merging, see [group handover](group-handover.md).

1. Open the project with **Unity 2023.1.22f1**.
2. In the **Project** window, go to **Assets → Scenes** and double-click **Prototype**.
3. In the **Hierarchy**, select the top-level **Player** object.
4. In the **Inspector**, look for its Character Controller, Mesh Renderer and gameplay scripts.
5. Select **Main Camera** to find the Camera Follow component. Expand **Player** to find **WeaponPivot**.

The scripts' file names have different capitalisation from their class names. For example, `Playermovement.cs` contains `PlayerMovement`. Use the exact file names below when searching.

| File | What it's for |
| --- | --- |
| [`Assets/Scenes/Prototype.unity`](../../Assets/Scenes/Prototype.unity) | Scene containing the player we inspected |
| [`Playermovement.cs`](../../Assets/Scripts/Movement/Playermovement.cs) | Movement, gravity, crouching, sprinting, jumping and rolling |
| [`Playerturn.cs`](../../Assets/Scripts/Movement/Playerturn.cs) | Mouse turns the player root left/right |
| [`Camerafollow.cs`](../../Assets/Scripts/Movement/Camerafollow.cs) | Third-person camera position and pitch |
| [`Playerstats.cs`](../../Assets/Scripts/Movement/Playerstats.cs) | Hunger, stamina, health and fall damage |
| [`PlayerCombat.cs`](../../Assets/Scripts/Combat/PlayerCombat.cs) | Equipping, weapon swings and guard pose |
| [`PlayerDefence.cs`](../../Assets/Scripts/Combat/PlayerDefence.cs) | Blocking, parrying and dodge checks |
| [`DamageFlash.cs`](../../Assets/Scripts/Combat/DamageFlash.cs) | Changes renderer colours briefly when hurt |
| [`PlayerDeathHandler.cs`](../../Assets/Scripts/Combat/PlayerDeathHandler.cs) | Disables controls and reloads the scene after death |
| [`Inventory.cs`](../../Assets/Scripts/Inventory/Inventory.cs) | Inventory UI and equipping through PlayerCombat |

## Current player setup

`Player` is stored directly in the Prototype scene. No separate player prefab was found.
Its root contains the CharacterController, capsule MeshFilter/MeshRenderer, PlayerMovement,
PlayerTurn, PlayerStats, PlayerCombat, PlayerDefence, DamageFlash and PlayerDeathHandler.
Keep this object and its references when adding the model.

The CharacterController is 2 units tall, with radius 0.5 and a centred origin.
A character whose origin is at its feet will need an offset underneath the player root;
we should work out the exact value from the imported model.

PlayerMovement moves relative to the player's facing direction. It uses CharacterController.Move,
not a Rigidbody. PlayerTurn rotates the root, and Main Camera follows it.

| Input | Existing behaviour |
| --- | --- |
| WASD | Movement, including strafing and backwards movement |
| Left Shift | Sprint forwards if stamina allows |
| Space | Jump; sprinting changes the jump height and distance |
| Left Ctrl | Crouch, with a ceiling check before standing |
| Q | Roll, spending stamina and using a cooldown |

Movement exposes `IsGrounded`, `IsSprinting`, `IsCrouching` and `IsRolling`.
Vertical velocity is private. We may need a small read-only addition for airborne animation,
but we should decide that when implementing the animation driver.

No Animator or legacy Animation components were found in the searched scenes/prefabs.
No `.controller`, `.anim` or FBX files were found under Assets. There are already visual
effects made with code: the roll rotates a visual child, and combat rotates WeaponPivot.

## Things to watch during integration

- **Crouch:** the code scales the whole Player root vertically. A child model would squash too. We need to agree a small compatible fix before adding a proper crouch pose.
- **Roll:** Start creates a RollVisual child from the capsule mesh and hides the original renderer. Disable this capsule fallback when adding the character. Decide whether to keep a temporary visual roll or use a clip later; avoid rotating the same object with both systems.
- **Weapons:** PlayerCombat changes WeaponPivot's local pose for swings and guarding. Keep the existing reference during the first model test. Attaching it to a hand bone needs its own check for clipping and competing pose changes.
- **Directions:** an idle/walk/run blend tree alone won't cover strafing and backwards movement properly. Choose directional clips or agree the visual compromise before calling locomotion complete.
- **Damage and death:** DamageFlash already searches child renderers, but the imported materials still need testing. Death disables movement; animation must stop or switch state too.
- **Respawn:** Prototype is missing from enabled Build Settings. Only archived MovementTesting is enabled. PlayerDeathHandler reloads the current scene, so Prototype respawn needs this fixed and tested.

## Planned implementation

1. Run the [baseline checks](testing.md) in Prototype before changing gameplay.
2. Choose a character from the [shortlist](asset-selection.md). Check its actual licence and rig before importing it into the shared project.
3. Add the model as a child of Player. Hide only the capsule renderer; keep the controller, scripts, camera and equipment references.
4. Check model scale, feet position, forward direction and materials. Validate the Humanoid Avatar in the Inspector's Rig tab.
5. Import in-place movement clips. Keep **Apply Root Motion off** so PlayerMovement stays responsible for movement.
6. Create locomotion and airborne states, then a small animation driver that reads the existing controller state. Avoid a second movement system.
7. Repeat the test checklist and record what passed, what failed and what still needs work.

## Folder plan

These asset folders are **proposed, not created yet**. We'll only add the ones we actually use.

| Path | Intended contents |
| --- | --- |
| `docs/character-animation/` | Setup instructions, asset decisions and test notes |
| `Assets/Art/Characters/Player/` | Our player-specific visual assets, materials and prefab |
| `Assets/ThirdParty/<PackName>/` | Imported supplier files and their licence, keeping the pack together |
| `Assets/Animations/Player/` | Our Animator Controller and any clips we create |
| `Assets/Scripts/Animation/` | Animation driver code |

For each implementation step, update these notes with the real file path, Hierarchy object,
Inspector fields and a short way to test it. Comments should explain a choice that isn't obvious
from the code. Keep them brief and in the same plain style as the rest of the project.

## Changes so far

- Created the feature branch and added these group notes, asset shortlist and test checklist.
- Added a root README so the group can find the notes from the repository homepage.
- Left the existing local edits to `.DS_Store` and the combat README alone.
- Copied the Quaternius Unity source assets and checked their CC0 licences. See
  [the asset folder guide](../../Assets/ThirdParty/Quaternius/README.md) and
  [copy manifest](import-manifest.json). Unity Editor import checks are pending.
- Prepared an initial local asset/documentation commit, including Unity-generated `.meta`
  files. No pushes or scene changes made.
- Kartikeya completed the Ranger rig setup steps, but the pre-commit disk check still shows
  Generic settings on both Ranger FBXs. Recheck Apply/Configure in Unity next; Humanoid
  validation isn't confirmed by the saved files yet.
