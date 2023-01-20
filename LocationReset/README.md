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

If you want locations to reset even if players have built/died around the entrance set the SkipPlayerGroundPieceCheck config to true. This will change the logic to check only for activity inside the location.

ServerSync is included with this mod.

### Locations Supported

* Troll Caves
* Burial Chambers
* Sunken Crypts
* Frost Caves
* Infested Mines

### Possible Future Improvements

* Adding support for resetting locations found on the "ground"
* Manual reset commands

## Changelog

### 0.1.1

* Added a new config option SkipPlayerGroundPieceCheck

### 0.1.0

* First release

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
