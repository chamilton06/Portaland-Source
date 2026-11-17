# Quaternius Low Poly Medieval Weapons

Added on 9 November 2026 for the combat animation and weapons work.
Models are by Quaternius: https://quaternius.itch.io/lowpoly-medieval-weapons

## Licence

CC0 1.0 Universal (public domain), as stated on the itch.io page. No credit is required.
The download didn't include a licence file, so this note records it.

## What's here

`Models/` holds the 24 FBX files from the download:

| Type | Files |
| --- | --- |
| Swords | `Sword`, `Sword_2`, `Sword_Big`, `Sword_Golden`, `Claymore` |
| Daggers | `Dagger`, `Dagger_2` |
| Axes | `Axe`, `Axe_Small`, `Axe_Double` |
| Hammers | `Hammer_Small`, `Hammer_Double` |
| Polearms | `Spear`, `Scythe` |
| Shields | `Shield_Round`, `Shield_Round_2`, `Shield_Heater`, `Shield_Heater_2`, `Shield_Celtic_Golden` |
| Bows | `Bow_Wooden`, `Bow_Wooden2`, `Bow_Golden`, `Bow_Evil`, `Arrow` |

The download also came with `.blend` and `.obj` versions of the same models. They weren't
copied into `Assets`, because Unity only needs the FBX files. Unity would also try to open
`.blend` files with Blender on every machine, which fails for anyone without Blender installed.

## Using them

These are the raw supplier models. Don't edit them here. Make our own prefabs, such as the
equipped weapon prefabs, outside this folder and reference these models from them.
Keep the `.meta` files with the models in Git so prefab references stay the same for everyone.
