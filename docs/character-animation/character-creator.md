# Character creator

When you press New on the main menu you now go to a character creator before the game starts.
It's based on the Elden Ring one. You pick an origin and then how your character looks, and
Begin takes you into Main Game as that character.

## What you can choose

On the Origin page you type a name and pick a class, a keepsake, and your height and build.

The class sets your health and stamina and gives you a starting item. The Knight has 120
health and 90 stamina and starts with a sword, the Ranger has 95 health and 120 stamina and
starts with an axe, and the Wanderer has 105 of both and starts with two healing potions. The
keepsake is one extra thing to start with: two healing potions, five apples or three crafting
herbs. You only get these items once per new game, not every time you respawn.

On the Appearance page you pick male or female, light or dark skin and a skin tint, eye
colour, a hairstyle and hair colour, a beard (male only), and your outfit. The outfit is split
into top, legs and boots, and each one can be Ranger or Peasant, so you can mix them. You can
also put the hood up or down, take the shoulder pads off, pick one of the two colour schemes
and tint the clothes.

Randomise changes your looks but keeps your name, class and size.

## How it works

Begin saves everything to PlayerPrefs. When Main Game loads, the `PlayerCharacterLook`
component on the Player reads it, swaps the default Ranger for your character, sets your
health and stamina for your class and gives you your starting items. If you go straight into
Main Game without making a character, you're just the normal Ranger.

The characters are `PlayerCharacterMale` and `PlayerCharacterFemale` in
`Assets/Art/Characters/Player`. Each one has every outfit piece and hairstyle on it already,
and `CharacterAppearance` turns the right ones on and sets the colours. If you drag one into a
scene you can change its Look in the Inspector and see it update without pressing Play.

The scripts are in `Assets/Scripts/Character`. The class stats and starting items are in
`Origins.cs` if you want to change them.

## Adding it to another scene

Tools → Portaland → Add Combat Player To Open Scene adds `PlayerCharacterLook` to the Player
along with the combat stuff. Main Game already has it.

## Limits

The free Quaternius packs only have one face and two outfits, so that's what we've got for
now. Colour tints can only make things darker or more coloured, not lighter, so you can't get
really pale hair. With light skin, bare arms on the Peasant top use the pack's own skin
texture because there isn't a light version of the body texture.
