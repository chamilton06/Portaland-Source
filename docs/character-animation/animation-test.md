# Testing the Ranger animations

This is a separate test, before we connect the models to the playable capsule.
Added 3 November 2026. Unity compiled the setup and generated the controller and test scene.
Kartikeya tested it and reported that it works. Detailed clipping and deformation checks
are still to be recorded.

The first setup attempt compiled but stopped while looking up clips because more than one
subasset matched a short name. The lookup now excludes Unity's `__preview__` clips and prefers
the exact exported take name before trying a short name. The corrected setup retries after
compilation. Scene generation and a working test were subsequently confirmed.

## Where things are

| File | Purpose |
| --- | --- |
| `Assets/Editor/PrepareRangerAnimationTest.cs` | Sets up the animation import, test controller and separate scene |
| `Assets/Scripts/Animation/RangerAnimationPreview.cs` | Switches both test characters between idle, walk, jog and sprint |
| `Assets/ThirdParty/Quaternius/AnimationLibrary/Models/UAL1_Standard.fbx` | Original in-place animation file |
| `Assets/Animations/Player/RangerPreview.controller` | Generated test controller; not wired to gameplay |
| `Assets/Scenes/Tests/RangerAnimationTest.unity` | Generated side-by-side test scene |

The last two files are created by the Editor setup. It attempts to run once per Editor
session after compilation, like the character preparation helper. If needed, run
**Tools → Portaland → Prepare Ranger Animation Test** outside Play mode.
Existing controller and scene files at these paths are left alone on later runs.

## Running the test

1. Go back to Unity and let it compile/import. Look for `Ranger animation test ready`
   in the Console, or read the setup error if one appears.
2. Save any scene work you want to keep before opening another scene.
3. Open **Assets → Scenes → Tests → RangerAnimationTest**.
4. Press Play, then click inside the Game view.
5. Press **1** for idle, **2** for walk, **3** for jog and **4** for sprint.
6. Both characters should change together. They animate on the spot; this scene has
   no gameplay movement controller. Stop Play mode when finished.

You can also select **Preview Controls** and adjust **Preview Speed** during Play mode.
Values between the whole numbers let us inspect the blends. These values are test choices,
not metres per second; they aren't the final movement thresholds.

## How it works

The animation FBX is set to Humanoid and checked for a valid Avatar. The setup finds the
actual clip names: `Idle_Loop`, `Walk_Loop`, `Jog_Fwd_Loop` and `Sprint_Loop` (allowing
the exported `Armature|` prefix). These clips loop, with horizontal root translation and
root rotation baked into the pose. Both preview Animators have Apply Root Motion off.

One 1D blend tree uses the float parameter `Speed`: idle at 0, walk at 1, jog at 2 and
sprint at 3. The test script only sets that parameter. Each character keeps its own Avatar.
The shared player prefabs and Prototype scene aren't edited by this setup.

The preview script belongs only on the test scene's Preview Controls object. It must not
go on the gameplay Player, where the future animation driver will read movement state.

## What to check

- Idle: no T-pose, head facing the expected way, no detached eyes or hood.
- Walk/jog/sprint: legs and arms bend properly on both characters; clothing follows.
- Neck and shoulders: no obvious gaps, stretching or skin poking through the outfit.
- Transitions: blends don't snap into a completely different pose.
- Position: characters stay in their test spots; no root-motion drift.
- Console: no new import, Avatar or Animator errors.

Record problems with the character, clip and a screenshot. Passing this test only confirms
basic animation retargeting; jumping, landing, crouching, rolling and gameplay integration
still need their own checks. The basic test works according to Kartikeya's report; individual
quality checks above haven't been recorded as passed yet.
