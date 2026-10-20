movement, stats and ui prototype

open assets/scenes/MovementTesting.unity and press play.

wasd to move, hold shift to sprint, space to jump, hold ctrl to crouch. you can only sprint going forward and not while crouched. sprint jumping goes a bit higher and further than a normal jump. you cant stand up if theres something above your head. the mouse turns the player left and right and tilts the camera up and down, the camera is third person and stays behind the player.

there are 3 bars in the top left. hunger goes down 10% every minute and 3x faster if you sprint. stamina drains 20 a second while sprinting and each jump costs 10, it refills a second after you stop. if stamina runs out you cant sprint again till its back to 20. when hunger hits 0 you start losing 1 health a second, at 0 health you die and cant move. press f to refill hunger, its a debug key for testing till food is added.

press 1 2 3 4 to pick a slot on the hotbar at the bottom, it just highlights for now, equipping items is waiting on the inventory. press i to open the inventory, its empty for now. while its open you cant look around and the mouse is freed to interact with the inventory. 

i'm not sure if thats what everyone was thinking in terms of ui, movement and camera feeling. can be changed after testing. 
