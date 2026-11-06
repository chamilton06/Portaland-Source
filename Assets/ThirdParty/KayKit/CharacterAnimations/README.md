# KayKit movement animations

From Kay Lousberg's **KayKit Character Animations 1.1**, free edition.
Official source: https://kaylousberg.itch.io/kaykit-character-animations
Downloaded 4 November 2026. The original `License.txt` says CC0.

Only `Rig_Medium_MovementAdvanced.fbx` and its licence were copied from the archive.
Our source/file hashes are in `docs/character-animation/kaykit-import-manifest.json`.

We want to try these clips on the Rangers:

- `Running_Strafe_Left`
- `Running_Strafe_Right`
- `Walking_Backwards`

These are actual side/back clips. The side clips have a running gait, so their pace needs
checking against our movement speeds. They aren't matching walk-strafe clips.
The archive's glTF equivalents have no horizontal root/hips translation in these clips;
Unity Avatar validation and automated retargeting/root-drift checks passed on the FBX for
both Ranger Avatars. Visible stride and clipping checks still need doing.

Use **Tools → Portaland → Set Up Directional Animations**, then **Prepare free KayKit clips
and build controller**. This configures Humanoid mapping and looping on these three clips,
validates the Avatar and fills empty directional slots. It leaves existing clip choices alone.
The included rig calls its wrist joints `wrist.l`/`wrist.r`; our import maps those to Hand.

Both Ranger models need a Play mode check for shoulders, hands, feet, clothing and pacing.
Keep Apply Root Motion off. No directional crouch clips were found in this file.
