# The vampire boss

This is the first boss in the game, the Servant of Dracula, who's in the church in Main Game.
I based the fight on Soldier of Godrick from Elden Ring. He's the first boss so he should teach
you to roll, punish and parry, and be beatable in a few tries without being a pushover.

## The fight

When you walk into the church he just stands there until you get within about 10 metres (or
hit him). Then his name and health bar come up at the bottom, the music starts and red mist
blocks both church doors so you can't leave.

His attacks are a quick claw swipe that often goes into a backhand, a slow overhead that hits
hard (about 31), a delayed swipe that catches you if you roll too early, and a lunge he can do
from up to 9 metres away so you can't just run off and heal. When he drops to half health he
roars, his claws glow brighter and he gets a bit faster.

He doesn't flinch when you hit him, and if you hit him 3 times in a row really fast he'll
swing back at you, so spamming attacks doesn't work. Parrying him is the best way to do damage:
he's stunned for a couple of seconds and a riposte does 4.5x.

When he dies you get VANQUISHED with a church organ sound, the mist fades so you can leave, and
a Blood Vial goes straight into your inventory. If you die, the scene reloads and the fight
resets.

## In the scene

In Main Game's Hierarchy there's **Vampire (Boss)**, which is the boss, and **Boss Barrier** and
**Boss Barrier (1)**, which are the red mist in the two doorways. The barriers are hidden until
the fight starts.

The vampire comes from `Assets/Prefabs/Combat/Vampire.prefab`. I put the boss settings on the
one in the church rather than on the prefab, so the prefab can be used as a normal vampire
enemy later.

## Changing it

Most of the boss settings are on the **Boss Encounter** component on Vampire (Boss): his name,
how close you need to get before he wakes up, his damage and poise, how aggressive he is, the
barriers, the music and the reward. Each one has a tooltip. His health is on the Health
component (350) and his moves are on Enemy Melee.

If you want the mist in another doorway, make an empty object, add **Arena Barrier** to it and
resize its Box Collider to fill the doorway (about 2.4 wide, 3.2 tall and 0.3 thick, with the
centre Y at 1.6 so it sits on the floor). Then add it to the Barriers list on Boss Encounter.

## Credits

The vampire model is "Vampire Rigged 3D" by Zain.2407 on Sketchfab. It's CC BY 4.0 so he needs
to be in the game credits. More details are in `Assets/ThirdParty/Sketchfab/Vampire/README.md`.
The sounds and music are CC0, see `Assets/ThirdParty/Audio/README.md`.
