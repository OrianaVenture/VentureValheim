## 0.11.0

* Added new config for hiding steam/xbox platform tags from shouts and chat messages.

## 0.10.0

* Added new config to offset the display day for new day banner message and potentially other mods.

## 0.9.0

* Added feature to only send boss message banners if in 100m range of the boss.

## 0.8.0

* Removed the maximum player count feature from this mod to improve compatibility with other mods like Valheim Plus.
* If looking for a replacement use MaxPlayerCount: https://valheim.thunderstore.io/package/Azumatt/MaxPlayerCount/

## 0.7.1

* Added new configuration option SkillLossOnAnyDeath to prevent losing skills on any death.
* Added compatibility with vanilla hardcore death penalty setting for skill loss on death features.

## 0.7.0

* Added new configuration options:
  * EnableTempleMapPin - Hide Start Temple map pin.
  * TeleportOnAnyDeath - Prevent players from teleporting away from their graves for any cause of death.
  * AllowMapPings - Toggle ability for players to ping the map.
  * AllowShoutPings - Toggle map pings for shout messages.
  * AdminBypass - When true allows admins to bypass much of the mod settings.
* Update for game patch 0.217.22: Fix for preventing teleporting on PVP death not working anymore.
* Bug fix for disabling skill drain on PvP death only working when teleporting was also disabled.
* Added 15 second grace window to player respawn to help prevent death loops.

## 0.6.0

* Added new configuration option to hide Hildir map pin.
* Added Jotunn library as new dependency for config syncing, you now must also install Jotunn for this mod to work.

## 0.5.0

* Update for game patch 0.216.9
* Removed the ModEnabled config since it doesn't really do anything important.

## 0.4.0 - 0.4.8

* Reworked/Reworded all of the configs, changed the default settings to match vanilla.
* Added ability to set the default spawn point of Players.
* Added ability to enforce PVP on or off.
* Added ability to prevent teleporting on a PVP death
* Updated the Hugin tutorials to use the new in-game setting for game patch 0.213.4
* Added option to prevent skill loss on a PvP death.
* Changed the format for the PlayerDefaultSpawnPoint config from x,z to x,y,z to fix an issue where the ground height could not always be determined, thus defaulting to using the standing stones spawn area. Please update this config to use the new system for best results.

## 0.3.0 - 0.3.2

* Added ability to force player map position icons on or off.
* Added new config section for map pins.

## 0.2.0

* Update for latest game patch 0.211.7 Crossplay: added back ability to patch number of players on a server.

## 0.1.0 - 0.1.4

* First release versions pre-0.211.7.

See all patch notes on Github.