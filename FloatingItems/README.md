# Venture Floating Items

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Makes previously sinkable items float on water, or floating items sinkable. Configurable.

## Features

Allows you to make items that usually sink the ability to float on water (or tar), and vice versa. To make items sink add their prefab names to the "SinkingItems". To make items float toggle the following categories you want, or add the specific item prefab names to the "FloatingItems" config list:

* FloatEverything: Floats everything except for items listed in the "SinkingItems" config
* FloatTrophies: Will add the floating ability to any item that contains "trophy" in it's prefab name
* FloatMeat: Will add the floating ability to any item that contains "meat" in it's prefab name
* FloatHides: Will add the floating ability to the different hides and fabrics
* FloatGearAndCraftable: Will add the floating ability to any item that contains a recipe (can be crafted by a player), and other gear like items bought from Haldor and meads
* FloatTreasure: Will add the floating ability to Coins, Ruby, Amber, AmberPearl, and SilverNecklace

If an item is included in the "SinkingItems" list it will be excluded from automatic item discovery when turning on the other config toggles. The mod defaults will add SerpentScale to the "FloatingItems" list, and will add BronzeNails and IronNails to the "SinkingItems" list.

## Installation

This mod needs to be on all clients to work properly. Config Syncing is included with Jotunn. Install on the server to enforce the same mod configuration for all players. Live changes to the configurations should take immediate effect but items already on the ground will not update until reloaded or picked up.

## Changelog

### 0.3.2

* Fixed a bug where some items were not applying floating correctly on a server or live configuration change.

### 0.3.1

* Fixed a bug that caused the Bonemaw Serpent attacks (and other creature attacks) to spam errors.

### 0.3.0

* New configurations: FloatEverything, FloatTreasure

### 0.2.2

* Updated to include new items from Ashlands. If upgrading your version check the config file for the new default additions.

### 0.2.1

* Fixed issue where patches may have applied before the initial server configuration syncing event. This made some server configurations not apply correctly.
* Added ability to update and apply configuration changes live.
* Added NeckTail to the meat category.

### 0.2.0

* Added Jotunn library as new dependency for config syncing, you now must also install Jotunn for this mod to work

### 0.1.1

* Bug fix for different configs not applying correctly (This is why you don't rush things when you copy/paste code)

### 0.1.0

* First release

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
