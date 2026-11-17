# Notes for Cole and Uliana

Just so you both know what I changed in your parts and how to link things up with the boss. I
haven't rewritten anything of yours, the changes are small and they're all listed here.

## Cole

In Main Game I added the combat stuff to the Player (the sword, attacks, blocking, lock-on,
health bars and sounds), the vampire boss inside the church, and the red mist barriers in the
two church doorways (Boss Barrier and Boss Barrier (1)). I didn't move or delete anything of
yours and the player spawn is still where you put it. AttackingDummy and StillDummy are still in
there too, feel free to delete them if you don't need them.

For the blacksmith quest you shouldn't need any extra code on the boss, because the fight only
starts when the player walks up to him. If you only want him there once the quest starts, keep
Vampire (Boss) and both Boss Barrier objects turned off in the scene and turn them on with
`SetActive(true)` when the blacksmith gives the quest.

To complete the quest when he dies, there's an On Death event on the vampire's Health
component. You can add your quest script to it in the Inspector, or do it in code:

```csharp
vampireHealth.onDeath.AddListener(CompleteChurchQuest);
```

He also gives the player a Blood Vial when he dies. It goes through `Inventory.AddItem`, so the
quests will see it through `ItemAdded` like any other item.

## Uliana

The Blood Vial is in `Assets/Resources/Items/Crafting/BloodVial.asset` with your other crafting
items, and its icon is `BloodVial.png` in the same folder. You get one from beating the vampire
and it goes straight into the inventory. It's just a normal item so you can use it in recipes,
and you can change the name, description or icon if you want.

I changed two numbers in `PlayerStats`: `staminaRegen` from 15 to 40 and `regenDelay` from 1 to
0.6. Stamina was running out way too fast in fights. I changed them on the Player in Main Game,
Prototype and the test scenes as well. Nothing else in the file is different, but let me know
if it messes anything up for you.

I didn't change your `DeathScreen`. Main Game uses my `PlayerDeathHandler` for the YOU DIED
screen, so if you want yours used there instead, let me know and we can sort it out.

I also used your Cinzel font for the boss name, VANQUISHED and YOU DIED. I didn't change the
font files.

## Both of you

Stamina is faster for the whole game now (rolls cost 18 instead of 25 too), so tell me if
sprinting or exploring feels weird.

If Unity says Main Game has changed on disk when you open it, click Reload. The main combat
notes are in [README.md](README.md) and the boss is in [boss-fight.md](boss-fight.md).
