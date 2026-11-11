# Guide for combat animations in scenes

This is the practical handover for bringing the character and combat work into another scene
or into main. Updated 11 November 2026. Everything is on `feature/combat-animation-weapons`,
which builds on the earlier `feature/player-character-animation-23` branch.

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
| `Assets/Prefabs/Combat` | The Dark Ranger enemy and the training dummy |
| `docs/character-animation`, `docs/combat` | These notes |

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
   Roll Visual** and leave **Roll Visual** empty when using the new roll animation. On
   **Player Locomotion Animation**, assign **CharacterVisual** as **Roll Facing**.
7. Keep the existing CameraFollow target and weapon pivot references. Don't parent the
   camera or weapon pivot under CharacterVisual; that wrapper turns towards the roll direction.
8. For the combat animations, add **Player Combat Animation** and **Weapon Hand Socket** to
   Player and use `RangerDirectional.controller`. The quickest way to get the attack settings
   is from the test scene: right-click **Player Combat** on its Player, choose **Copy Component**,
   then on your Player choose **Paste Component Values**. Do the same for **Player Defence**.
   Check **Weapon Pivot** afterwards: links to objects in another scene don't copy across, so
   point it back at your own Player's WeaponPivot.
   Add **Player Lock On** and **Enemy Health Bars** too if you want them.
9. Save the scene and test movement, crouching, jumps, collisions, equipment and damage.
   For scene reload on death, add the destination scene to enabled Build Settings.

The runtime animation scripts don't load a scene by path or search for a Player by name.
Their Inspector references work in any scene. The test builder has fixed paths because
it's a convenience for producing our test, not the runtime integration mechanism.

## When merging with main

- Go over `Playermovement.cs` with Uliana. The crouch option, the stored height and centre,
  the ceiling check and the roll check are the changes on the movement side. The crouch option
  is off by default. If main has newer movement code, keep it when you sort out conflicts.
- Keep main's version of the shared scene and add the character to it using the steps above.
  Don't replace it with our test scene.
- Be careful with `.meta` conflicts on the animation files. Our clips rely on their Humanoid,
  loop and root settings, and losing them breaks the controllers.
- The setup tools never commit, push or merge anything. That's always done separately.

## Check before merging

Play each test scene, then the merged scene, and make sure that:

- walking, sprinting, strafing, crouching, jumping and rolling all look right, with both models
- the sword stays in the hand through every attack
- light, heavy, jump, sprint and crouch attacks and the weapon art (G) all play and do damage
- blocking, parrying and getting hit play their animations
- lock-on works, and enemies show health bars when hit
- the Dark Ranger chases, attacks, staggers and dies
- the player dies and the scene reloads
- the Console has no red errors
