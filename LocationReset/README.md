# Venture Location Reset

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Automatically reset Dungeons and Locations with a customizable in-game day rate. Inspired by DungeonReset mod by Tekla.

## Features

Resets dungeons/locations when a player approaches them given there is no "player activity" nearby. Locations will not reset until the second time it is visited with this mod installed given the ResetTime has been reached. For example, if you install this mod then visit a burial chamber on Day 100 it will not reset until visited on or after day 130 (given the default reset time is 30).

Player activity includes:

* A Player has built anything near or inside the dungeon
* There is a Tombstone near or inside the dungeon
* There is a Player inside the dungeon

If you want sky locations to reset even if players have built/died around the entrance set the SkipPlayerGroundPieceCheck config to true. This will change the logic to check only for activity inside the sky location. This check will always occur for ground locations, you cannot disable it for them. The check for player activity is dependant on the size of the location, bigger dungeons will have a wider range for the activity check. If you suspect the mod is not working as intended it might be due to this player activity check, test the reset on a new area where no player has built to ensure the mod is working as intended.

There are advanced options in the config file to set individual reset times for certain locations. To use the advanced options set OverrideResetTimes to true, you must then customize all overridden values. Any locations not specified will use the default value and cannot be changed individually. If you do not want specific locations to reset you can set the reset time to an arbitrarily large value like 100000, or any value that will be greater than the number of expected passed in-game days.

### Locations Supported

"Sky locations" are any dungeon or location that is generated suspended in the sky in the game. If the location has a teleporting feature it probably is located in the sky. The following are considered "sky locations":

* Troll Caves
* Burial Chambers
* Sunken Crypts
* Frost Caves
* Infested Mines/Citadel

"Ground locations" are every other type of location in the game. This mod supports resetting all kinds of ground locations including abandoned buildings, shipwrecks, infested trees, tar pits, etc. If you do not want to reset ground locations set ResetGroundLocations to False. This will not apply for meadows farms/villages or fuling camps as those locations are considered dungeons and are in a separate category.

If you are using another mod that adds custom locations or dungeons you may see this mod behave unexpectedly. If you would like support added for another mod please reach out to me in my discord (link below).

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

This mod needs to be on the client, it will work even if other players do not have it installed but may behave unexpectedly when playing around other players without the mod. When this mod is put on a server it will sync the configurations from the server to all clients on connection. Live changes to the configurations should take immediate effect.

## Changelog

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
