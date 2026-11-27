# Project Name
    Portaland

# Team Members
    Uliana, Cole, James, Kartik

# Project Description

Players should feel the disorientation and wonder of a scientist trapped in a strange, magical new world that goes against everything they thought they knew, and should learn to survive its dangers while exploring its mysteries. Through cataloguing new discoveries, exploring far and wide, and searching for mystical artifacts, the player will move from a newcomer to a foreign land to an expert who finds their way home.

# Existing Project Features

## Movement
    
This prototype demonstrates the player’s movement through walking (forward, backward, left, right), running, jumping, looking/turning, rolling (combat move) and player gravity.

### Keybinds
    Forward (W), Backward (S), Left (A), Right (D), Look (Mouse), Run/Sprint 
    (Shift), Jump (Space), Combat Roll (Q), Crouch (Control)

## Combat

The prototype demonstrates our combat system through simple weapons that deal different levels of damage, rudimentary enemies, simple combat moves like parry/block and roll, and item drops that heal the player stats (in this case, hunger, which can help the player regenerate), demonstrating the combat reward system.

### Keybinds
    Hit (Left Click), Block/Parry (Right Click), Roll (Q), Switch Weapon from Hotbar (1-9)

## Player UI and Living
    
The prototype demonstrates the player’s basic needs through three stat bars in the UI: health, hunger, and stamina. Health is tied to movement, since significant falls deal damage. Hunger decays naturally over time and can be restored by enemy item drops. Stamina decreases with player activity (sprinting and jumping). Hunger is also linked to health: when hunger is empty the player takes damage, and when it’s high enough the player heals.

### Keybinds (DEBUG ONLY)
    Reduce Hunger by 25% (P), Refill Hunger (F)

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

# Our contribution

## Uliana

## Cole

## James

## Kartik (kartikeyapro1)

### Character creation

**Overview:** Pressing New Game takes you to the character creator. You can choose your name, class, keepsake and appearance before starting Main Game. The model updates as you change things, and you can drag it to turn it around. Your class also sets your starting health, stamina and items.

**Third-party content:** The characters, hairstyles and clothes are from Quaternius' Universal Base Characters and Modular Character Outfits – Fantasy packs, under CC0. The [asset credits](Assets/ThirdParty/Quaternius/README.md) and [source links](docs/character-animation/asset-selection.md) have the details.

**My contribution:** I worked on the creator, switching appearance options, saving the choices and loading the character into the game with the right stats and equipment. I used AI for help with some of this, as explained below. There is only one face and two outfits in the packs we're using, so the options are limited to those. The colour options tint the textures, which means they can't make them lighter. More detail is in the [character creator notes](docs/character-animation/character-creator.md).

#### AI assistance

I used Claude Code in VS Code with Claude Opus 5.5 on medium for help with the mesh preparation and loading the selected character into gameplay. I struggled with those parts and copied the generated snippets into the project.

In `Assets/Editor/PrepareRangerCharacters.cs`, I used it to help keep just the head from the base mesh and get the clothes working on the same skeleton, including the bone mapping and bind poses. In `Assets/Scripts/Character/PlayerCharacterLook.cs`, it helped with loading the new model first, pointing the animation scripts at it and waiting for the inventory before giving items. I copied those marked snippets from its answers and added them to the project.

I've put the prompts and explanations next to the snippets and checked the wording against my Claude Code messages. I'm responsible for these parts in the group and for understanding how they work.

### Player characters and animation

**Overview:** There are male and female player models with movement and combat animations. Weapons sit in the character's hand and move with the animations.

**Third-party content:** The models, clothes, animation clips and medieval weapons are from Quaternius. See the [character and animation credits](Assets/ThirdParty/Quaternius/README.md) and [weapon credits](Assets/ThirdParty/Quaternius/MedievalWeapons/README.md).

**My contribution:** I put the player prefabs together, worked on the preparation tools and animation setup, and connected the animations and held weapons to the player. The models and animation clips themselves came from the packs. The [animation notes](docs/character-animation/README.md) and [character preparation notes](docs/character-animation/character-preparation.md) explain the setup and checks.

### Combat and vampire boss

**Overview:** Combat includes light and heavy attacks, weapon arts, blocking, timed parries, stagger, ripostes, backstabs and lock-on. The vampire in the church has a boss health bar, music and barriers that close the doors during the fight.

**Third-party content:** The animations and weapons are from Quaternius. The vampire is “Vampire Rigged 3D” by Zain.2407 on Sketchfab, under CC BY 4.0; its [credits](Assets/ThirdParty/Sketchfab/Vampire/README.md) list the source and changes. Sounds and music come from Kenney, artisticdude, Ogrebane and Juhani Junkala; see the [audio credits](Assets/ThirdParty/Audio/README.md).

**My contribution:** I started with health, training dummies, weapon stats and attacks in the prototype, then added blocking, parrying and rolling. Later I worked on the different attacks, lock-on, combat animations, enemies, the vampire fight and combat sounds. These connect to the inventory and pickup work done by other group members. The [combat guide](docs/combat/README.md) and [boss notes](docs/combat/boss-fight.md) explain how to use and change them.

The vampire sometimes slides when chasing, as mentioned in the combat notes. Ripostes and backstabs use normal attacks for now. Still need to check these in the latest build.
