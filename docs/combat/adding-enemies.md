# Adding enemies

This is for anyone putting enemies into a level. Cole, this one's mostly for you.

## The quick way

Drag `Assets/Prefabs/Combat/DarkRanger.prefab` into your scene and put it somewhere on the
ground. That's it: it finds the Player by itself and starts fighting when you get close.

Your scene needs a few things for it to work properly:

- **A Player with `PlayerStats` and `PlayerDefence`.** Without `PlayerDefence` the enemy still
  hurts you, but you can't block, parry or dodge it.
- **Colliders on the ground and walls.** The enemy uses a CharacterController and gravity, so
  it'll fall through anything without a collider.
- **`PlayerLockOn` and `EnemyHealthBars` on the Player,** if you want lock-on and health bars.
  The test scenes have them already. Prototype doesn't yet.

It goes around small obstacles but it doesn't do proper pathfinding. It'll get stuck behind a
long wall or a building. Keep that in mind when you're laying out arenas, or let me know if you
need it to find its way around.

## Making a different enemy

Don't edit the Dark Ranger prefab itself. Right-click it in the Project window and pick
**Create → Prefab Variant**, then change the variant. That way your enemy picks up any fixes
I make to the original.

On the variant's **Enemy Melee** component you can change:

- **Tint** to recolour it. White leaves the model as it is.
- **Run Speed**, **Strafe Speed** and **Turn Speed** to make it faster or slower.
- **Aggro Range** for how close you get before it notices you.
- **Circle Distance** and **Circle Time** for how far away it hangs back and how long it waits
  before attacking. Shorter circle times make it more aggressive.
- **Poise** for how much damage it takes before it staggers.
- **Weapon Model** for what it's holding. Any model whose blade points along its Y axis works.

Health is on the **Health** component, and **Drop Prefab** there lets it drop an item when it
dies, the same as the old dummies.

## Changing its attacks

The **Attacks** list on Enemy Melee is its moveset. It picks one at random each time it
attacks. Each attack has:

| Field | What it means |
| --- | --- |
| Label | The name printed in the Console when it uses the attack |
| State | Which animation to play. It has to be a state on the controller's Combat layer, e.g. `Sword_Regular_A` |
| Damage | Damage to the player |
| Reach | How close it gets before swinging, and how far the swing reaches |
| Hit Time | Seconds into the animation when the blade lands |
| Duration | Seconds until the attack is over |
| Track Until | It keeps turning to follow you until this point. After it, sidestepping makes it miss |
| Lunge | How far it steps in while winding up |
| Hold At / Hold For | For a delayed swing: when to freeze the wind-up, and for how long |
| Recovery | How long it stands open afterwards. This is the player's window to punish it |
| Follow Up | The number of another attack in the list to chain into (-1 for none) |
| Follow Up Chance | How often it does the chain |

Hit Time is the one most worth getting right. Play the game, watch when the sword actually
reaches you, and nudge the number until the damage lines up with the swing.

The animations available are the ones on the Combat layer of `RangerDirectional.controller`:
`Sword_Regular_A`, `Sword_Regular_B`, `Sword_Regular_C`, `Sword_Attack`, `Sword_Dash` and
`Sword_Heavy_Combo`. If you want one that isn't there, tell me and I'll add it through the
setup tool. That's safer than adding states by hand.

## Using a different character model

Any Humanoid model works, as long as its Animator uses `RangerDirectional.controller`. Easiest is
to swap the model inside a Dark Ranger variant: replace `DarkRangerModel` with your model, set
its Animator controller and make sure **Apply Root Motion** is off. Then line its feet up with
the bottom of the capsule.

## Writing a new kind of enemy

`EnemyMelee` is built for sword fighters. If you're making something that behaves really
differently, like an archer or an animal, it's probably easier to write its own script. To fit
in with the player's combat it needs:

- A **`Health`** component. That's all the player's attacks and lock-on look for.
- To hit the player, call `PlayerDefence.ReceiveHit(damage, transform.position)` on the player.
  It handles blocking, parrying and dodging, and tells you what happened. If the result is
  `Parried`, your enemy should stagger.
- Optionally, these interfaces from `Assets/Scripts/Combat`:
  - **`IHitReceiver`** to react when the player hits it, e.g. to flinch.
  - **`IStaggerable`** so a parried enemy can be riposted.
  - **`IBackstabbable`** so it can be backstabbed.

`EnemyMelee.cs` uses all of these, so it's a good one to copy from. `DummyAttacker.cs` is a much
smaller example if you just want the basics.
