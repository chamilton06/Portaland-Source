# Project Name
    Portaland

# Team Members
    Uliana, Cole, James, Kartik

# Project Description
    Players should feel the disorientation and wonder of a scientist trapped in
    a strange, magical new world that goes against everything they thought they
    knew, and should learn to survive its dangers while exploring its mysteries.
    Through cataloguing new discoveries, exploring far and wide, and searching
    for mystical artifacts, the player will move from a newcomer to a foreign
    land to an expert who finds their way home.

# Existing Project Features

## Movement

    This prototype demonstrates the player’s movement through walking (forward, backward, left, right), running, jumping, looking/turning, rolling (combat move) and player gravity.

### Keybinds
    Forward (W), Backward (S), Left (A), Right (D), Look (Mouse), Run/Sprint 
    (Shift), Jump (Space), Combat Roll (Q)

## Combat

The prototype demonstrates our combat system through simple weapons that deal different levels of damage, rudimentary enemies, simple combat moves like parry/block and roll, and item drops that heal the player stats (in this case, hunger, which can help the player regenerate), demonstrating the combat reward system.

### Keybinds
    Hit (Left Click), Block/Parry (Right Click), Roll (Q), Switch Weapon from Hotbar (1-9)

## Player UI and Living
    The prototype demonstrates the player’s basic needs through three stat bars in the UI: health, hunger, and stamina. Health is tied to movement, since significant falls deal damage. Hunger decays naturally over time and can be restored by enemy item drops. Stamina decreases with player activity (sprinting and jumping). Hunger is also linked to health: when hunger is empty the player takes damage, and when it’s high enough the player heals.

### Keybinds (DEBUG ONLY)
    Reduce Hunger by 25% (P)

## Inventory

    The prototype includes a simple inventory with a main inventory that opens with a specific key, a hotbar that persists whether or not the main inventory is open, and the ability to select/use and equip items, which take up different amounts of space.

### Keybinds
    Open/Close Inventory (Tab), Add Axe Item (E, Debug Only), Add Sword Item (R, Debug Only)

## Exploration

    The prototype includes two additional features: a map and a journal. The map opens with a key (M) and will let the player mark explored structures, monuments, and artifacts. The journal lives within the inventory, where items and eventually species and more can be catalogued.

### Keybinds
    Open Journal (J), Open Map (M)

## Environment
    The prototype displays a simple environment in which the player’s movement system is demonstrated.