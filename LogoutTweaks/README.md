# Logout Tweaks

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Apply your rested bonus and stamina from your last logout. Use on servers to prevent "stamina logging".

## Features

Tracks your rested bonus and stamina values from your last logout and saves to a new file for next login. Helps prevent "stamina logging" where players will log out to replenish their stamina faster or in places where it is impossible like in the ocean.

* Reapply your rested bonus from your last logout
* Reapply your stamina from your last logout
* ServerSync included

## Changelog

### 0.2.1

* ServerSync patch for game patch 0.211.11.

### 0.2.0

* Upgraded ServerSync to V1.11: Crossplay compatibility upgrade for config sync.

### 0.1.1

* Small code refactoring, no feature changes.

### 0.1.0

* Update for game patch 0.211.7 Crossplay. Reverted ServerSync to 1.6.

### 0.0.5

* Updated ServerSync to V1.10. Fixed an issue with Server Sync config not locking.

### 0.0.4

* Added ability to restore stamina from last logout with regen delay of 5 seconds. Added more config options. Changed file storage structure, expect a warning on version upgrade.

### 0.0.3

* Minor refactoring to the code, no feature changes. Changed the patch for loading saved rested bonus data to be more correct. Reset file data on load to prevent an possible incorrect state. Moved the call to get the rested bonus data to a prefix to ensure the data is more accurate.

### 0.0.2

* Packaged ServerSync into the main dll.

### 0.0.1

* Added ability to reapply your rested bonus from your last logout

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me on the [Odin Plus](https://discord.gg/vYfFHxpJgN) discord, or at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
