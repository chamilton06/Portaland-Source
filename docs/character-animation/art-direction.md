# Character and art direction

Updated 1 November 2026. These are Kartikeya's preferences and the group's suggested packs,
not confirmed choices for the whole game. No assets have been imported as part of this work.

## What we're aiming for

Elden Ring is the reference for the mood: a worn medieval adventurer exploring a dangerous
fantasy world. For our project, I'd start with normal human proportions, a readable armour
silhouette, muted cloth colours and weathered metal. We can still use low-poly assets if
the shapes, materials and colours look consistent together.

KayKit was the first suggestion before this reference was given. Its chunky proportions
are less suitable now, so we should compare more naturally proportioned knights or travellers.
Avoid choosing a character just because it has lots of detail: it needs to fit the environment
and weapons the group actually settles on.

This doesn't add combat features to issue #23. Camera changes, lock-on, attack timing and
other Souls-like gameplay choices would need separate tasks. Here we're choosing the model
and animating the movements that already exist.

## Packs suggested by the group

Official listing links were checked during this research. The fit notes below are planning
judgements based on the kinds of assets proposed, not results from an in-engine comparison.

| Pack | What to compare before choosing |
| --- | --- |
| [Free Ui Pack](https://assetstore.unity.com/packages/2d/gui/icons/free-ui-pack-170878) | Icon readability, colour and border style alongside the existing inventory and journal |
| [Free Low Poly Swords](https://assetstore.unity.com/packages/3d/props/weapons/free-low-poly-swords-189978) | Simple weapon shapes against the character's level of detail |
| [Low Poly Weapons](https://assetstore.unity.com/packages/3d/props/weapons/low-poly-weapons-71680) | Whether its silhouettes and materials match the chosen sword set |
| [Elven Weapons](https://assetstore.unity.com/packages/3d/props/weapons/elven-weapons-36901) | Whether ornate fantasy weapons work as special equipment alongside plainer starting gear |
| [Treasure Chest Maker](https://assetstore.unity.com/packages/3d/props/treasure-chest-maker-20850) | Wood/metal materials, scale and detail next to the character |
| [Greatsword of Frozen Night](https://assetstore.unity.com/packages/3d/props/weapons/greatsword-of-frozen-night-28991) | Large weapon proportions, hand placement and clipping; a model alone won't provide a two-handed animation setup |
| [Medieval Tavern Pack](https://assetstore.unity.com/packages/3d/props/furniture/medieval-tavern-pack-112546) | Furniture scale and surface detail as a reference for the character |
| [FREE - RPG Weapons](https://assetstore.unity.com/packages/3d/props/weapons/free-rpg-weapons-199738) | Another weapon option; this is the full listing for the shared package ID 199738 |

These packs don't include a confirmed player character. Having several weapon packs available
doesn't mean we need all of them in the game. Start with one character, one basic weapon and
one representative part of the environment, then compare them together in the same lighting.

## Technical checks

- The current project settings point to the **Built-in Render Pipeline**: GraphicsSettings has
  no custom pipeline assigned, QualitySettings has no pipeline override found, and the package
  manifest doesn't list URP/HDRP. Keep this setup during character work.
- Check each selected pack's actual shaders in Unity 2023.1.22f1. A listing's original Unity
  version isn't proof that every material will work unchanged in our project.
- Character requirements: a valid Humanoid Avatar, usable hand bones, sensible scale and
  in-place locomotion clips. Check this on the actual import before wiring up the Animator.
- Keep the supplier licence and record where the pack came from. Free pricing doesn't mean
  the files are CC0 or suitable for public redistribution.

## Next character comparison

Look for an armoured knight or lightly equipped traveller with natural proportions. Compare
the front, side and back views, since we'll see the back most often during play. Start with
a short tunic or simple armour rather than a long cape that could need extra cloth work.
The final model is still Kartikeya's choice.
