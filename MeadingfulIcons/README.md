# Meadingful Icons

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Adds identifiers to the different mead base icons in the game. Increases the maximum stack size of mead bases, configurable.

## Features

This mod adds a small identifier to the existing mead base icon so you can tell them apart. This feature has a configuration option to toggle the icon changes off that is not synced with the server. To set the default to off for your server include the config file with your modpack download.

<img alt="Mead Icons" src="https://github.com/OrianaVenture/VentureValheim/blob/master/MeadingfulIcons/MeadingfulIcons.png?raw=true" />
<br><br>

Additionally adds a configuration for setting the maximum stack size of all mead bases controlled server side. This configuration defaults to 10. If intending to use this mod with vanilla players you must set the stack size to 1 to avoid losing items. Unmodded clients can overwrite your stored item data and reset the stack size to 1, thus losing additional items. Reducing the stack size configuration mid-game can also potentially cause loss of items, please be aware.

## Installation

This mod needs to be on all clients to work properly. Config Syncing is included with Jotunn. Install on the server to enforce the same mod configuration for all players. Live changes to the configurations will not take effect until the player relogs into the world/server.

## Changelog

### 0.1.2

* Fixed issue where patches may have applied before the initial server configuration syncing event. This made some server configurations not apply correctly.
* Code cleanup to remove null-propagating operators & Improved error handling.

### 0.1.1

* Fixed the exception bubbling up when logging out and back in during the same game session.

### 0.1.0

* First release.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).