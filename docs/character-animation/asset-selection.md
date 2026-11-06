# Choosing the player character

Status: **Quaternius selected as the first free option to try** (2 November 2026).
The downloaded Unity files have been copied to `Assets/ThirdParty/Quaternius` (2 November
2026). All three supplied licences were checked and say CC0. Unity Editor import checks,
exact character assembly and rig validation are still pending.
Initial research: 1 November 2026.

## Current plan: keep it free

Start with the free Standard editions of these official downloads:

1. [Universal Base Characters](https://quaternius.itch.io/universal-base-characters) — `Universal Base Characters[Standard].zip`.
2. [Modular Character Outfits - Fantasy](https://quaternius.itch.io/modular-character-outfits-fantasy) — `Modular Character Outfits - Fantasy[Standard].zip`.
3. [Universal Animation Library](https://quaternius.itch.io/universal-animation-library) — `Universal Animation Library[Standard].zip`.

On each page, choose **Download Now**, then **No thanks, just take me to the downloads**,
and pick the Standard archive. Source and Pro editions are optional paid versions.
Don't assume the free edition contains everything shown in the full-pack previews.

For now, save the ZIPs in a folder outside Unity's `Assets`, such as
`Downloads/Portaland-Character-Assets/`. We'll inspect the included README/licence and the
male/female clothing first, then copy only the files we need into the project. The official
pages label these packs CC0, but we still need to retain the licence from the actual downloads.

The animation page advertises in-place and root-motion versions for the overall library.
Check the Standard archive's actual clips before deciding whether Mixamo is needed for gaps.
Use in-place clips for our existing CharacterController movement.

### Unity Student benefits

The [official Unity Student page](https://unity.com/products/unity-student), checked on
2 November 2026, lists a premium Synty asset bundle and an Odin Inspector/Validator education
licence among its benefits. It doesn't identify all Synty pack names on that page, and we
haven't inspected Kartikeya's account, so no specific character entitlement is confirmed.

Check the Student plan activation/benefits email using the same Unity ID as the student
licence. Follow its Synty redemption instructions, then check the Asset Store's **My Assets**
or Unity's **Window → Package Manager → My Assets** for anything already claimed. Share pack
names for comparison; keep redemption codes private. These benefits don't make every paid
Asset Store item free. We'll carry on with Quaternius without depending on student extras.

## Updated shortlist: one male and one female

We now want two matching player models. Use the same gameplay root and controller setup
for both, with separate visual prefabs and validated Humanoid Avatars. We can share an
Animator Controller if retargeting looks right on both. A character selection screen isn't
part of this asset decision yet.

Prices below were displayed in USD before tax on 1 November 2026; checkout pricing may differ.
These are listing claims, not import tests in our Unity project.

| Pack | Listed price | Why it's on our list | Main trade-off |
| --- | --- | --- | --- |
| [Polytope Low Poly Modular Armors](https://assetstore.unity.com/packages/3d/characters/humanoids/low-poly-modular-armors-lowpoly-medieval-fantasy-series-229963) | $64.99 | My preferred paid starting point for comparison with the group's low-poly weapons. Creator lists five male and five female armour sets, shared Humanoid rig, Mecanim/Mixamo support and Built-in/URP support. | Still visibly low-poly; no animations included. Needs our normal import and material checks. |
| [Stylized Medieval Modular Characters — JustCreate](https://assetstore.unity.com/packages/3d/characters/humanoids/humans/stylized-medieval-modular-characters-336650) | $69.99 | One male and one female, ranger/warrior outfits, Humanoid/Mixamo support. Listing says a Standard/Built-in package is included. | More stylised than a realistic dark-fantasy character. URP is the default setup; no animations included. |
| [Inquisitor — Ida Faber](https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/inquisitor-rpg-dark-fantasy-modular-female-and-male-characters-305495) | $119.99 | Male and female dark-fantasy characters, Humanoid rig and listed Built-in support. Worth comparing if we want more detailed character materials. | 2.3 GB download, no LOD system according to the description, and more shader/customisation work to assess. Could stand out against simple weapons. |
| [Quaternius Universal Base Characters](https://quaternius.com/packs/universalbasecharacters.html) + [Fantasy Outfits](https://quaternius.com/packs/modularcharacteroutfitsfantasy.html) | Free Standard tiers | Best free route to investigate for matching characters. Base pack lists male/female proportions; outfit pack lists compatible modular clothing and Humanoid retargeting. Official pages label these packs CC0. | Full pack contents aren't all in the free tier. Check exact male/female outfit coverage and included licence in the downloads before choosing. The ready-made engine project is URP; we need FBX/material setup for Built-in. |

Source detail for Polytope's included parts: [creator's portfolio page](https://polytope.artstation.com/projects/Xnnkgw).
The [Quaternius outfit download page](https://quaternius.itch.io/modular-character-outfits-fantasy/purchase)
confirms a free Standard archive and a paid Source archive. Keep the exact downloaded licence
with any selected files rather than assuming all packs or editions use the same terms.

Earlier comparison: Polytope was the paid starting point. The current choice is to inspect
Quaternius Standard first and keep costs at zero. The free downloads are now copied into
the project; no paid assets were purchased.

## Earlier options and selection criteria

The direction is now a darker medieval adventurer, with Elden Ring as a reference for mood
and character silhouette. KayKit is no longer the first choice: its chunky proportions could
make the player feel too cartoon-like for this direction. See the [art direction notes](art-direction.md)
for the group's suggested assets. This is a proposed direction, not a group-wide asset decision.

We want a character that reads clearly from behind. A ready-made model should
save time, but being rigged doesn't automatically mean Unity's Humanoid Avatar will validate.
We need to check the actual skeleton and animation previews before settling on the import.

## Shortlist

| Option | Why consider it | What we'd need to check |
| --- | --- | --- |
| [KayKit Adventurers](https://kaylousberg.itch.io/kaykit-adventurers) | Earlier suggestion for a more cartoon-like direction. The creator lists five free rigged characters, FBX files, basic movement animations and CC0 licensing. | Lower priority after the Elden Ring reference. The proportions are quite chunky. Check Humanoid bone mapping, retargeting, and which movement clips come in the free download. |
| [Quaternius Ultimate Modular Men](https://quaternius.com/packs/ultimatemodularcharacters.html) | More customisation: the pack page lists 11 characters with swappable parts, 24 animations, FBX files and CC0 licensing. | More parts to organise. Check the outfit suits our environment, the included licence matches the download, and the rig validates as Humanoid. |
| [Mixamo character library](https://www.mixamo.com/) | A practical fallback if the first two rigs need too much work. Adobe provides humanoid rigging and animation tools. | Choose a specific model first; visual style varies. Requires an Adobe ID. Check the downloaded model and terms before adding raw files to the shared repository. |

These are style and workflow recommendations, not confirmed Unity compatibility results.
We haven't tested any of these downloads in this project.

## Animation source

[Adobe's Mixamo FAQ](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html) says the service
is free with an Adobe ID and permits royalty-free use of its characters and animations in games.
Its rigging guidance also flags unusual proportions and disconnected parts as possible problems.
This matters when comparing stylised characters. Game-use permission alone isn't evidence that
raw assets can be redistributed publicly, so check the applicable terms before committing downloads.

Prefer the chosen pack's own clips if they cover our movements and look right. Use Mixamo or
another compatible source for gaps once we know the rig works.

## Record this when we choose

- Character and pack/version: **Quaternius Standard packs; Ranger/Peasant male and female outfit FBXs present. Final assembly pending.**
- Creator and official download URL: **Quaternius; official pack links above**.
- Download date and licence file location: **downloads located on 2 November 2026; original licences retained under each pack in `Assets/ThirdParty/Quaternius`**.
- Permission to share the source assets in our group repository: **all three supplied licences checked; CC0 dedication**.
- Unity Humanoid Avatar result: **Kartikeya reports completing the setup for both Rangers. At the pre-commit check, both saved `.meta` files still show `animationType: 2` (Generic), `avatarSetup: 0` and empty human mappings. Recheck Apply/Configure in Unity before treating Humanoid setup as complete.**
- Included clips and any missing directions/states: **not inspected**.
- Final import paths and settings: **source files and Unity-generated `.meta` files are present under `Assets/ThirdParty/Quaternius`; material and rig validation pending**.

Keep the supplied licence alongside the imported pack. Credit the creator in our project notes
even if attribution isn't required. Record any changes we make to the original assets.
