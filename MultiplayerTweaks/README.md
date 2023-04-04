# Venture Multiplayer Tweaks

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Commonly requested tweaks for servers and single player modes. Toggle the Valkyrie, track Hugin tutorials, customize arrival messages, hide the Haldor map pin, PVP options, and more!

## Features

* Set the maximum player count for servers.
* Set the default spawn point for all Players.
* Toggle Valkyrie opening cut scene.
* Add Hugin tutorials to the seen list when tutorials are turned off.
* Toggle or customize the "I have arrived!" message on new player connection.
* Toggle the Haldor trader Map Pin.
* Toggle Player Map Position Icons always on or off.
* Toggle PVP always on or off.
* Toggle teleporting on a PVP death.
* Toggle skill loss on a PVP death.
* ServerSync included.

### Player Default Spawn Point

Set the PlayerDefaultSpawnPoint by entering an x,y,z pair (for example: 20.5, 10, -80.7), or leave this config blank to use the default start location. To get your player's current position in game you can use the vanilla "pos" command, which will return your position in an x,y,z format. This is the location the game will use when spawning new players, or respawning dead players with no bed point.

### Hugin Tutorials

The base game added a setting to toggle Hugin tutorials on or off under the misc category. This mod will now use the game setting rather than the old config setting. However, since it is not a feature in the base game, this mod will still add all discovered tutorials to the player's "seen" list when tutorials are disabled. If the tutorials at the beginning of the game annoy you, turn off tutorials until you need to see any new tutorials your character discovers. There will not be a flood of tutorials to shift through when you finally need them again!

### I Have Arrived!

This mod gives you the ability to customize the arrival message, or turn it off completely. Change the default vanilla "shout" to a normal message (UseArrivalShout = false) to get rid of the arrival pings but still use the login message. Set your own message string with OverrideArrivalMessage to change the arrival message for all players.

### Haldor Map Pin

This mod gives you the ability to block the Haldor trader map pin from showing on the map. This feature is flexible and can be disabled or enabled at any time. If disabled (EnableHaldorMapPin = false) the pin will not show for all players when the Haldor trader is discovered. Useful for keeping the location of Haldor a secret until you choose to reveal it!

### Player Map Pin Position

Force player map position icons always on or always off. To use set OverridePlayerMapPins to True. Disables the toggle in the minimap.

### Player VS Player

Gives you the ability to force PVP always on or always off (OverridePlayerPVP must be set to true, then uses the value of ForcePlayerPVPOn). Disables the toggle in the UI. Live changes to the configs will not apply until a player respawns/relogs. Remember if PVP is off you cannot kill tames without the butcher knife! There is an option to prevent skill loss caused by a PvP death to incentivize players to fight each other on PvP focused servers. You also have the option to turn off teleporting to your bed (or default spawn point) when another player kills you (set TeleportOnPVPDeath to false). Useful for roleplaying situations. Playing with friends and one of them is about to die? Just slap them across the face to pull them back into reality, don't let their spirit wander, the time to fight is here and now!

## Installation

This mod needs to be on both the client and server for all features to work. When this mod is put on a server it will sync the configurations from the server to all clients on connection. Live changes to the configurations will not always take effect until the player relogs into the world/server.

## Changelog

### 0.4.8

* Changed the format for the PlayerDefaultSpawnPoint config from x,z to x,y,z to fix an issue where the ground height could not always be determined, thus defaulting to using the standing stones spawn area. Please update this config to use the new system for best results.
* Removed an unnecessary minimap patch for forcing player map positions to improve compatibility with other mods.

### 0.4.7

* Update for game patch 0.214.2: "I have arrived" message is now localized in the vanilla game.
* Added option to prevent skill loss on a PvP death.

### 0.4.6

* Actually fixed the bug where on join there would be spammed "I have arrived" messages

### 0.4.5

* Fixed a bug where on join there would be spammed "I have arrived" messages
* Updated the Hugin tutorials to use the new in-game setting for game patch 0.213.4

### 0.4.4

* Fixed the previous patch throwing errors (I must have tested the wrong file, sorry about that)
* Changed the get default spawn point logic to using the original spawn location on failure to find the custom spawn point.
* Changed the internal return type of getting the custom maximum players (might have been causing issues?)

### 0.4.3

* Fix for last player hit not resetting after death so an aoe death would count as a pvp death
* Patch for a transpiler method replacement not matching the original return type (might have caused problems with player respawn?)

### 0.4.2

* Added ability to prevent teleporting on a PVP death
* Bug fix for the PVP toggle not working when the override was disabled

### 0.4.1

* Fixed a compatibility issue with other mods that patch the maximum player count. You should no longer see errors, but if using another mod that patches player count you may get unexpected results (OdinsQOL, Valheim Plus, MaxPlayerCount, etc).

### 0.4.0

* Reworked/Reworded all of the configs, changed the default settings to match vanilla. You will have to update your config file!! Delete the old one and generate a new one by launching the game once.
* Added ability to set the default spawn point of Players.
* Added ability to enforce PVP on or off.
* Update ServerSync to version 1.13 for game patch 0.212.9

### 0.3.2

* ServerSync patch for game patch 0.211.11.

### 0.3.1

* Fixed a server sync issue for new player map position icons feature not syncing on first connection.

### 0.3.0

* Added ability to force player map position icons on or off.
* Added new config section for map pins.

### 0.2.0

* Update for latest game patch 0.211.7 Crossplay: added back ability to patch number of players on a server.

### 0.1.4

* Temporary update for game patch 0.211.7 Crossplay: removed max player check patch. Reverted ServerSync to 1.6.

### 0.1.3

* Changed how ServerSync project is bundled to fix config not locking.

### 0.1.2

* Updated ServerSync to V1.10. Fixed an issue with Server Sync config not locking.

### 0.1.1

* Assembly update, copy and pasting projects makes mistakes lol

### 0.1.0

* First release

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
