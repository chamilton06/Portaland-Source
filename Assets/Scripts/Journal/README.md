journal, map, item info and death screen

press tab to open the inventory, its closed when you start now. press e for an axe or r for a sword (debug keys), 
then click an item and the inventory slides to the left and an info panel opens on the right. it shows the icon, name, stats and a short description. 
weapons show attack, range and speed, food shows how much health/hunger it gives, and anything with weight or durability shows that too. 

the "journal" button closes the inventory and opens the journal on that item's page.

press j to open the journal, escape or j again closes it. looks like an old book with tabs for notes, enemies, materials, crafting and weapons. 
the left page lists the entries in that tab and the right page shows the picture, title and lore text. 

the game pauses while its open. you can write a proper lore page for an item by making a journal entry asset (right click > create > journal entry) and dragging it into the item's journal entry field. 
if an item doesnt have one, the journal writes a basic page for it automatically from its name, icon and short description. 

new stuff shows up with a red NEW! and a dot on the tab till you click it. untick starts unlocked on an entry if you want it hidden till the player finds the item.

it works with the food that cole added to the enemy. kill the enemy, pick up the food and the popup text shows up at the top saying the item got added to the journal, then its in the materials tab as NEW!. any pickup can do this now, just drag the item asset into the item field on the pickup prefab.

press m to open the map. its a tan paper panel covering most of the screen so you can still see the game behind it, the game doesnt pause so you can walk around and watch the player icon move and turn. the map size comes from the terrain. putting the map marker script on the obstacles object draws the obstacle course on the map (i haven't checnked this tho ^^).

new scripts
- journal/JournalEntry.cs, one journal page (title, tab, picture, lore text)
- journal/JournalUI.cs, the journal screen, tabs, NEW! tags, auto pages and the popup
- ui/ItemInfoPanel.cs, the stats panel next to the inventory and the slide
- ui/MenuState.cs, keeps track of if any menu is open so the mouse gets freed and you cant look around or attack
- ui/DeathScreen.cs, you died screen
- map/MapUI.cs and map/MapMarker.cs, the map, player icon and obstacle course
- inventory/FoodItem.cs, a food item type with health and hunger restore

changes to existing scripts
- Item.cs, added short description, weight, durability and a journal entry link, plus the stats text for the info panel
- Weapon.cs, shows its attack, range and speed in the info panel
- Slot.cs, slots can be clicked now
- Inventory.cs, starts closed, tab uses a new SetOpen, clicking a slot opens the info panel, picked up items get added to the journal
- ItemPickup.cs, has an optional item field that gets added to the journal when picked up
- PlayerCombat.cs, PlayerTurn.cs and CameraFollow.cs, also check MenuState so the journal works like the inventory (no looking or attacking, mouse is free)

still to do
- the death screen script is done but it isnt in the scene yet, ill add it by tomorrow. when its in, health at 0 stops the player and shows "you died", pressing "k" kills you for testing.
- the journal text positioning isnt lining up properly yet, need to fix that.
- when you open the journal or a tab an item shouldn't be selected, right now it doesnt work like that so i need to change the logic.
