## Unrelease

* Fix blocking mechanism for crafting recipes requiring only one ingredient.

## 0.2.13

* Bug fixes to allow skill cache to update when keys change. Should fix issues with skill capping not working as expected.
* Bug fix for raids feature only being "rallies the creatures", was not using the correct ID so no keys were being detected for players.

## 0.2.12

* Update for game version  0.218.12+.
* Updated to include new items from Ashlands.
* Skill capping now accounts for the Ashlands boss, this will naturally lower the skill gain ceiling.

## 0.2.11

* Refactored configuration live updates to only happen when needed.
* Refactored server key tracking to use player IDs rather than names. This will allow players to have the same name on a server.
* Removed cleanup patch for incorrectly configured world modifiers (if you manually break your world modifiers you can fix them with Venture Debugger).
* Set commands to onlyServer false.

## 0.2.10

* Update for game patch 0.217.46

## 0.2.9

* Added compatibility for power locking with the Passive Powers mod. Should now work when multiple powers are selected given player and/or world must have unlocked all keys for selected powers.

## 0.2.8

* Restored private key implementation for raids from the pre-Hildir update.
* Fixed blocking not working for ammo items like arrows due to auto-equipping.

## 0.2.7

* Improved other mod compatibilities by additionally patching for ZoneSystem.RPC_RemoveGlobalKey method.
* Added new command removeglobalkey due to previous change altering the vanilla removekey command.
* Refactored patches so both global keys and private keys lists are handled appropriately when using private keys to respect global key configurations. Changes some vanilla commands' behavior.
* Added built in support for the Seasonality mod.

## 0.2.6

* Improved other mod compatibilities when using private keys by additionally patching for ZoneSystem.GetGlobalKeys method.
* Added new command listglobalkeys due to previous change altering the vanilla listkeys command.
* Added new command resetglobalkeys.
* All keys will now be converted to lowercase internally for handling to mimic vanilla behavior (let me know if this causes issues with other mods using keys)

## 0.2.5

* Fixed issues with skill levels being calculated incorrectly when UseBossKeysForSkillLevel was true.
* Added new configuration LockPortalsKey to control player ability to use portals. Defaults to off.
* Added check to make sure activeBosses key is treated like world modifiers.

## 0.2.4

* Fixed an issue where item upgrades were not checked for locking. Will now correctly identify item level and will lock items based on current summation of crafting ingredients for items.

## 0.2.3

* Removed the Qualifying keys feature added in 0.1.1, mod will now correctly identify world modifiers without further configuration
* Orphaned config QualifyingKeys will remain in your config files but is not used
* If using other mods that add keys between the updates (0.1.1 - 0.2.2) they may now be in your global key list (might not cause issues, but be aware)

## 0.2.2

* Fixed issue with PlayerEvents world modifier not being applied correctly (was disabling raids when using private keys)

## 0.2.1

* Update for game patch 0.217.22, bepinex version 5.4.22.0

## 0.2.0

* Added Jotunn library as new dependency for config syncing, you now must also install Jotunn for this mod to work
* Added new config for admins to bypass locking settings
* Added new config to toggle blocked action fire effect
* Added missing items to locking system: lox pelt, blue jute, sharpening stone, thistle, entrails
  * Note: entrails set to defeating Eikthyr (not Elder) due to meadows draugr villages
* Fixed issue with cauldron not locking cooking

## 0.1.4

* Added new configurations for Hildir trade items
* Bug fix for instances where hiding all trader items throws an error

## 0.1.3

* Created save recovery patch to fix duplicate world modifiers entry
* GlobalKeyAdd use false for canSaveToServerOptionKeys parameter to avoid duplicate entry

## 0.1.2

* Added check to ensure player events key is not added multiple times after restart (part of corruption issue)

## 0.1.1

* Hard check added to ensure keys are not world modifiers, fixes world save corruption issue (super apologies for this)
* QualifyingKeys config added to allow support for other mods due to hard check
* Private key cleanup will now remove all keys that are non-qualifying, make sure to configure this new setting

## 0.1.0

* Update for game patch version 0.217.14 (Please read Hildir update notes at top)
* First official release, bug fixing to come for Hildir as reported
* UsePrivateKeys config now defaults to true
* Removed private key implementation for raids, now sets and uses the vanilla world modifier for player based raids

## 0.0.29

* Removed world scaling feature from this mod (get the new mod if previously using)
* Deprecated the key file port logic (Files deprecated in version 0.0.19, if you have used a newer version of this mod your characters are already upgraded)
* Added logic to support global key management server-side only
* Added config toggle to display the blocked action message and effect
* Added config settings for automatic boss summon unlocking over time
* Moved the locking configs to a new section (You will have to manually update your config file if using)
* Moved boss keys for skill level configs to Skills section (You will have to manually update your config file if using)
* P.S. This should be the last set of major changes before the official release

See all patch notes on Github.
