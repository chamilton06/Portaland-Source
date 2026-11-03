# First playable Ranger test

Added 3 November 2026. This connects ground locomotion to the existing controller in a
separate scene. Unity compilation and scene/controller generation are confirmed by the
Editor log and generated files. Kartikeya reports that the Ranger movement test works.
This is a ground-movement checkpoint; it doesn't mean all character animations are finished.

## Remaining work

- Finish and check jumping, falling and landing.
- Add crouch poses and address the current root-scale squash.
- Add sideways and backwards locomotion animations.
- Integrate animations for existing combat, including weapon/hand alignment, attacks and
  guarding. New combat mechanics aren't implied by this list.
- Check rolling and repeat gameplay tests on the female character.

## Where things are

| Location | What it does |
| --- | --- |
| `Assets/Editor/PrepareRangerMovementTest.cs` | Copies the saved Prototype and sets up the character in the copy |
| `Assets/Scripts/Animation/PlayerLocomotionAnimation.cs` | Reads CharacterController velocity and sends horizontal speed to the Animator |
| `Assets/Scenes/Tests/RangerMovementTest.unity` | Generated gameplay test scene |
| `Assets/Animations/Player/RangerLocomotion.controller` | Generated copy of the preview controller, with movement-speed thresholds |
| Hierarchy: `Player → CharacterVisual → PlayerRangerMale` | Generated character placement in the test scene |

The last two asset files and the Hierarchy setup are created when the Editor helper runs.
The helper tries once after compilation. You can also use
**Tools → Portaland → Prepare Ranger Movement Test** outside Play mode.
If the test scene already exists, it keeps your saved changes and doesn't rebuild it.

## Running it

1. Return to Unity and wait for compilation. Check for `Ranger movement test ready` in
   the Console. If there's an error, read it before continuing.
2. Save any scene work you want to keep, then open
   **Assets → Scenes → Tests → RangerMovementTest**.
3. Press Play and click the Game view. Close inventory with Tab if needed.
4. Use WASD and the mouse. Hold Shift while moving forwards to sprint.
5. Check starting, stopping and pushing against a wall. Speed should drop when blocked.
6. Stop Play mode when finished; record any animation or gameplay problems.

## What changed in the copy

The original Player root, collider, movement script, turning, stats, defence and combat
components stay on the same object. Existing UI, equipment and camera references are kept.
Only the capsule's MeshRenderer is hidden; its CharacterController stays enabled.
The three named standalone model previews are removed from the copied scene.

CharacterVisual sits at the controller's centre. The character is fitted to its height and
offset so the feet match the capsule bottom. The movement script's Roll Visual points to
CharacterVisual and its capsule visual fallback is disabled. This keeps the existing visual
roll without rotating the Player root or its weapon pivot.

The new driver reads actual horizontal CharacterController velocity after PlayerMovement's
Update. It doesn't read input or move the player. The copied blend tree uses 0 for idle,
walkSpeed for walk, the midpoint between walkSpeed and sprintSpeed for jog, and sprintSpeed
for sprint. The current scene values are 5 and 9. Changing movement speeds later also means
reviewing these controller thresholds. Root motion stays off.

Death or disabled movement freezes the Animator so the character doesn't keep running.
There is no death animation yet. The preview number-key script isn't added to this scene,
so hotbar keys keep their gameplay purpose.

## Still to do

- **Play mode checks:** setup generation alone doesn't prove gameplay works.
- **Foot sliding:** actual movement speeds may not match each clip's stride; tune after viewing.
- **Direction:** sideways and backwards movement still uses forward locomotion clips.
- **Airborne:** the next update adds jump/fall/landing states. See
  [airborne animation](airborne-animation.md); generation is confirmed, but jump quality
  and gameplay checks still need completing.
- **Crouch:** the existing controller scales the Player root. The Ranger will squash too;
  a proper crouch pose and compatible collider change are still needed.
- **Roll:** the wrapper still makes a full visual rotation, rather than using a roll clip.
  Check floor clipping and return to the resting pose.
- **Weapons:** they stay on the existing WeaponPivot. Check hand alignment and guard/swing
  clipping; hand-bone attachment is a separate step.
- **Respawn:** the copied scene isn't automatically added to Build Settings. Before testing
  death/reload, add `RangerMovementTest` under **File → Build Settings → Scenes In Build**.
- **Female model:** the first gameplay scene uses the male prefab. The female has only been
  tested in the separate animation preview so far; repeat gameplay fitting/testing later.

Prototype, the movement script and shared character prefabs aren't edited by this setup.
This setup is saved as a local movement-integration checkpoint before airborne work.
