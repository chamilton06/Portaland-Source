# Ranger crouch setup

This adds crouch idle/walk poses and stops the animated character being squashed by root
scaling. Code added 3 November 2026. Kartikeya confirmed that crouching now works in Play
mode. The controller has its crouch parameters/state, and the saved movement test scene
enables collider resizing. Detailed low-ceiling and regression checks remain to be recorded.

The first reported test still squashed the model. The saved scene hadn't enabled collider
resizing, the controller had no crouch parameters, and the Editor log showed missing-parameter
warnings from the driver. The setup now retries on returning to edit mode if compilation
happened during Play mode. The driver gives one setup warning instead of repeating missing
parameter messages every frame. Stop Play mode, let setup finish, and save the open test scene
before testing again. Kartikeya subsequently confirmed the correction works in Unity.

## Where it is

- Scene: `Assets/Scenes/Tests/RangerMovementTest.unity`.
- On **Player → Player Movement**, the new **Resize Controller For Crouch** option enables
  capsule resizing instead of scaling the whole Player. It defaults to off for other scenes.
- `Assets/Scripts/Movement/Playermovement.cs` handles collider height and the ceiling check.
- `Assets/Scripts/Animation/PlayerLocomotionAnimation.cs` reads the existing crouch state.
- `Assets/Animations/Player/RangerLocomotion.controller` gets a **Crouching** state.
- `Assets/Editor/PrepareRangerMovementTest.cs` adds the setup to the movement test.

## Running it

1. Stop Play mode and return to Unity so it can compile.
2. Look for `Ranger crouch setup ready`. If needed, run
   **Tools → Portaland → Prepare Ranger Movement Test**.
3. Open **Assets → Scenes → Tests → RangerMovementTest**.
4. If the scene was already open when the setup ran, save it to keep the new crouch option.
   The helper leaves unrelated unsaved scene edits for you to save yourself.
5. Press Play. Hold Ctrl while idle and moving. Release Ctrl to stand.

## How it works

The original root-scale crouch is still the default. The new option remembers the standing
CharacterController height and centre in Awake. Crouching reduces height and lowers the centre
by half the height difference, keeping the bottom fixed. Player scale, model scale, camera
and weapon pivot aren't scaled by this mode. The minimum height is twice the capsule radius.

The standing-space check uses the resized capsule's actual centre. Releasing Ctrl under a
ceiling keeps crouching; it also checks during the stand-up transition. Rolling waits until
the collider is back at its full standing height. Sprint and jump restrictions still use
the existing crouch state, stamina and input handling.

The Animator gets `Crouching` and `CrouchSpeed`. The latter is measured horizontal speed
divided by crouchSpeed. Its blend tree uses `Crouch_Idle_Loop` at 0 and `Crouch_Fwd_Loop`
at 1. Both clips loop and keep root motion off. Grounded crouching can follow landing, and
walking off an edge while crouched goes to Falling.

## Checks to record

- Hold Ctrl: knees bend, model proportions stay the same and feet stay on the floor.
- Move while crouched, then stop: crouch walk blends back to idle.
- Release Ctrl in open space: collider and pose return to standing.
- Release Ctrl under a low ceiling: no standing through it; move clear and stand again.
- Try Shift, Space and Q while crouched: existing restrictions still apply.
- Release crouch and press Q immediately: roll waits until the collider has finished growing.
- Walk off a small edge while crouched: falling and landing recover correctly.
- Inspect Player scale during Play mode: it should stay at its starting value.
- Open Prototype separately: its new option should remain off and old behaviour should remain.

Collider size and the animation's shape may need tuning after viewing low-ceiling clipping.
Crouch sideways/backwards movement still uses the forward clip. Directional locomotion,
combat animation work and full jump checks remain outstanding.

Basic crouch behaviour works according to Kartikeya's test. The individual checks above
haven't all been recorded as passed. This setup is saved in the local crouch checkpoint.
