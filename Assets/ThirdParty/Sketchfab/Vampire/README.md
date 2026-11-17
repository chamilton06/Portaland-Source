# Vampire character

"Vampire Rigged 3D" by **Zain.2407**, from Sketchfab:
https://sketchfab.com/3d-models/vampire-rigged-3d-1e543399464e4f5180c5d2f3486d3cd4

Licensed under **CC BY 4.0** (https://creativecommons.org/licenses/by/4.0/). We can use and
change it, including in a public repo, as long as the author is credited. Keep this file with
the model, and put the credit in the game's credits too.

Downloaded on 16 November 2026. We're using it for the vampire boss in the church, and later
for normal vampire enemies.

## What we changed

- Renamed `source/Model.fbx` to `Model/Vampire.fbx`.
- Shrank the textures from 4096 to 2048 so the repo stays small.
- Only kept the colour and normal maps. The roughness, metallic and height maps from the
  download weren't copied, since the Built-in pipeline's Standard shader doesn't use them as
  they are.

## Notes

- The skeleton is a Mixamo rig, so it imports as Humanoid and plays our Quaternius
  animations.
- Materials and the enemy prefab are made by **Tools → Portaland → Prepare Vampire**, outside
  this folder. Don't edit the files in here directly.
