# Venture Location Reset

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Automatically reset Dungeons, Locations, and Leviathans with a customizable in-game day rate. Inspired by DungeonReset mod by Tekla.

## Features

Resets dungeons/locations when a player approaches them given the player is hosting the chunk and there is no "player activity" nearby. Locations will not reset until the second time it is visited with this mod installed given the ResetTime has been reached. For example, if you install this mod then visit a burial chamber on Day 100 it will not reset until visited on or after day 130 (given the default reset time is 30). Similarly, this mod has the ability to reset Leviathans in their original spawn locations.

Player activity includes:

* A Player has built anything near the entrance to or inside the location
* There is a Tombstone near the entrance to or inside the location
* There is a Player inside the location

There are advanced options in the config file to set individual reset times for certain locations. To use the advanced options set OverrideResetTimes to true, you must then customize all overridden values. Any locations not specified in the config will use the default value and cannot be changed individually. If you do not want specific locations to reset you can set the reset time to an arbitrarily large value like 100000, or any value that will be greater than the number of expected passed in-game days.

### Console Commands

To perform a manual reset use the "resetlocations" command. To specify a range use a whole number: "resetlocations 10". Maximum range for the manual reset command is 100 and defaults to 20. Manual resets will ignore time and Player activity restrictions specified above. This can cause loss of player built structures and tombstones. PLEASE USE WITH CAUTION.

Don't know how to use commands? Dedicated servers do not allow for use of commands, but there are mods that can enable them (like Server devcommands by JereKuusela). The command added by this mod is considered a "cheat". To use cheats you must enable them with the "devcommands" command, you may have to be an admin for them to work depending on what mod you use to access commands.

### Locations Supported

"Sky locations" are any dungeon or location that is generated suspended in the sky in the game. If the location has a teleporting feature it probably is located in the sky. The following are considered "sky locations":

* Troll Caves
* Burial Chambers
* Sunken Crypts
* Frost Caves
* Infested Mines/Citadel

"Ground locations" are every other type of location in the game. This mod supports resetting all kinds of ground locations including abandoned buildings, shipwrecks, infested trees, tar pits, etc. If you do not want to reset ground locations set ResetGroundLocations to False. This will not apply for meadows farms/villages or fuling camps as those locations are considered dungeons and are in a separate category.

If you are using another mod that adds custom locations or dungeons you may see this mod behave unexpectedly. If you would like support added for another mod please reach out to me in my discord (link below).

### Skip Player Ground Piece Check

If you want "sky locations" to reset even if players have built/died around the entrance set the SkipPlayerGroundPieceCheck config to true. This will change the logic to check only for activity inside the sky location. This check will always occur for ground locations, you cannot disable it for them. The check for player activity is dependant on the size of the location, bigger dungeons will have a wider range for the activity check. If you suspect the mod is not working as intended it might be due to this player activity check, test the reset on a new area where no player has built to ensure the mod is working as intended.

<details close>
<summary>Expand/Collapse Hildir Note (Spoilers!)</summary>

You may notice that Sealed Towers (Hildir plains dungeon) are not resetting. Since you must build to enter the tower it is very likely the mod is detecting your player placed pieces and is refusing to reset. Your placed pieces must be about 16 meters away from the tower itself, or about 8 wooden walls length. If you do not see a log line like "Done regenerating location Hildir_plainsfortress ..." then it did not reset. Turn on bepinex debug logs to see more detailed information.

</details>

### Leviathans

Leviathans will respawn in their original locations when resetting is enabled given there are none found in the zone. When causing a Leviathan to dive after mining the "leave" time will be recorded as the "visited" day, and the Leviathan will not delete itself as it does in vanilla. Upon reloading a zone the Leviathan will appear again on the surface unchanged from when last visited. Once a reset time has been reached the leviathan will delete itself upon the next time it is loaded. Currently you must reload the zone after a Leviathan is deleted to get them to respawn.

This feature may impact performance more so than resetting dungeons and Locations. If you have performance issues with this mod consider disabling Leviathan resetting when you do not need it on.

### Limitations

Due to loading times and the very random nature of the world spawning system there may be cases where this mod behaves strangely.

* Two locations spawned on top of each other will delete parts of the other during regeneration.
* Resets can potentially remove some things around the ground of the location that are not respawned like berry bushes, logs, and large rocks.
* Item duplication should not happen, but is possible if timing conditions are right. If you can consistency reproduce duplication issues please report the problem.
* If you move very fast (or admin fly) through an area you may see locations change as they regenerate, especially noticeable in the plains. If you move too fast through an area while triggering a reset you can cause item duplication.
* When a new zone or location is loaded there is an expected small lag spike. This happens in vanilla already, but might also be noticeable when this mod is performing a reset.
* Dungeons with radial camps (like Fuling camps) will be randomized every reset due to the generation algorithm.

### Other Mod Support

If set up correctly by the mod author this mod should successfully regenerate all custom locations and dungeons. There are cases where this mod may behave strangely. Please report any issues you may encounter when using custom locations.

The following locations from other mods are excluded from resetting to prevent issues:

* Monsterlabz: Mystical_Well0
* Wayshrine by Azumatt: Wayshrine, Wayshrine_Ashlands, Wayshrine_Frost, Wayshrine_Plains, Wayshrine_Skull, Wayshrine_Skull_2

When using the mod "Dungeon Splitter" by JereKuusela all sky location resetting will be disabled for compatibility reasons. If you still wish to reset sky locations you will have to use Jere's server side mods for that. Resetting leviathans and ground locations will still work as expected.

## Installation

This mod needs to be on the client, it will work even if other players do not have it installed but may behave unexpectedly when playing around other players without the mod. For best results have everyone install the mod. Config Syncing is included with Jotunn. Install on the server to enforce the same mod configuration for all players. Live changes to the configurations should take immediate effect.

## Changelog

Moved to new file, it will appear as a new tab on the thunderstore page.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
