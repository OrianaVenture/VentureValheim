# Venture Multiplayer Tweaks

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Commonly requested tweaks for servers and single player modes. Toggle the Valkyrie opening, track Hugin tutorials, customize arrival messages, hide the Haldor/Hildir/Temple map pins, PVP options, disable pings, and more!

## Features

* Set the maximum player count for servers.
* Set the default spawn point for all players.
* Toggle Valkyrie opening cut scene.
* Add Hugin tutorials to the seen list when tutorials are turned off.
* Toggle or customize the "I have arrived!" message on new player connection.
* Toggle the visibility of the Haldor, Hildir, and Start Temple Map Pins.
* Toggle player Map Position Icons always on or off.
* Toggle PVP always on or off.
* Toggle teleporting on death, or only on PVP deaths.
* Toggle skill loss on a PVP death.
* Toggle player ability to ping the map

Admins have the ability to bypass some of these settings when the AdminBypass config is enabled.

### Player Default Spawn Point

Set the PlayerDefaultSpawnPoint by entering an x,y,z pair (for example: 20.5, 10, -80.7), or leave this config blank to use the default start location. To get your player's current position in game you can use the vanilla "pos" command, which will return your position in an x,y,z format. This is the location the game will use when spawning new players, or respawning dead players with no bed point.

### Hugin Tutorials

The base game added a setting to toggle Hugin tutorials on or off under the misc category. This mod will now use the game setting rather than the old config setting. However, since it is not a feature in the base game, this mod will still add all discovered tutorials to the player's "seen" list when tutorials are disabled. If the tutorials at the beginning of the game annoy you, turn off tutorials until you need to see any new tutorials your character discovers. There will not be a flood of tutorials to shift through when you finally need them again!

### I Have Arrived!

This mod gives you the ability to customize the arrival message, or turn it off completely. Change the default vanilla "shout" to a normal message (UseArrivalShout = false) to get rid of the arrival shout and ping but still use the login message. Set your own message string with OverrideArrivalMessage to change the arrival message for all players. When disabling all shout pings (AllowShoutPings = false) it will also apply to this arrival message.

### Haldor/Hildir Map Pins

This mod gives you the ability to block the trader map pins from showing on the map. This feature is flexible and can be disabled or enabled at any time. If disabled (EnableHaldorMapPin = false) the pin will not show for all players when the Haldor trader is discovered. Useful for keeping the location of Haldor a secret until you choose to reveal it! Similar config for Hildir and Start Temple pins included. Admins are able to bypass these settings.

### Player Map Pin Position

Force player map position icons always on or always off. To use set OverridePlayerMapPins to True. Disables the toggle in the minimap. Admins are able to bypass this setting.

### Map Pings

There are two ways the player can ping the map: Shouting and Pinging. There are two configuration options to disable these pings respectively: AllowShoutPings, AllowMapPings. Admins are able to bypass these settings and will be able to send map pings to players and view shout pings.

This feature should be compatible with the Groups mod, and will allow group pings and respect the settings from that mod.

### Player VS Player

Gives you the ability to force PVP always on or always off (OverridePlayerPVP must be set to true, then uses the value of ForcePlayerPVPOn). Disables the toggle in the UI. Live changes to the configs will not apply until a player respawns/relogs. Remember if PVP is off you cannot kill tames without the butcher knife!

There is an option to prevent skill loss caused by a PvP death to incentivize players to fight each other on PvP focused servers. You also have the option to turn off teleporting to your bed (or default spawn point) when another player kills you (set TeleportOnPVPDeath to false). Useful for roleplaying situations. Playing with friends and one of them is about to die? Just slap them across the face to pull them back into reality, don't let their spirit wander, the time to fight is here and now!

Admins are able to bypass the PvP always on or always off setting.

### Death Teleporting and Grace Period

Similarly to the TeleportOnPVPDeath option, there is a config for disabling teleporting on death entirely regardless of how the player died (TeleportOnAnyDeath = false). Be warned, if players die in a tight situation they may get stuck in a death loop and will get very angry at your for turning these settings on. There is a short immunity window to help mitigate death loops set to 15 seconds after respawn. This should give players some time to try to escape repeated deaths (or at least get farther away from the scene). If the player receives damage that drains their health to 0 during this grace period they will not die until the grace period ends. This grace period is always active regardless of mod settings.

## Installation

This mod needs to be on both the client and server for all features to work. Config Syncing is included with Jotunn. Install on the server to enforce the same mod configuration for all players. Live changes to the configurations will not always take effect until the player relogs into the world/server.

## Changelog

### 0.7.0

* Added new configuration options:
  * EnableTempleMapPin - Hide Start Temple map pin
  * TeleportOnAnyDeath - Prevent players from teleporting away from their graves for any cause of death
  * AllowMapPings - Toggle ability for players to ping the map
  * AllowShoutPings - Toggle map pings for shout messages
  * AdminBypass - When true allows admins to bypass much of the mod settings
* Update for game patch 0.217.22: Fix for preventing teleporting on PVP death not working anymore
* Bug fix for disabling skill drain on PvP death only working when teleporting was also disabled
* Added 15 second grace window to player respawn to help prevent death loops

### 0.6.0

* Added new configuration option to hide Hildir map pin.
* Added Jotunn library as new dependency for config syncing, you now must also install Jotunn for this mod to work

### 0.5.0

* Update for game patch 0.216.9
* Removed the ModEnabled config since it doesn't really do anything important

### 0.4.0 - 0.4.8

* Reworked/Reworded all of the configs, changed the default settings to match vanilla.
* Added ability to set the default spawn point of Players.
* Added ability to enforce PVP on or off.
* Added ability to prevent teleporting on a PVP death
* Updated the Hugin tutorials to use the new in-game setting for game patch 0.213.4
* Added option to prevent skill loss on a PvP death.
* Changed the format for the PlayerDefaultSpawnPoint config from x,z to x,y,z to fix an issue where the ground height could not always be determined, thus defaulting to using the standing stones spawn area. Please update this config to use the new system for best results.

### 0.3.0 - 0.3.2

* Added ability to force player map position icons on or off.
* Added new config section for map pins.

### 0.2.0

* Update for latest game patch 0.211.7 Crossplay: added back ability to patch number of players on a server.

### 0.1.0 - 0.1.4

* First release versions pre-0.211.7.

See all patch notes on Github.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
