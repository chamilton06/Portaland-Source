# Roll animation and female gameplay test

Added 4 November 2026. Setup and scripted Animator checks passed in Unity 2023.1.22f1.
The generated files have been copied back into the project. Visual/gameplay checks remain.

## Open these scenes

- Male: **Assets → Scenes → Tests → RangerMovementTest**.
- Female: **Assets → Scenes → Tests → RangerMovementTestFemale**.

Let Unity refresh and reopen the saved scene if it was already open. Both scenes use
`Assets/Animations/Player/RangerDirectional.controller`. The female scene keeps the same
gameplay Player and references, with its own fitted female character visual and Avatar.

Press Play and test WASD, diagonals, Shift, Space, Ctrl and Q. Test rolls forwards, sideways
and backwards, and off a small edge. Check the hood, shoulders, feet and clothes as well as
movement/stamina. Try turning the mouse during a roll and check the visual returns upright.

## What changed

- **PlayerMovement** exposes its existing roll direction through a read-only property.
  Roll movement, stamina cost, cooldown and dodge window aren't changed by this step.
- **PlayerLocomotionAnimation** reads `IsRolling` and sets `Rolling` and `RollRate`.
  The roll state's base speed is the clip length, and RollRate is `1 / rollDuration`.
  This makes its normalised duration match the gameplay roll (currently 0.45 seconds).
- The supplied Quaternius `Roll` clip is non-looping, with root translation/rotation baked
  into the pose and Apply Root Motion off on the Animator.
- The Animator can enter Rolling from another state. It stays there while IsRolling is true,
  then returns to locomotion/crouch on the floor or Falling off an edge.
- In both test scenes, **Player Movement → Roll Visual** is now empty and **Create Capsule
  Roll Visual** is off. This disables the old whole-model spin.
- **Player Locomotion Animation → Roll Facing** points to **Player → CharacterVisual**.
  The driver rotates this wrapper only around its upright facing direction for directional
  rolls, then turns it back towards Player. The animation performs the tumble.

The camera and weapon pivot stay outside this visual wrapper. Weapons still use the existing
combat pivot; proper hand attachment and combat animation are still to do.

## Preparing these scenes again

Use **Tools → Portaland → Prepare Ranger Roll Tests** outside Play mode. It configures both
controllers and the two gameplay scenes. Save any edits in an already-open test scene first.
The command can create the female scene from the male test if it doesn't exist yet, and
keeps an existing female visual rather than replacing it every time.

## What was checked

Both actual test-scene Animators were sampled through takeoff, falling, landing, crouch,
roll entry/exit, rolling off an edge and crouched landing. Roll progress matched the movement
duration. Model roots stayed fixed and the sampled skinned vertices passed finite/size checks.
The [check report](movement-state-check-results.json) lists the checks and their limits.

These were scripted animation-state checks, not interactive physics or keyboard tests.
Stride/roll appearance and gameplay collisions need checking in Play mode. The current
roll duration plays the supplied clip faster than its original length; keep the existing
gameplay timing until the group agrees any tuning. Directional crouching still needs clips.

For death/reload tests, add the test scene to enabled Build Settings first. This setup doesn't
change the project's build scene list. No commit has been made for this step yet.
