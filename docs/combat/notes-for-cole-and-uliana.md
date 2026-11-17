# Notes for Cole and Uliana

What the combat work changed in your parts of the project, and how to hook into it. Nothing of
yours was rewritten. The changes below are small, and each one is listed.

## Cole

### What I added to Main Game

All of it is new objects or components. Nothing of yours was moved or deleted, and your
player spawn point is untouched.

- **The Player** has the combat components now (sword in the hand, attacks, block and parry,
  lock-on, health bars, sounds). I added them with Tools → Portaland → Add Combat Player To Open
  Scene.
- **Vampire (Boss)** standing inside the church.
- **Boss Barrier** and **Boss Barrier (1)** in the two church doorways. They're hidden until
  the fight starts.

`AttackingDummy` and `StillDummy` are still in the scene. I didn't remove them in case you
wanted them, but they can go.

### Hooking the boss into your blacksmith quest

The fight only starts when the player walks up to the vampire, so the quest doesn't need any
special code:

- **To only have the boss there once the blacksmith gives the quest:** keep **Vampire (Boss)**
  and the two **Boss Barrier** objects switched off in the scene, and switch them on (`SetActive(true)`)
  when the quest starts.
- **To finish the quest when he dies:** on **Vampire (Boss) → Health**, there's an **On Death**
  event. Add your quest script there in the Inspector, or listen to it in code:

  ```csharp
  vampireHealth.onDeath.AddListener(CompleteChurchQuest);
  ```

The boss also hands over a **Blood Vial** when he dies, through `Inventory.AddItem`, so your
quests hear about it through `Inventory.ItemAdded` like any other item.

### Stamina

Stamina refills faster now for the whole game: 40 a second instead of 15, after a 0.6 second
pause instead of 1. Rolls cost 18 instead of 25. These are just numbers on `PlayerStats` and
`PlayerMovement`. If sprinting or exploring feels off now, tell me and we'll tune them together.

## Uliana

### The Blood Vial (for crafting)

- The item is `Assets/Resources/Items/Crafting/BloodVial.asset`, next to your other
  ingredients, with the icon `BloodVial.png`. It stacks to 10.
- You get one for beating the vampire boss. It goes straight into the inventory (like a boss
  reward in Elden Ring), with a small "obtained" popup.
- It's a normal `Item`, so you can use it in recipes straight away. The name, description and
  icon are yours to change.

### Small changes to your scripts

- **`PlayerStats`:** the defaults for `staminaRegen` (15 → 40) and `regenDelay` (1 → 0.6),
  and the same values on the Player in Main Game, Prototype and our test scenes. Stamina was
  running out far too quickly in fights, so I changed it for the whole game. Nothing else in
  the file changed. Shout if it causes you problems.
- I didn't touch `DeathScreen`. Main Game uses our `PlayerDeathHandler` for the YOU DIED
  screen. If you'd rather use yours there, tell me and we'll sort out which one shows.

### Fonts

The boss name, VANQUISHED and YOU DIED use your **Cinzel** font from `Assets/Fonts`. I only
reference it; the files weren't changed.

## Both of you

- If you open Main Game and Unity asks about changes on disk, choose **Reload**.
- The combat guide is [README.md](README.md), and the boss is explained in [boss-fight.md](boss-fight.md).
