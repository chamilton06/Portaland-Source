# Directional movement

Work started 4 November 2026. The blend-tree builder and driver support are written.
Unity compilation, Humanoid import, controller generation and test-scene assignment are
confirmed in an isolated Unity 2023.1.22f1 run. The results have been copied into this project.
The free Quaternius Standard pack has no
left/right/backwards walk clips. We have now downloaded KayKit's free animation pack from
the official page and copied its movement FBX and CC0 licence into the project. Controller
generation and assignment are now complete. Visual gameplay checks are still pending.

## Current test result

`RangerMovementTest` now uses `RangerDirectional.controller`; no manual assignment is needed.
Open that scene in Unity and test WASD/diagonals. If it was already open, reload the saved
scene to pick up the controller change, keeping any unsaved work you want first.

Both male and female prefabs were sampled in the generated controller for idle, forward,
backwards, left/right and two diagonals (30 frames each). Model roots stayed in place and
the sampled skinned vertices were finite and within a 10-unit sanity bound. These checks
don't prove that stride timing or clothing looks good. See [check results](directional-check-results.json).

The single command **Tools → Portaland → Prepare Directional Movement Test** can prepare
and assign the whole test setup later. It refuses to save an already-open dirty scene.
No setup command runs automatically on compilation.

## What we actually have

The imported `UAL1_Standard.fbx` has `Walk_Loop`, `Jog_Fwd_Loop`, `Sprint_Loop`,
`Crouch_Fwd_Loop` and `Crouch_Idle_Loop`. I checked its actual clip list. The
[full library page](https://quaternius.itch.io/universal-animation-library) advertises more
directions, but we shouldn't assume those are in the free Standard download.

The new `Assets/ThirdParty/KayKit/CharacterAnimations/Rig_Medium_MovementAdvanced.fbx`
contains `Running_Strafe_Left`, `Running_Strafe_Right` and `Walking_Backwards`. The included
licence says CC0. The running strafe clips may look faster than the forward walk, so compare
their pacing before calling directional movement complete. The archive's glTF equivalents
were inspected and have zero horizontal root/hips translation; the imported FBX still needs
a Unity drift test. Crouch left/right/backwards remain missing.

Source: [official KayKit animation page](https://kaylousberg.itch.io/kaykit-character-animations).
The [import manifest](kaykit-import-manifest.json) records the archive and copied-file hashes.

[Mixamo](https://www.mixamo.com/) is a possible free source. Search for walking backwards
and left/right strafing, preview them on the same character and use in-place exports when
available. Import and validate as Humanoid, enable Loop Time and preview on both Rangers.
Adobe's [FAQ](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html) confirms free access
with an Adobe ID and game use. Check raw-file sharing terms before committing downloads to
a public repository. Keep any new downloads outside Assets until their licence is checked.

## Where the setup lives

| File | Purpose |
| --- | --- |
| `Assets/Scripts/Animation/PlayerDirectionalClips.cs` | Stores the group's directional clip choices in a project asset |
| `Assets/Editor/RangerDirectionalSetup.cs` | Window for assigning clips and building a controller |
| `Assets/Scripts/Animation/PlayerLocomotionAnimation.cs` | Sends actual player-relative sideways/forward speed to the Animator |
| `Assets/Animations/Player/RangerDirectionalClips.asset` | Created by the window when we have the clips |
| `Assets/Animations/Player/RangerDirectional.controller` | Created only after the required clips validate |

## Building it when the clips are ready

For the supplied KayKit file, use **Tools → Portaland → Set Up Directional Animations →
Prepare free KayKit clips and build controller** after Unity finishes importing. It validates
the rig, configures the three loop clips, fills empty clip slots and builds a controller if
one doesn't exist yet. Then select **Player** in the movement test and click **Use controller
on selected Player**. Save the scene and run the checks below.

For other clips or later replacements, the manual setup is:

1. Stop Play mode. Open **Tools → Portaland → Set Up Directional Animations**.
2. Click **Create clip choices asset** and assign Walk Left, Walk Right and Walk Back.
3. Optional: assign all three crouch directions together. Otherwise the existing forward
   crouch tree stays in use, and directional crouching is still unfinished.
4. Click **Build directional controller**. Missing, non-Humanoid or non-looping clips stop
   the build with a message. An existing directional controller won't be overwritten.
5. In the movement test Hierarchy, select **Player** and click **Use controller on selected
   Player**. Save the scene. This assignment supports Undo.
6. Enter Play mode and test W, A, S, D, diagonals, starts/stops, walls and mouse turning.
   Repeat for the female visual before marking it done.

## How the blending works

The driver converts measured horizontal velocity into Player-local space. `MoveX` is sideways
and `MoveZ` is forward/backwards, divided by walkSpeed and clamped to a unit circle. A 2D
Cartesian blend tree places idle at the centre, left/right at the sides and backwards below.
The original idle/walk/jog/sprint tree sits at the forward point and still uses measured
`Speed` to select forward gait. Its existing speed thresholds need reviewing if movement
speeds change.

Diagonals blend the cardinal poses; they aren't separate diagonal clips. Optional crouch
directions use `CrouchX`/`CrouchZ`, normalised by crouchSpeed. The old controller still works
with the updated driver because directional parameters are only used when present.

The copied controller keeps jump, falling, landing and crouch transitions. The window assigns
only the Animator reference on the selected scene Player, so it doesn't depend on a scene name.
See the [guide for combat animations in scenes](guide-for-combat-animations-in-scenes.md) for moving the setup to another scene.
