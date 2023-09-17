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

* Two locations very close together can delete parts of the other during regeneration.
* Resetting ground locations can potentially remove pickables around a location that are not respawned like stones, branches, or berry bushes.
* If you move very fast (or admin fly) through an area you may see locations change as they regenerate, especially noticeable in the plains. If you move too fast through an area while triggering a reset you can cause item duplication.
* Some items are not respawned like black forest barrels, boulders, and mistlands giant mineables. I could not find an easy way to identify these items without also destroying trees and rocks.
* Item duplication should not happen, but is possible if timing conditions are right. If you can consistency reproduce duplication issues please report the problem.
* When a new zone or location is loaded there is an expected small lag spike. This happens in vanilla already, but might also be noticeable when this mod is performing a reset.

### Other Mod Support

The following locations from other mods are supported:

* Monsterlabz: SpiderCave01, AshlandsCave_01, AshlandsCave_02
* Horem: Loc_MistlandsCave_DoD

### Possible Future Improvements

* Manual reset commands
* Finding a way to reset items listed in limitations
* Support for other mods (by request)

## Installation

This mod needs to be on the client, it will work even if other players do not have it installed but may behave unexpectedly when playing around other players without the mod. For best results have everyone install the mod. When this mod is put on a server it will sync the configurations from the server to all clients on connection. Live changes to the configurations should take immediate effect.

## Changelog

### 0.5.1

* Fixed configs not selecting the correct reset times and just using default
* Added advanced configs for new Hildir's Request dungeons

### 0.5.0

* Update for game patch version 0.217.14 (Hildir's Request)
* Internal code refactor to simplify resetting, now auto-detects dungeons and all reset tracking moved to LocationProxy object
  * Improved compatibility for custom dungeons from other mods (does not apply for non-dungeon sky locations)
  * You will lose reset times on all dungeons when upgrading, they will be treated as if the mod was newly installed
* Bug fix for frost caves and mistlands dungeons regenerating with different seeds
* Reworked resetting logic of leviathans to only happen if the player is the "chunk owner" (similar update happened in 0.3.1)
* Added ItemDrop to recognized type for resets, this will now delete and respawn things you can throw on the ground
* Extended the range of triggering resets to 100 meters for all locations and dungeons (sky locations previously 30)

### 0.4.0

* Update for game patch 0.216.9
* Bug fix for multiple leviathans spawning in the same place in multiplayer (needs extra testing)
* Bug fix for ground locations not detecting player built pieces correctly (incorrect radius check)

### 0.3.1

* Reworked resetting logic of dungeons and locations to only happen if the player is the "chunk owner". Should fix a bug where reset times were not being recorded correctly and caused multiple resets to happen in a short amount of time
* Added a redundant error check to ensure a reset does not happen unless the reset time is set properly

### 0.3.0

* Small optimization to the reset timing for dungeons and locations, now checks the area is ready before performing a reset - should help reduce small duplication errors
* Major addition of Leviathan Resetting with new configs! (defaults to on! be aware when you upgrade the mod)

### 0.2.3

* Changed how zone centers of locations are determined to improve accuracy of location deletion
* Removed the ModEnabled config since it doesn't really do anything important

### 0.2.2

* Fixed issue with multiplayer sessions triggering multiple resets due to reset day not being recorded correctly, plus more internal error checking
* Changed how zone sizes of locations are calculated to improve accuracy of location deletion
* Added resetting for doors that require keys to open (sunken crypts and citadel, should support custom content)
* Added missing wear and tear damage to resetting dungeons

### 0.2.1

* Added mod support for Monsterlabz and Horem's locations.

### 0.2.0

* Added support for resetting all kinds of ground locations including villages and fuling camps
* New config option to toggle resetting ground locations (defaults to on! be aware when you upgrade the mod)
* New config options for specifying reset times for different dungeons

### 0.1.2

* Patch for object deletion to hopefully solve a multiplayer deletion issue.

### 0.1.1

* Added a new config option SkipPlayerGroundPieceCheck
* Please note I forgot to bump the version number for this release.

### 0.1.0

* First release

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
