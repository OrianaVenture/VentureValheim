## 0.10.1

* Update for game version 0.219.13.

## 0.10.0

* Added new configuration IgnoreList to specify locations to not reset.

## 0.9.1

* Blacklisted random flying birds from being reset - they tend to gather in large numbers and I think are plotting something.
* Added additional configurations for setting reset times of Ashlands locations: CharredFortress, LeviathanLava, MorgenHole, PlaceofMystery

## 0.9.0

* Update for Ashlands game version 0.218.15.
* Locations resets will now also reset any terrain modifications in their radius.

## 0.8.2

* Update for game patch 0.217.46

## 0.8.1

* Added compatibility for the "Dungeon Splitter" mod, will not reset sky locations when using it.

## 0.8.0

* Improved resetting logic for sky locations:
    * Can now auto-detect if a location is a sky location, improves other mod compatibilities
    * Can now additionally reset the ground outside a sky location
    * Better bounds detection for all locations
    * Improved performance for door resetting
* Added support for resetting "Destructable" and "MineRock(5)" objects. This includes trees, rocks, ores, and mistlands giant mineables
* Improved error handling

## 0.7.0

* Added support for Mining Caves
* Added support for all Wayshrines and Mystical_Well0, these will never reset to prevent issues
* Added new console command "resetlocations" with optional range parameter
* Small bug fix where a respawn would apply the wrong rotation to some objects (Quaternion math is special and I'm apparently dyslexic)

## 0.6.1

* Added support for CaveDeepNorth_TW from Therzie's Warfare mod

## 0.6.0

* Added Jotunn library as new dependency for config syncing, you now must also install Jotunn for this mod to work

## 0.5.1

* Fixed configs not selecting the correct reset times and just using default
* Added advanced configs for new Hildir's Request dungeons

## 0.5.0

* Update for game patch version 0.217.14 (Hildir's Request)
* Internal code refactor to simplify resetting, now auto-detects dungeons and all reset tracking moved to LocationProxy object
    * Improved compatibility for custom dungeons from other mods (does not apply for non-dungeon sky locations)
    * You will lose reset times on all dungeons when upgrading, they will be treated as if the mod was newly installed
* Bug fix for frost caves and mistlands dungeons regenerating with different seeds
* Reworked resetting logic of leviathans to only happen if the player is the "chunk owner" (similar update happened in 0.3.1)
* Added ItemDrop to recognized type for resets, this will now delete and respawn things you can throw on the ground
* Extended the range of triggering resets to 100 meters for all locations and dungeons (sky locations previously 30)

## 0.4.0

* Update for game patch 0.216.9
* Bug fix for multiple leviathans spawning in the same place in multiplayer (needs extra testing)
* Bug fix for ground locations not detecting player built pieces correctly (incorrect radius check)

## 0.3.1

* Reworked resetting logic of dungeons and locations to only happen if the player is the "chunk owner". Should fix a bug where reset times were not being recorded correctly and caused multiple resets to happen in a short amount of time
* Added a redundant error check to ensure a reset does not happen unless the reset time is set properly

## 0.3.0

* Small optimization to the reset timing for dungeons and locations, now checks the area is ready before performing a reset - should help reduce small duplication errors
* Major addition of Leviathan Resetting with new configs! (defaults to on! be aware when you upgrade the mod)

## 0.2.3

* Changed how zone centers of locations are determined to improve accuracy of location deletion
* Removed the ModEnabled config since it doesn't really do anything important

## 0.2.2

* Fixed issue with multiplayer sessions triggering multiple resets due to reset day not being recorded correctly, plus more internal error checking
* Changed how zone sizes of locations are calculated to improve accuracy of location deletion
* Added resetting for doors that require keys to open (sunken crypts and citadel, should support custom content)
* Added missing wear and tear damage to resetting dungeons

## 0.2.1

* Added mod support for Monsterlabz and Horem's locations.

## 0.2.0

* Added support for resetting all kinds of ground locations including villages and fuling camps
* New config option to toggle resetting ground locations (defaults to on! be aware when you upgrade the mod)
* New config options for specifying reset times for different dungeons

## 0.1.2

* Patch for object deletion to hopefully solve a multiplayer deletion issue.

## 0.1.1

* Added a new config option SkipPlayerGroundPieceCheck
* Please note I forgot to bump the version number for this release.

## 0.1.0

* First release
