# World Advancement and Progression

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Control skill levels, haldor's items, and manage world and individual player keys! Lock different actions behind an enforced boss progression to control the rate at which players advance.

## Features

The main feature of this mod is to have an easy way to control the rate at which the world and individual player advances.

Below are some explanations of features and how to configure them. See more details in the config file. Generate the config file by launching the game once with this mod installed. The information included in the file will not always match the information provided in this readme.

### Hildir Update Notes

There were changes to the vanilla commands that may be confusing. There are now "player unique keys" being used which are not the same thing as this mod's private key system. Please note these commands will not influence your private keys when using that feature, you must use the commands added (explained below under Key Management -> Commands) to manage private keys for this mod.

* listkeys command now shows you your vanilla player unique keys
* Original resetkeys command will now also reset your vanilla player unique keys in addition to global keys
* Additional vanilla key commands were added to manage vanilla player unique keys: setkeyplayer, removekeyplayer

There is now a feature in vanilla for "player based events". Using the private key system the player based raid setting will be applied automatically at startup. The old logic for this feature has been removed and is no longer supported at this time.

In order to preserve the new world modifiers (that act as global keys) this mod now has a built in system to check keys are "qualifying". All the keys listed in the Vanilla Public Keys section below are considered "qualifying keys". All keys that do not fall under this list will not be added to player private keys, and will not be blocked from the public key list. There is a new configuration option QualifyingKeys where you can add additional keys from other mods so they can be supported.

There are likely new bugs with existing features and the new game content, please report any issues you encounter.

** This information is subject to change in a future update

<details open>
<summary>Expand/Collapse Features</summary>

### Key Management

When this mod is installed there will be a key "cleanup" performed for the server and any player who joins the game based off the mod configurations. When using the default settings you can expect all global keys to be cleared when you start up the server, resetting your server's key progress. When using private keys a similar principal applies, depending on your blocked or allowed key list, any keys that are not expected will be removed. All enforced keys will be added to the appropriate list on startup regardless of other settings. If you see your keys resetting unexpectedly make sure to check your mod configuration is allowing the keys you want to exist. Any keys added manually will persist until the server is restarted (for private keys when the player logs back in), to ensure these keys remain after a restart you must check your mod configuration!

#### Global Key Management

What is a key and what is controlled by them? In vanilla Valheim there exists a "global key" list that is a bunch of strings shared by all players. Worldly spawns, raids, dreams, and Haldor's items are all controlled by the presence of specific keys. By default this mod will prevent/block global keys from being added to the global list which will prevent game behaviors that rely on the presence of these keys. This lets you control the game progression by choosing when these keys can be added to the game.

#### Private Key Management

This mod also adds a private player key system in which data is saved to the character file. You can use this private key system to tailor game functionality to individuals rather than the vanilla default server-wide global keys. Gameplay will be altered when using private keys: The player that is hosting a loaded "chunk" will control the worldly spawns. For example, a player A with no keys joins player B with all the boss keys and starts seeing Fuling night spawns, but if player A is alone they should not get higher level spawns. If player A loads and hosts an area and is later joined by player B, the area should not spawn the higher level monsters that become unlocked with keys.

Private keys will be added to any player within a 100 meter range of the hosting player when the action occurs. For example, when a boss dies any player close enough to the chunk-hosting player should also get the private key, but a player online on the other side of the map will not get it.

After the Hildir update a new game setting was added to vanilla for player-based raids. When using private keys this mod will enable this setting for you automatically.

#### Key Configuration Options

* BlockedActionMessage: The message used in-game for certain actions when certain keys are blocked for players.
* BlockAllGlobalKeys: Prevent/block public all keys from being added to the global list, set to false to use vanilla behavior
* AllowedGlobalKeys: Allow only these keys being added to the global list when BlockAllGlobalKeys is true
* BlockedGlobalKeys: Stop only these keys being added to the global list when BlockAllGlobalKeys is false
* EnforcedGlobalKeys: Always add these keys to the global list on startup (regardless of other settings)
* UsePrivateKeys: Use private player keys, rather than global keys for game key checking
* BlockedPrivateKeys: Stop only these keys being added to the player's key list when UsePrivateKeys is true (use this or AllowedPrivateKeys)
* AllowedPrivateKeys: Allow only these keys being added to the player's key list when UsePrivateKeys is true (use this or BlockedPrivateKeys, if the BlockedPrivateKeys has any values it will use that setting)
* EnforcedPrivateKeys: Always add these keys to the player's private list on startup (regardless of other settings)
* QualifyingKeys: Additional keys you would like to be tracked by this mod, use if you have other mods that add keys. You do not need to specify the vanilla keys as they are added automatically.

#### Vanilla Public Keys (That this mod will track)

* defeated_eikthyr
* defeated_gdking
* defeated_bonemass
* defeated_dragon
* defeated_goblinking
* defeated_queen
* defeated_hive
* KilledTroll
* killed_surtling
* KilledBat
* Hildir1
* Hildir2
* Hildir3

#### Commands

Due to the changes this mod makes the vanilla "setkey" command will not function as expected in most cases. There is an added command "setglobalkey" that will work in it's place. For private keys there are 4 new commands added that work similar to the vanilla public key commands: setprivatekey, removeprivatekey, resetprivatekeys, listprivatekeys. For example, you can set your local player's key with "setprivatekey defeated_eikthyr", or any online player with "setprivatekey defeated_eikthyr PlayerName".

The server also tracks the player's keys in each game session. This list is cleared on a server restart and data for each player will only be available once the player reconnects. The command is "listserverkeys", if you are hosting the data will be available in the console window. If you have a dedicated server you can send this command to the server from the client and the data will be printed to the bepinex/logoutput.log file.

Don't know how to use commands? Dedicated servers do not allow for use of commands, but there are mods that can enable them (like Server devcommands by JereKuusela). All of these commands are considered "cheats" except the "listprivatekeys" command. To use cheats you must enable them with the "devcommands" command, you may have to be an admin for them to work.

### Locking

Taming can be locked by keys when enabled. By default Wolf is locked by the defeated_bonemass key and Lox is locked by the defeated_dragon key. You can override this by using the prefab name of the creature, allowing you to add support for content from other mods. When overriding you must define all the creatures since it will no longer include the defaults. When using private keys make sure the player who can tame the animals is the first to load the area since taming is controlled by the player hosting the chunk. You will still see taming hearts (for now), but if you check the taming status on the animals you will see the percentage no longer increases when blocked.

Guardian Powers and Boss Alter Summoning can be locked too. By default summoning is locked the key given by the previous boss in the natural progression order. You can override this by using the prefab name of the creature the alter summons, allowing you to add support for content from other mods. When overriding you must define all the bosses manually (similar to the taming override). Additionally, these actions have a fun special effect on failure!

Using equipment, crafting, building, and cooking can all be locked with individual settings. The materials are categorized by the biome they are naturally found in. For example, if you lock all these features, if you have not defeated any bosses then you cannot use or craft the antler pickaxe, place a forge, make deer stew, or unlock swamp crypts. When paired with private keys, these features force new players joining your server to follow the progression of the game in order to advance. This will apply to all game items that use vanilla crafting materials.

#### Locking Configuration Options

* LockTaming: If true you can only tame certain creatures if you have the required key.
* OverrideLockTamingDefaults: Define your own required keys to tame specific creatures or leave blank to use the defaults. Example: Boar, defeated_eikthyr, Wolf, defeated_dragon, Lox, defeated_goblinking
* LockGuardianPower: If true locks the ability to get or use boss powers based on the required key.
* LockBossSummons: If true you can only summon bosses based on the required key.
* OverrideLockBossSummonsDefaults: Define your own required keys to summon bosses or leave blank to use the defaults. Note that if you do not include a boss in this list then the unlock over time settings will not apply to that boss. If you just want to enforce time as the restraint then set each of the boss keys to blank (like for Eikthyr). Example (also the mod defaults): Eikthyr, ,gd_king, defeated_eikthyr, Bonemass, defeated_gdking, Dragon, defeated_bonemass, GoblinKing, defeated_dragon, SeekerQueen, defeated_goblinking
* UnlockBossSummonsOverTime: If true will additionally check the appropriate time has passed for unlocking the boss alters. This will still enforce the boss progression order unless overridden above.
* UnlockBossSummonsTime: Time for previous setting, default is 100. Example: Eikthyr wil be available on day 0, the Elder on 100, Bonemass on 200 etc.
* LockEquipment: If true you can only equip or use boss items or items made from biome metals/materials if you have the required key
* LockCrafting: If true you can only craft items made from boss items or biome metals/materials if you have the required key
* LockBuilding: If true you can only build pieces made from boss items or biome metals/materials if you have the required key
* LockCooking: If true you can only cook items made from biome materials if you have the required key

### Trader Configuration Options

There are more key options specifically for the Haldor trader under their own section. All vanilla items have their own configuration option if you wish to override the required key to unlock them. If these configurations are left blank it will use the game defaults. If you wish to remove only some key requirements you can achieve this by setting the item keys to your own custom key like "Trader" and then "enforce" this key in the appropriate configuration mentioned above so all players can access it. Similarly, you can lock items by specifying a custom key that is then never added to the game (or only given to certain players when using private keys).

* UnlockAllHaldorItems: If true bypasses the key check for Haldor's items and unlocks everything

### Skill Manager

Have more control over Skill loss and gain. When using skill capping any skills that are already above the maximum skill cap will remain "frozen" and will not gain, but can still be lowered on death. Console cheats will still work as intended. Other mods that change how skill gain and loss functions may cause unexpected behaviors. Turn off this feature if using another mod for skill management if you see mod conflicts.

Want to just lose accumulation points when you die? This is possible if you set UseAbsoluteSkillDrain = true, and AbsoluteSkillDrain = 0.

#### Configuration Options

* EnableSkillManager must be set to True to enable these features.
* AllowSkillDrain: Set to False to turn off all skill loss on death.
* UseAbsoluteSkillDrain: Set to True to use an absolute number (AbsoluteSkillDrain) for skill loss. (Vanilla uses a percentage for skill loss, so you will lose more skill the higher the skill is)
* CompareAndSelectDrain: Set to true to use the minimum or maximum value between the vanilla skill loss and the absolute skill loss. Set CompareUseMinimumDrain to False to use the maximum of these two values as the skill loss.
* OverrideMaximumSkillLevel: True to set a server wide skill level ceiling for gaining skill to the value of MaximumSkillLevel. For example, if this value is 50 then you will not gain skill once you reach level 50+ in that skill.
* OverrideMinimumSkillLevel: True to set a server wide skill level floor for skill loss to the value of MinimumSkillLevel. For example, if this value is 10 then you will not lose skill on death until you reach level 10 in that skill, so your skills will not drop below 10.
* UseBossKeysForSkillLevel: Set this to true to use a more dynamic skill control dependant on boss completion. Skill minimum will start at 0 and increase by BossKeysSkillPerKey for each boss defeated. Skill maximum will be capped at [ 100 - (number of bosses: 6) * (BossKeysSkillPerKey: 10) = 40 ] with the current game state. For example, if you defeat one boss then your skill minimum for loss will be raised to 10, and your skill maximum will be raised to 50.
* BossKeysSkillPerKey: Amount used in calculation above. Note: This feature will only work if you DO NOT override the minimum and/or maximum skill levels as described above. Overridden values will take precedence.

### World Scaling

This was a previous feature of this mod that has been pulled out into it's own module. To use scaling please download and configure the new mod. Directions on how to port over existing yaml files included with new mod.

</details>

## Installation

This mod needs to be on both the client and server for all features to work. When this mod is put on a server it will sync the configurations from the server to all clients on connection. Live changes to the configurations will not always take effect until the player relogs into the world/server.

### Server-Side Only?

This mod will partially work server-side only to control the global key list. If you just need to stop global spawns and raids this mod can just be installed on the server. However, you may have to spawn in any of Haldor's items that would otherwise be unavailable when blocking global keys. The private keys, Haldor, locking, and skills features have to be on a client to work.

### Client-Side Only?

If you do not install this mod on the server then any player can change the configurations however they please. Raids will not be selected correctly when using private keys.

## Changelog

### 0.1.1

* Hard check added to ensure keys are not world modifiers, fixes world save corruption issue (super apologies for this)
* QualifyingKeys config added to allow support for other mods due to hard check
* Private key cleanup will now remove all keys that are non-qualifying, make sure to configure this new setting

### 0.1.0

* Update for game patch version 0.217.14 (Please read Hildir update notes at top)
* First official release, bug fixing to come for Hildir as reported
* UsePrivateKeys config now defaults to true
* Removed private key implementation for raids, now sets and uses the vanilla world modifier for player based raids

### 0.0.29

* Removed world scaling feature from this mod (get the new mod if previously using)
* Deprecated the key file port logic (Files deprecated in version 0.0.19, if you have used a newer version of this mod your characters are already upgraded)
* Added logic to support global key management server-side only
* Added config toggle to display the blocked action message and effect
* Added config settings for automatic boss summon unlocking over time
* Moved the locking configs to a new section (You will have to manually update your config file if using)
* Moved boss keys for skill level configs to Skills section (You will have to manually update your config file if using)
* P.S. This should be the last set of major changes before the official release

### 0.0.28

* Update for game patch 0.216.9
* Fixed skill drain edge cases for skill manager where skills could rarely drop below the skill floor
* Bug fix for keys saving to incorrect character when switching between them in the same game session

### 0.0.27

* Bug fix for blocking taming not always working correctly, not comparing correct prefab name so getting false negatives
* Small qol refactors (see github for full commit)

See all patch notes on Github.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
