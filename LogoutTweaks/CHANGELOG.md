## 0.5.1

* Fixed mod compatibility issue with SpeedyPaths 1.0.8 by Nextek that caused the "status effects" for that mod to display incorrectly.
* Added status effect for Ashland boss and new items.

## 0.5.0

* Reworked mod to only track certain vanilla status effects. Fixes mod compatibility issues.
* Poison and Burning effects now reapply damage.

## 0.4.1

* Bug fix for issues when using mods with custom status effects like Valheim Legends.

## 0.4.0

* Update for game patch 0.216.9
* Upgraded file storage to use the player save file rather than an additional file (you will lose your last logout session upon upgrading, you can delete the extra files .previously created by this mod).
* Removed stamina feature since it is now in vanilla.
* Removed config file, no longer needed.

## 0.3.1

* Update for game patch 0.212.7
* Upgraded ServerSync to version 1.13

## 0.3.0

* Now tracks all status effects!
* Fix for stamina config setting, now lets you ignore that feature entirely.
* You will need to generate a new config file.
* Changed file storage structure, expect a warning on version upgrade.

## 0.2.1

* ServerSync patch for game patch 0.211.11.

## 0.2.0

* Upgraded ServerSync to V1.11: Crossplay compatibility upgrade for config sync.

## 0.1.1

* Small code refactoring, no feature changes.

## 0.1.0

* Update for game patch 0.211.7 Crossplay. Reverted ServerSync to 1.6.

## 0.0.5

* Updated ServerSync to V1.10. Fixed an issue with Server Sync config not locking.

## 0.0.4

* Added ability to restore stamina from last logout with regen delay of 5 seconds. Added more config options. Changed file storage structure, expect a warning on version upgrade.

## 0.0.3

* Minor refactoring to the code, no feature changes. Changed the patch for loading saved rested bonus data to be more correct. Reset file data on load to prevent an possible incorrect state. Moved the call to get the rested bonus data to a prefix to ensure the data is more accurate.

## 0.0.2

* Packaged ServerSync into the main dll.

## 0.0.1

* Added ability to reapply your rested bonus from your last logout