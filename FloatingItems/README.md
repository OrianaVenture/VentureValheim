# Venture Floating Items

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Makes previously sinkable items float on water, or floating items sinkable. Configurable.

## Features

Allows you to make items that usually sink the ability to float on water (or tar), and vice versa. To make items sink add their prefab names to the "SinkingItems". To make items float toggle the following categories you want, or add the specific item prefab names to the "FloatingItems" config list:

* FloatTrophies: Will add the floating ability to any item that contains "trophy" in it's prefab name
* FloatMeat: Will add the floating ability to any item that contains "meat" in it's prefab name
* FloatHides: Will add the floating ability to the different hides and fabrics
* FloatGearAndCraftable: Will add the floating ability to any item that contains a recipe (can be crafted by a player), and other gear like items bought from Haldor and meads

If an item is included in the "SinkingItems" list it will be excluded from automatic item discovery when turning on the other config toggles. The mod defaults will add SerpentScale to the "FloatingItems" list, and will add BronzeNails and IronNails to the "SinkingItems" list.

This mod does not make all items float. This version is intended for people who want to customize specific items, rather than make all of them float by default. If you wish to make all items float then check out FloatingItems by castix. It is a lighter weight solution for all your floating item needs.

## Installation

This mod needs to be on all clients with matching configurations to work properly. When this mod is put on a server it will sync the configurations from the server to all clients on connection. Live changes to the configurations will not take effect until the player relogs into the world/server.

## Changelog

### 0.1.1

* Bug fix for different configs not applying correctly (This is why you don't rush things when you copy/paste code)

### 0.1.0

* First release

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
