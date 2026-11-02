# Portaland

Our Unity survival game for ECS657U at Queen Mary University of London.

## Opening the project

Use **Unity 2023.1.22f1**, as recorded in `ProjectSettings/ProjectVersion.txt`.
In Unity Hub, add this folder as a project. In Unity's Project window, open
`Assets/Scenes/Prototype.unity`. This is the scene inspected for the character work.

## Group notes

- [Character appearance and animation — issue #23](docs/character-animation/README.md): current setup, file locations and planned work.
- [Character shortlist](docs/character-animation/asset-selection.md): options to choose from before importing anything.
- [Art direction and group asset references](docs/character-animation/art-direction.md): the updated Elden Ring influence and possible packs.
- [Character testing checklist](docs/character-animation/testing.md): checks to run before and after integration.
- Existing system notes: [movement](Assets/Scripts/Movement/README.md), [combat](Assets/Scripts/Combat/README.md), [inventory](Assets/Scripts/Inventory/README.md), [journal](Assets/Scripts/Journal/README.md).

Some older system notes describe earlier prototypes, so check the current scene and scripts as well.

Keep shared setup instructions in `docs/`, alongside links to the actual scripts and assets.
When adding Unity assets, include their `.meta` files so references stay the same for everyone.
