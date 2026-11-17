# Combat audio

All of these are **CC0**, so they're free to use and change in a public repo, and crediting is
optional. We credit them anyway, so they should go in the game's credits too. Downloaded on
17 November 2026.

| Folder | What | From | Licence |
| --- | --- | --- | --- |
| `Kenney/Impacts` | Hits (punch medium/heavy), blocks (metal medium), parries (bell heavy) | [Impact Sounds](https://kenney.nl/assets/impact-sounds) by Kenney | CC0, see `Kenney/License-ImpactSounds.txt` |
| `Kenney/Foley` | Cloth rustles, used for rolls | [RPG Audio](https://kenney.nl/assets/rpg-audio) by Kenney | CC0, see `Kenney/License-RPGAudio.txt` |
| `OpenGameArt/Swishes` | Swooshes for swings and claw swipes | [Swishes Sound Pack](https://opengameart.org/content/swishes-sound-pack) by artisticdude | CC0 |
| `OpenGameArt/Monster` | Growls and shrieks, used as the vampire's voice | [Monster Sound Pack](https://opengameart.org/content/monster-sound-pack-volume-1) by Ogrebane (the download is named Volume 2) | CC0 |
| `OpenGameArt/Music` | "Epic Boss Battle", loops seamlessly | [Boss Battle Music](https://opengameart.org/content/boss-battle-music) by Juhani Junkala | CC0 |

## What we changed

- Only copied the sounds we use. The rest of each pack wasn't copied.
- Renamed the monster files so they all start with a capital letter.
- Converted the boss music from a 21 MB WAV to a 3 MB OGG.

## Where they're used

The sounds are picked in `Assets/Audio/CombatSounds.asset` (player and general combat) and on
the vampire prefab's **Enemy Voice** component. To swap a sound, drag a different clip into the
list there. Don't edit the files in this folder.
