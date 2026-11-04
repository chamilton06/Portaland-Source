# Preparing the Ranger prefabs

The Editor helper is `Assets/Editor/PrepareRangerCharacters.cs`.
Run it explicitly from
**Tools → Portaland → Prepare Ranger Characters** while outside Play mode.

It builds `PlayerRangerMale.prefab` and `PlayerRangerFemale.prefab` in
`Assets/Art/Characters/Player/`. Existing prefabs at those paths are left alone.
The output is a visual prefab, not a replacement for our gameplay Player.

The base character provides the Humanoid Avatar and skeleton. The helper copies its
skin meshes and keeps only triangles weighted mainly to the neck/head bones. This leaves
the original downloaded mesh untouched. It then adds the outfit and maps its skin bones
to the base skeleton by name. Clothing gets copied meshes with matching bind poses.
Only the base Animator is kept, with root motion switched off.

Both base Avatars must validate as Humanoid first. Missing bone matches or invalid Avatars
stop preparation and print an error in the Console. Generated assets for a failed character
are cleaned up; an already completed character remains available. The helper doesn't save
or alter the open scene, the Player object or source import settings.

## Checks still needed

Generation is confirmed: both prefab files and their generated mesh assets are present,
and the Unity Editor log reports `Ranger preparation finished`. This confirms the helper
compiled and completed. A Scene view preview shows both characters with faces inside the
hoods and clothing in place. Close-up clipping checks and animation deformation are still untested.
Preview both generated prefabs before
using them. Check the neck edge inside the collar, eyes, hood and clothing alignment.
Test an idle and a walk animation for deformation before calling assembly complete.

The head mesh still contains unused body vertices; only its visible triangle lists are
filtered. We can compact these meshes later if needed. A weight-based cut is a first pass
and may need adjustment after the visual check. Materials are inherited from the imports;
shader and skin texture checks are still needed.

Keep generated `.asset`, `.prefab` and `.meta` files together in Git after reviewing them.
The prefab preparation checkpoint includes the helper, generated meshes, both prefabs and
their metadata. Temporary preview placements in Prototype are kept out of this commit.
