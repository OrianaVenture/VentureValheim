# Asocial Cartography

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Improved multiplayer Cartography Table handling. Toggle ability to share and receive player placed map pins at the table. The table will now remember all pins shared to it.

## Features

Playing multiplayer can often cause cartography table debacle. Your friend NEEDS to mark every berry bush, and then they share it with you too. Well, no more.

### Player Placed Pins

This mod gives you two new ways to manage map pins from the cartography table: prevent adding and prevent taking. By default this mod will prevent the player from adding custom map pins to the cartography table, and allow taking all pins from the table. If your friends are less tech savy than you and don't know how to change config files this should fix your problems given they install the mod. If you are the only one installing the mod then you can change the ReceivePins config to false to prevent getting all those nasty berry bush pins.

These toggles only apply to the 5 placable pins in vanilla by default. All discovered boss alter locations and other types of pins will still be shared as usual. This makes it easier to share the pins that really matter without worrying about clutter. If you wish to also filter boss or hildir map pins set IgnoreBossPins and/or IgnoreHildirPins to false and they will be treated as player-placed pins for the other settings.

### Map Merging

In vanilla the last person to write to a cartography table will erase the existing stored data. Usually you must first read the cartography table before writing to it to preserve the old pins. This mod changes the behavior to merge the existing data on the table with your valid entries before saving it to the table. If you desire to reset the cartography table with this mod installed you must delete it and rebuild.

### Vanilla Commands

If you are adding this mod mid-game and need to clean up your existing map pins the vanilla command "resetsharedmap" will remove shared cartography data for you.

## Installation

This mod is client side only and changes made to the configurations will only affect your client. Live updates to the configs will take immediate effect.

## Changelog

### 0.2.0

* Added new configs to block adding/receiving boss and hildir map pins.

### 0.1.0

* First release.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).