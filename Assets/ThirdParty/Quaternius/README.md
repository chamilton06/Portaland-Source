# Quaternius character assets

Added from the free Standard downloads on 2 November 2026. Models and textures are by
Quaternius. The original licence files are kept inside each pack folder; all three say CC0.

## Finding the files in Unity

In the Project window, open **Assets → ThirdParty → Quaternius**.

| Folder | Contents |
| --- | --- |
| `BaseCharacters/Models` | Male and female base character FBX files |
| `BaseCharacters/Textures` | Base textures, including supplied Unity/Godot normal maps |
| `BaseCharacters/Hairstyles` | Unity FBX hair/eyebrow models, in both supplied attachment variants, plus textures |
| `FantasyOutfits/Models/Outfits` | Male/female Ranger and Peasant FBX files |
| `FantasyOutfits/Models/Modular Parts` | Individual clothing pieces |
| `FantasyOutfits/Textures` | Clothing and body textures, kept in the supplier's layout |
| `AnimationLibrary/Models` | `UAL1_Standard.fbx`, the in-place animation file |
| `MedievalWeapons/Models` | 24 weapon and shield FBX files, added 9 November 2026 (see its README) |

The root-motion `_RM` animation file, glTF/Unreal exports and original ZIPs weren't copied.
The outfit texture folder still contains the supplier's alternative normal maps. Pick the
correct version when setting up materials rather than assigning textures just by name.

## Before using them on Player

These are source assets, not finished playable character prefabs. Unity import, Humanoid
Avatar mapping, materials and animation previews still need checking in the Editor.
The Prototype scene hasn't been changed.

The outfit README says to use only the base character's head with clothing. The free base
files are full-body models, so we still need to prepare a suitable head-only visual. Don't
put the full body under the outfit and assume the clipping is fixed.

We're using the Built-in Render Pipeline. Create our materials/prefabs outside this supplier
folder, and keep Apply Root Motion off when connecting animations to PlayerMovement.
Unity has generated `.meta` files for these assets. Keep those files with the assets in Git
so everyone gets the same GUIDs and saved import settings. The initial saved Ranger settings
still show Generic; recheck Apply/Configure before treating Humanoid validation as complete.

See `docs/character-animation/` at the repository root for the setup plan, source links,
test checklist and `import-manifest.json` (copied file paths and SHA-256 hashes).
