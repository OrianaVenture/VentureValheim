# Venture Multiplayer Tweaks

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Commonly requested tweaks for servers and single player modes. Toggle the Valkyrie opening, track Hugin tutorials, customize arrival messages, hide the Haldor/Hildir/Temple map pins, PVP options, disable pings, and more!

## Features

* Set the default spawn point for all players.
* Toggle Valkyrie opening cut scene.
* Add Hugin tutorials to the seen list when tutorials are turned off.
* Toggle or customize the "I have arrived!" message on new player connection.
* Toggle the visibility of the Haldor, Hildir, and Start Temple Map Pins.
* Toggle player Map Position Icons always on or off.
* Toggle PVP always on or off.
* Toggle teleporting on death, or only on PVP deaths.
* Toggle skill loss  on death, or only on PVP deaths.
* Toggle player ability to ping the map.
* Automatic death grace window on player respawn
* Boss message banners only send to players within range
* Ability to offset the game day
* Ability to hide steam/xbox platform tags from shouts and chat messages

Admins have the ability to bypass some of these settings when the AdminBypass config is enabled.

### Player Default Spawn Point

Set the PlayerDefaultSpawnPoint by entering an x,y,z pair (for example: 20.5, 10, -80.7), or leave this config blank to use the default start location. To get your player's current position in game you can use the vanilla "pos" command, which will return your position in an x,y,z format. This is the location the game will use when spawning new players, or respawning dead players with no bed point.

### Hugin Tutorials

The base game added a setting to toggle Hugin tutorials on or off under the misc category. This mod will now use the game setting rather than the old config setting. However, since it is not a feature in the base game, this mod will still add all discovered tutorials to the player's "seen" list when tutorials are disabled. If the tutorials at the beginning of the game annoy you, turn off tutorials until you need to see any new tutorials your character discovers. There will not be a flood of tutorials to shift through when you finally need them again!

### Messages

#### I Have Arrived!

This mod gives you the ability to customize the arrival message, or turn it off completely. Change the default vanilla "shout" to a normal message (UseArrivalShout = false) to get rid of the arrival shout and ping but still use the login message. Set your own message string with OverrideArrivalMessage to change the arrival message for all players. When disabling all shout pings (AllowShoutPings = false) it will also apply to this arrival message.

#### Boss Message Banners

When a boss is summoned, alerted, and/or killed there is a server-wide message banner sent to alert players. This can get very annoying in multiplayer games. This mod changes these messages to only send to players within 100 meters of the boss (or other applicable creature) when the message is normally triggered.

### Haldor/Hildir Map Pins

This mod gives you the ability to block the trader map pins from showing on the map. This feature is flexible and can be disabled or enabled at any time. If disabled (EnableHaldorMapPin = false) the pin will not show for all players when the Haldor trader is discovered. Useful for keeping the location of Haldor a secret until you choose to reveal it! Similar config for Hildir and Start Temple pins included. Admins are able to bypass these settings.

### Player Map Pin Position

Force player map position icons always on or always off. To use set OverridePlayerMapPins to True. Disables the toggle in the minimap. Admins are able to bypass this setting.

### Map Pings

There are two ways the player can ping the map: Shouting and Pinging. There are two configuration options to disable these pings respectively: AllowShoutPings, AllowMapPings. Admins are able to bypass these settings and will be able to send map pings to players and view shout pings.

This feature should be compatible with the Groups mod, and will allow group pings and respect the settings from that mod.

### Player VS Player (PVP)

Gives you the ability to force PVP always on or always off (OverridePlayerPVP must be set to true, then uses the value of ForcePlayerPVPOn). Disables the toggle in the UI. Live changes to the configs will not apply until a player respawns/relogs. Remember if PVP is off you cannot kill tames without the butcher knife! Admins are able to bypass this setting.

### Skill Loss on Death

There are two configs for controlling the skill loss on death vanilla feature when set to False:

* SkillLossOnAnyDeath: For any death.
* SkillLossOnPVPDeath: Only PVP deaths. Incentivize players to fight each other.

These features will work when using the hardcore death penalty world modifier. When using these features the resetcharacter vanilla command may not work as intended.

### Death Teleporting and Grace Period

There are two configs for controlling the teleporting on death vanilla feature to allow respawning directly on your grave rather than a bed or the default spawn point when set to False:

* TeleportOnAnyDeath: For any death.
* TeleportOnPVPDeath: Only PVP deaths. Useful for roleplaying situations.

Be warned, if players die in a tight situation they may get stuck in a death loop and will get very angry at your for turning these settings on. There is a short immunity window to help mitigate death loops set to 15 seconds after respawn. This should give players some time to try to escape repeated deaths (or at least get farther away from the scene). If the player receives damage that drains their health to 0 during this grace period they will not die until the grace period ends. This grace period is always active regardless of mod settings.

### Day Offset

The GameDayOffset config allows you to change the display day without resetting it for the world. Useful if you have spent time manually customizing a map and want to open your server on "day 1". For example, if it is day 233 on your world set GameDayOffset = 232 to set this as "day 1". If you see mod conflicts when using this setting please report it for visibility.

This feature can potentially cause issues with other mods which rely on the same method to get the game day such as Location Reset. If building a server and want to use Location Reset please finish setup before adding that mod to have full compatibility with this feature.

## Installation

This mod needs to be on both the client and server for all features to work. Config Syncing is included with Jotunn. Install on the server to enforce the same mod configuration for all players. Live changes to the configurations will not always take effect until the player relogs into the world/server.

## Changelog

Moved to new file, it will appear as a new tab on the thunderstore page.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
