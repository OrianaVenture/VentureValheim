# Incognito Mode

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Roleplaying mod that changes the Player display name if their face is hidden by certain helmet, shoulder, or utility items. Configurable.

## Features

This is a roleplaying mod aimed at making in-game experiences more realistic. Come across a player in a full padded armor set? Well I wouldn't be able to tell who they are either! Configure the display name of these mystery characters with the HiddenDisplayName config, and pick which items can hide a player name with HiddenByItems config. Players will have to take off their items to reveal their identities! Good luck knowing who robbed you!

Currently, equipped helmet, shoulder, and utility items are supported. Will work with custom items added by other mods given they are of one of these categories.

## Installation

All players will have to have this mod installed for hidden names to work 100% of the time. Any player who does not have this installed will not have their name hidden (even for players with the mod installed).

This mod needs to be on all clients to work properly. Config Syncing is included with Jotunn. Install on the server to enforce the same mod configuration for all players.

### Vanilla Helmet Prefabs

* HelmetLeather
* HelmetBronze
* HelmetTrollLeather
* HelmetIron
* HelmetRoot
* HelmetFenring
* HelmetDrake
* HelmetPadded
* HelmetMage
* HelmetCarapace

## Changelog

### 0.3.0

* Added Jotunn library as new dependency for config syncing, you now must also install Jotunn for this mod to work

### 0.2.0

* Update for game patch 0.216.9
* Bug fix for name not hiding correctly in the chat introduced in the 0.214.2 game patch
* Added support for shoulder and utility items being able to hide names

### 0.1.2

* Changed the priority of patching the chat name to work with the "me" mod

### 0.1.1

* Added the "hidden" effect to apply to the chat name too

### 0.1.0

* First release

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
