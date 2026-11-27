# Project Name
    Portaland

# Team Members
    Uliana, Cole, James, Kartik

# Project Description

Players should feel the disorientation and wonder of a scientist trapped in a strange, magical new world that goes against everything they thought they knew, and should learn to survive its dangers while exploring its mysteries. Through cataloguing new discoveries, exploring far and wide, and searching for mystical artifacts, the player will move from a newcomer to a foreign land to an expert who finds their way home.

# Getting Started

# Default Controls & Keybinds

# Features

## Movement

### Overview
    
Describe the feature here

### Third-Party Content

List any assets below, in the following format:

**Asset:** Asset Name by Asset Author (link)
* **Used For:** Describe what the asset was used for.

### Our Contribution

Describe what we did, specifically. Use script names, etc.

## Combat

### Overview
    
Describe the feature here

### Third-Party Content

List any assets below, in the following format:

**Asset:** Asset Name by Asset Author (link)
* **Used For:** Describe what the asset was used for.

### Our Contribution

Describe what we did, specifically. Use script names, etc.

## Player UI
    
### Overview
    
Describe the feature here

### Third-Party Content

List any assets below, in the following format:

**Asset:** Asset Name by Asset Author (link)
* **Used For:** Describe what the asset was used for.

### Our Contribution

Describe what we did, specifically. Use script names, etc.

## Player Living (Health & Statistics)

### Overview
    
Describe the feature here

### Third-Party Content

List any assets below, in the following format:

**Asset:** Asset Name by Asset Author (link)
* **Used For:** Describe what the asset was used for.

### Our Contribution

Describe what we did, specifically. Use script names, etc.

## Inventory

### Overview
    
Describe the feature here

### Third-Party Content

List any assets below, in the following format:

**Asset:** Asset Name by Asset Author (link)
* **Used For:** Describe what the asset was used for.

### Our Contribution

Describe what we did, specifically. Use script names, etc.

## Storyline & NPCs

### Overview
    
Describe the feature here

### Third-Party Content

List any assets below, in the following format:

**Asset:** Asset Name by Asset Author (link)
* **Used For:** Describe what the asset was used for.

### Our Contribution

Describe what we did, specifically. Use script names, etc.

## Terrain & Physical Environment

### Overview

The player spawns at a mysterious teleportation monument within sight of a village. The village has several buildings and NPCs, including a blacksmith's shop where the player can start their first quest.

A path leads from the village to a lake and a small lakeside settlement, where a family lives and farms. Along the way, the forest gets denser and the trees more varied. At the settlement, a father asks the player to help save his daughter.

Past the settlement, the path continues to an old church. The landscape briefly shifts back to the open forest around the starting village, then returns to the denser woods. At the church, the player faces the area's final boss, a vampire, to rescue the daughter.

Players who explore the woods can find a hidden cabin, home to a potion maker NPC.

The environment is built with terrain stamps, textures, materials, trees, grass, bushes and rocks from various asset packs.

### Third-Party Content

**Asset:** Terrain Asset Pack | Terrain Sample by Unity Technologies (https://assetstore.unity.com/packages/3d/environments/landscapes/terrain-asset-pack-terrain-sample-145808)
* **Used For**: Terrain Tools, Terrain Stamps and Brushes, Terrain Textures, Terrain Vegetation

**Asset:** Free Medieval Village Pack Vol.1 by MyDreamGameStudio (https://assetstore.unity.com/packages/3d/environments/fantasy/free-medieval-village-pack-vol-1-374316)
* **Used For**: Village content including buildings and some props.

**Asset:** Tree Collection Pack 2017 by ALIyerEdon (https://assetstore.unity.com/packages/3d/vegetation/trees/tree-collection-pack-2017-76974#content)
* **Used For:** Two tree prefabs used in the Village biome, grass used throughout.

**Asset:** Blacksmith by 3DForge (https://assetstore.unity.com/packages/3d/environments/fantasy/blacksmith-46265)
* **Used For:** Buildings, fences, and stables at the lakeside settlement.

**Asset:** Procedural Water Shader by Pedro Verpha (https://assetstore.unity.com/packages/vfx/shaders/procedural-water-shader-353486)
* **Used For:** Water texturing for Lake

**Asset:** Flooded Grounds by Sandro T (https://assetstore.unity.com/packages/3d/environments/flooded-grounds-48529)
* **Used For:** Trees and vegetation in Lake environment and Church environment, Church building & props, Potionmaker Hut building and props.

**Asset:** Altar Ruins Free by Underhill Labz (https://assetstore.unity.com/packages/3d/environments/fantasy/altar-ruins-free-109065)
* **Used For:** Teleportation Monument

### Our Contribution

The environment's raw materials (terrain textures, stamps, trees, grass, rocks and building models) come from third-party asset packs. The environment itself was built by hand. We did not import a pre-made scene or model. All of the following was done by us:

- **Terrain:** sculpted the height map and shaped hills and valleys with brushes and stamps
- **Texturing:** painted ground textures by hand to define paths, clearings and the two biomes
- **Layout:** placed every building, and designed the village, lakeside settlement, church and cabin to fit the story
- **Paths:** built the routes that connect each location and guide the player through the quest
- **Water:** created and shaped the lake
- **Vegetation:** placed the trees, bushes, grass and rocks, varying how dense they are to mark the change between biomes

In short, the assets are the building blocks and the world is our design.

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
