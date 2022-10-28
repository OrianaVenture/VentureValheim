# Venture Multiplayer Tweaks

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Commonly requested tweaks for servers and single player modes. Toggle the Valkyrie, Hugin tutorials, arrival messages, Haldor map pin, and more!

## Features

* Ability to set the maximum player count for servers.
* Toggle Valkyrie opening cut scene.
* Toggle Hugin tutorials (client side config).
* Toggle or customize the "I have arrived!" message on new player connection.
* Toggle the Haldor trader Map Pin.
* Force Player Map Pins always on or off.
* ServerSync included.

### Hugin Tutorials

This mod can block Hugin from spawning but will still add all discovered tutorials to the player's "seen" list. If the tutorials at the beginning of the game annoy you - disable this feature (EnableHugin = false). You can turn it back on at any time to see any new tutorials your character discovers. If you do this, a client restart is required to re-enable tutorials.

If you use another mod that relies on Hugin spawns (otherwise known as the Raven), EnableHugin must be true for that mod to work. (If you still want to disable tutorials with my mod I can make an update for compatibility with other mods if you send me all the deets!)

### I Have Arrived!

This mod gives you the ability to customize the arrival message, or turn it off completely. Change the default vanilla "shout" to a normal message (UseArrivalShout = false) to get rid of the arrival pings but still use the login message. Set your own message string with OverrideArrivalMessage to change the arrival message for all players.

### Haldor Map Pin

This mod gives you the ability to block the Haldor trader map pin from showing on the map. This feature is flexible and can be disabled or enabled at any time. If disabled (EnableHaldorMapPin = false) the pin will not show for all players when the Haldor trader is discovered. Useful for keeping the location of Haldor a secret until you choose to reveal it!

### Player Map Pins

Force player map pins always on or always off. To use set OverridePlayerMapPins to True.

## Changelog

### 0.3.2

* ServerSync patch for game patch 0.211.11.

### 0.3.1

* Fixed a server sync issue for new player map pin feature not syncing on first connection.

### 0.3.0

* Upgraded ServerSync to V1.11: Crossplay compatibility upgrade for config sync.

### 0.3.1

* Fixed a server sync issue for new player map pin feature not syncing on first connection.

### 0.3.0

* Added ability to force player map pins on or off.
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
