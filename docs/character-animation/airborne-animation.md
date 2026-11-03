# Jump, fall and landing animations

Added after local checkpoint `15e6768` on 3 November 2026. Unity compilation and controller
generation are confirmed by the Editor log. Kartikeya reports that the movement test works,
but jumping still needs completing and its quality checks aren't marked as passed.

## Where to find it

- Scene: `Assets/Scenes/Tests/RangerMovementTest.unity`.
- Controller: `Assets/Animations/Player/RangerLocomotion.controller`.
- Driver: `Assets/Scripts/Animation/PlayerLocomotionAnimation.cs`, on **Player**.
- Editor setup: `Assets/Editor/PrepareRangerMovementTest.cs`.

Return to Unity and let it compile. The updated movement setup tries once automatically.
If needed, run **Tools → Portaland → Prepare Ranger Movement Test** outside Play mode.
It upgrades the existing controller and keeps the saved test scene intact.

## What the new states do

| State | Supplied animation | When we use it |
| --- | --- | --- |
| Jump Start | `Jump_Start` | Off the floor and moving upwards |
| Falling | `Jump_Loop` | In the air with no upward speed, including stepping off an edge |
| Landing | `Jump_Land` | Back on the floor after a jump or fall |

The free library doesn't provide a separate clip named Fall, so we're using its airborne
Jump_Loop. Preview this pose before deciding whether we need another asset.

The driver reads `CharacterController.isGrounded` and `velocity.y` after the existing
movement script runs. It sends `Grounded` and `VerticalSpeed` to the Animator along with
horizontal `Speed`. A short 0.08-second grace period helps stop little slope drops from
flickering into Falling. Upward takeoff doesn't wait for that timer.

Transitions blend over 0.08 seconds. Landing returns to locomotion near the end of its clip,
or sooner if the player is already moving. Jumping again can interrupt landing. Movement
and stamina still belong to PlayerMovement; no jump force, gravity or input was added here.
There are no changes to `Playermovement.cs` for this step.

## Test in Unity

Open the movement test scene, press Play and click inside the Game view.

1. Stand still and press Space: takeoff, airborne pose, landing, then idle.
2. Hold W and jump; repeat with Shift: movement and stamina should behave as before.
3. Walk off a platform without pressing Space: enter Falling without a takeoff pose.
4. Release Space early and try a low ceiling: return to the falling/landing states properly.
5. Jump again just after landing: the landing pose shouldn't prevent the next jump.
6. Walk along steps/slopes: check for flickering airborne poses.
7. Check feet, hood, face and clothes, plus any new Console errors.

Record failures with the action and state shown in the Animator window. The controller's
state machine can be viewed by selecting the character's Animator during Play mode and
opening **Window → Animation → Animator**.

Crouch, directional locomotion and a proper roll clip are still unfinished. Large drops
can trigger fall damage/death; scene reload requires the test scene in Build Settings.
No airborne gameplay test has been recorded as passed yet. This initial setup is included
in the working movement-test checkpoint, with jump completion still outstanding.
