# Character setup for the group

This is the practical handover for bringing the character work into another scene or main.
Updated 4 November 2026. The work is still on `feature/player-character-animation-23`.
Directional clips, final jump checks, combat animations and female gameplay checks remain.

## Keep these folders together

| Folder | What belongs here |
| --- | --- |
| `Assets/ThirdParty/Quaternius` | Downloaded models, textures, animations and CC0 licence files |
| `Assets/ThirdParty/KayKit/CharacterAnimations` | Free side/back animation source and original CC0 licence |
| `Assets/Art/Characters/Player` | Our assembled male/female visual prefabs and copied meshes |
| `Assets/Animations/Player` | Shared controllers and clip-choice assets |
| `Assets/Scripts/Animation` | Scene-independent animation scripts |
| `Assets/Editor` | Optional preparation tools, excluded from game builds by Unity |
| `Assets/Scenes/Tests` | Character tests, kept separate from the shared Prototype |
| `docs/character-animation` | Setup, test results and remaining work |

Always include the matching `.meta` files. They keep prefab, mesh, clip and script GUIDs
consistent on everyone's machines. Don't regenerate the prepared assets after a merge just
because the helper exists; use the committed prefabs and controllers.

The preparation tools now run **only from Tools → Portaland**. Opening Unity or recompiling
doesn't build test scenes or change asset import settings. The directional tool also waits
for real clip assignments before making a controller.

## Adding a character to another scene

Use `RangerMovementTest` as the reference. Keep the destination scene's existing Player root
and gameplay components, since the camera, inventory and other objects may refer to them.
The visual prefabs don't contain the full gameplay Player.

1. Under the existing Player, create an empty **CharacterVisual** at the CharacterController
   centre. Add `PlayerRangerMale` or `PlayerRangerFemale` beneath it.
2. Fit the character to the controller height and align its feet with the capsule bottom.
   In the current test the capsule is centred, 2 units tall, so its bottom is local Y -1.
   Use the test instance as a reference; model import scale can differ.
3. Disable only the root capsule's **MeshRenderer**. Keep its CharacterController and scripts.
4. Add **Player Locomotion Animation** to Player and assign the child Animator in its
   **Character Animator** field. It reads PlayerMovement/CharacterController from this root.
5. Assign `RangerLocomotion.controller` to the Animator, or the validated directional
   controller once it has been built. Turn **Apply Root Motion off**.
6. On Player Movement, enable **Resize Controller For Crouch**, disable **Create Capsule
   Roll Visual** and assign **CharacterVisual** as **Roll Visual**.
7. Keep the existing CameraFollow target and weapon pivot references. Don't parent the
   camera or weapon pivot under CharacterVisual; that wrapper rotates during a roll.
8. Save the scene and test movement, crouching, jumps, collisions, equipment and damage.
   For scene reload on death, add the destination scene to enabled Build Settings.

The runtime animation scripts don't load a scene by path or search for a Player by name.
Their Inspector references work in any scene. The test builder has fixed paths because
it's a convenience for producing our test, not the runtime integration mechanism.

## When merging with main

- Review `Playermovement.cs` with Uliana: the crouch option, height/centre storage, ceiling
  origin and roll readiness check are the movement-side edits. The option defaults to off.
  Preserve any new movement logic from main when resolving conflicts.
- Keep main's latest shared scene. Bring the character into that scene using the steps above
  rather than replacing it wholesale with our test copy.
- Review any animation-library `.meta` conflict carefully: our clips use Humanoid imports
  and loop/root settings. Losing those settings can break the controllers.
- Keep the user's unrelated Prototype preview edits, combat README edits and spare supplier
  prefab out of a character PR unless they're reviewed and intentionally included.
- Test both models and existing gameplay after resolving conflicts. Current tests aren't
  evidence that a future merge against newer movement code works automatically.

No merge, push or commit is performed by these tools. Git actions remain separate from setup.
