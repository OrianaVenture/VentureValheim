# World Advancement and Progression

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Control skill levels, trader items, and manage world and individual player keys! Lock different actions behind an enforced boss progression to control the rate at which players advance.

## Features

The main feature of this mod is to have an easy way to control the rate at which the world and individual player advances. Below are some explanations of features and how to configure them. See more details in the config file. Generate the config file by launching the game once with this mod installed. The information included in the file will not always match the information provided in this readme.

**ASHLANDS UPDATE:** If you are upgrading the mod for the ashlands update and use default configurations there will be no action you will need to take. If you have customized the configurations you will need to add in support for the new ashlands boss. The new boss prefab is "Fader" and defeating it adds the "defeated_fader" key. The new tamable creature is "Asksvin" and by default taming is locked by the "defeated_queen" key.

### Key Management

What is a key and what is controlled by them? In vanilla Valheim there exists a "global key" list that is a bunch of strings shared by all players. Worldly spawns, raids, dreams, and Haldor's items are all controlled by the presence of specific keys. Each boss, select creatures in the game, as well as Hildir's quests each have keys associated with their completion. The main feature of this mod is the addition of a private key system that makes this progress all individual.

#### Private Keys

When enabled, private keys will be added to any player within a 100 meter range of the hosting player when the action occurs. For example, when a boss dies any player close enough to the chunk-hosting player should also get the private key, but a player online on the other side of the map will not get it.

The player that is hosting a loaded "chunk" will control the worldly spawns. For example, a player A with no keys joins player B with all the boss keys and starts seeing Fuling night spawns, but if player A is alone they should not get higher level spawns. If player A loads and hosts an area and is later joined by player B, the area should not spawn the higher level monsters that become unlocked with keys.

Hildir keys for unlocking store content are applied when the chests are turned in. Make sure all participating players are present when the chest is turned in to get credit. (Hildir wants to thank you personally!)

#### Private Raids (Events)

When using private keys it will alter how raids work in your world. After the Hildir update a new game setting (world modifier) was added to vanilla for player-based raids. When using the private key system the player based raid setting will be applied automatically for you at startup. This is required for the private raids feature to work.

When using this feature with other mods it is important to note this mod checks the "global key" requirements on the raid to determine if a raid is valid. It is the same logic checks as if using vanilla global keys, just done for each player.

If you see conflicts with other mods you can turn off this mod's private raids feature by setting UsePrivateRaids = false. When set to false the vanilla player based raids logic will be used.

#### Important Tips

* It is recommended to set up this mod on a new or unused world, otherwise ensure you have backups to restore if something goes wrong.
* By default this mod will prevent/block global keys from being added to the global list, and will enable private keys where all players keys will be tracked individually.
* When this mod is installed there will be a key "cleanup" performed for the server and any player who joins the game based off the mod configurations. When using the default settings all global keys are cleared on startup, resetting your server's key progress.
* Any keys added with commands will always persist until the server is restarted (for private keys when the player logs back in). If you see your keys resetting unexpectedly on restart it may be due to the mod configurations not allowing for them to exist.
* Private Key data is saved to the character file when enabled, making it compatible with Server Characters.
* Vanilla World Modifiers should not be affected by this mod.

#### Key Configuration Options

| Configuration <br>_______________| Description <br>_____________|
|--- |--- |
| BlockAllGlobalKeys | Prevent/block public all keys from being added to the global list, set to false to use vanilla behavior. |
| AllowedGlobalKeys | Allow only these keys being added to the global list when BlockAllGlobalKeys is true. |
| BlockedGlobalKeys | Stop only these keys being added to the global list when BlockAllGlobalKeys is false. |
| EnforcedGlobalKeys | Always add these keys to the global list on startup (regardless of other settings) |
| UsePrivateKeys | Use private player keys, rather than global keys for game key checking. |
| BlockedPrivateKeys | Stop only these keys being added to the player's key list when UsePrivateKeys is true (use this or AllowedPrivateKeys). |
| AllowedPrivateKeys | Allow only these keys being added to the player's key list when UsePrivateKeys is true (use this or BlockedPrivateKeys, if the BlockedPrivateKeys has any values it will use that setting). |
| EnforcedPrivateKeys | Always add these keys to the player's private list on startup (regardless of other settings). |

#### Important Vanilla Public Keys

* defeated_eikthyr
* defeated_gdking
* defeated_bonemass
* defeated_dragon
* defeated_goblinking
* defeated_queen
* defeated_fader
* KilledTroll
* killed_surtling
* KilledBat
* Hildir1
* Hildir2
* Hildir3

#### Commands

Due to the changes this mod makes the vanilla commands will not work as expected. Below is an explanation of how commands function with this mod installed:
| Command  <br>_____________________________________| Origin <br>__________| Behavior <br>___________________|
|--- |--- |--- |
| listkeys | vanilla | Will list all global and private keys for the current character in one list. Also displays vanilla "player unique keys". |
| listprivatekeys | this mod | Lists the private keys for the current character. |
| listglobalkeys | this mod | Lists the global keys for the world. |
| setkey Key | vanilla | Adds the key if allowed to the global list and to the private key list of all players in range of you when the command is sent. |
| setkeyplayer Key | vanilla | Adds the key to your current character's vanilla "player unique keys" list. |
| setprivatekey Key PlayerName | this mod | Adds a private key to the specified online player. If name left blank will apply to your current character. |
| setglobalkey Key | this mod | Adds a global key for the world. |
| removekey Key | vanilla | Removes a global key for the world and from all online players' private key list when using private keys. |
| removeglobalkey Key | this mod | Removes a global key for the world. |
| removekeyplayer Key | vanilla | Removes the key from your current character's vanilla "player unique keys" list. |
| removeprivatekey key PlayerName | this mod | Removes a private key to the specified online player. If name left blank will apply to your current character. |
| resetkeys | vanilla | Removes all global keys for the world and all vanilla "player unique keys" from your current character. |
| resetprivatekeys PlayerName | this mod | Removes all private keys for the specified online player. If name left blank will apply to your current character. |
| resetglobalkeys | this mod | Removes all global keys for the world. |
| listserverkeys | this mod | If hosting a game session will print all recorded player keys to the console window (and bepinex\logoutput.log file). If on a dedicated server will send a command for the server to print it to the server log file for viewing. This data is cleared on every restart and only records players who have reconnected at least once during the session. |

Don't know how to use commands? Dedicated servers do not allow for use of commands, but there are mods that can enable them (like Server devcommands by JereKuusela). All of these commands are considered "cheats" except the "listprivatekeys" command. To use cheats you must enable them with the "devcommands" command, you may have to be an admin for them to work.

### Locking

Many actions in the game can be locked by world or player progress. If enabled, performing these actions have a fun special effect on failure! If you prefer to opt out of these effects you may edit the UseBlockedActionMessage, BlockedActionMessage, and UseBlockedActionEffect configs. I do not recommend turning off all indicators of failure, otherwise players may get confused.

Admins can bypass locking settings by enabling the AdminBypass setting.

#### Locking Taming

Taming can be locked by keys when enabled. By default Wolf is locked by the defeated_bonemass key, Lox by defeated_dragon, and Asksvin by defeated_queen. You can override this by using the prefab name of the creature, allowing you to add support for content from other mods. When overriding you must define all the creatures since it will no longer include the defaults. When using private keys make sure the player who can tame the animals is the first to load the area since taming is controlled by the player hosting the chunk. You will still see taming hearts (for now), but if you check the taming status on the animals you will see the percentage no longer increases when locked.

#### Locking Bosses

Guardian Powers and Boss Alter Summoning can be locked. By default summoning is locked by the key given by the previous boss in the natural progression order. You can override this by using the prefab name of the creature the alter summons, also allowing you to add support for content from other mods. When overriding you must define all the bosses you want included in the locking system.

Additionally, you can bypass the progression order and just enforce in-game days as the only restraint for unlocking boss alters when using UnlockBossSummonsOverTime with the following setting for OverrideLockBossSummonsDefaults: Eikthyr, , gd_king, , Bonemass, , Dragon, , GoblinKing, , SeekerQueen, , Fader, ,

If you want manual control over when bosses become available to summon you will need to manually update the OverrideLockBossSummonsDefaults setting each time you want to unlock a new boss. Using any key that a player cannot obtain will block them from summoning it. You can also use this strategy to only give certain player access to boss alters. Example of locking all boss alters: Eikthyr, Locked, gd_king, Locked, Bonemass, Locked, Dragon, Locked, GoblinKing, Locked, SeekerQueen, Locked, Fader, Locked

#### Locking Portals

The ability to use portals can be locked behind one specified key with the LockPortalsKey setting.

#### Locking Everything Else

Using equipment, crafting, building, and cooking can all be locked with individual settings. The materials are categorized by the biome they are naturally found in. For example, if you lock all these features, if you have not defeated any bosses then you cannot use or craft the antler pickaxe, place a forge, make deer stew, or unlock swamp crypts. When paired with private keys, these features force new players joining your server to follow the progression of the game in order to advance. This will apply to all game items that use vanilla crafting materials.

#### Locking Configuration Options

| Configuration <br>_______________| Description <br>_____________|
|--- |--- |
| LockTaming | If true you can only tame certain creatures if you have the required key. |
| OverrideLockTamingDefaults | Define your own required keys to tame specific creatures or leave blank to use the defaults. Example: Boar, defeated_eikthyr, Wolf, defeated_dragon, Lox, defeated_goblinking, Asksvin, defeated_fader |
| LockGuardianPower | If true locks the ability to get or use boss powers based on the required key. |
| LockBossSummons | If true you can only summon bosses based on the required key. |
| OverrideLockBossSummonsDefaults | Define your own required keys to summon bosses or leave blank to use the defaults. Example (also the mod defaults): Eikthyr, ,gd_king, defeated_eikthyr, Bonemass, defeated_gdking, Dragon, defeated_bonemass, GoblinKing, defeated_dragon, SeekerQueen, defeated_goblinking, Fader, defeated_queen |
| UnlockBossSummonsOverTime | If true will additionally check the appropriate time has passed for unlocking the boss alters. This will still enforce the boss progression order unless overridden above. |
| UnlockBossSummonsTime | Time for previous setting, default is 100. Example: Eikthyr wil be available on day 0, the Elder on 100, Bonemass on 200 etc. |
| LockEquipment | If true you can only equip or use boss items or items made from biome metals/materials if you have the required key. |
| LockCrafting | If true you can only craft items made from boss items or biome metals/materials if you have the required key. |
| LockBuilding | If true you can only build pieces made from boss items or biome metals/materials if you have the required key. |
| LockCooking | If true you can only cook items made from biome materials if you have the required key. |
| LockPortalsKey | Define your own required key to control player ability to use portals. Leave blank to allow vanilla portal behavior. |

### Trader Configuration Options

There are more key options specifically for Haldor under it's own section called "Trader". Similarly there is a section for Hildir. All vanilla items have their own configuration option if you wish to override the required key to unlock them. If these configurations are left blank it will use the game defaults. If you wish to remove only some item key requirements you can achieve this by setting the item keys to your own custom key like "Trader" and then "enforce" this key in the appropriate configuration mentioned above so all players can access it. Similarly, you can lock items by specifying a custom key that is then never added to the game (or only given to certain players when using private keys).

| Configuration <br>_______________| Description <br>_____________|
|--- |--- |
| UnlockAllHaldorItems | If true bypasses the key check for Haldor's items and unlocks everything. |
| UnlockAllHildirItems | If true bypasses the key check for Hildir's items and unlocks everything. |

### Skill Manager

Have more control over Skill loss and gain. When using skill capping any skills that are already above the maximum skill cap will remain "frozen" and will not gain, but can still be lowered on death. Console cheats will still work as intended. Other mods that change how skill gain and loss functions may cause unexpected behaviors. Turn off this feature if using another mod for skill management if you see mod conflicts.

Want to just lose accumulation points when you die? This is possible if you set UseAbsoluteSkillDrain = true, and AbsoluteSkillDrain = 0.

#### Configuration Options

| Configuration <br>_______________| Description <br>_____________|
|--- |--- |
| EnableSkillManager | Must be set to True to enable these features. |
| AllowSkillDrain | Set to False to turn off all skill loss on death. |
| UseAbsoluteSkillDrain | Set to True to use an absolute number (AbsoluteSkillDrain) for skill loss. (Vanilla uses a percentage for skill loss, so you will lose more skill the higher the skill is) |
| CompareAndSelectDrain | Set to true to use the minimum or maximum value between the vanilla skill loss and the absolute skill loss. Set CompareUseMinimumDrain to False to use the maximum of these two values as the skill loss. |
| OverrideMaximumSkillLevel | True to set a server wide skill level ceiling for gaining skill to the value of MaximumSkillLevel. For example, if this value is 50 then you will not gain skill once you reach level 50+ in that skill. |
| OverrideMinimumSkillLevel | True to set a server wide skill level floor for skill loss to the value of MinimumSkillLevel. For example, if this value is 10 then you will not lose skill on death until you reach level 10 in that skill, so your skills will not drop below 10. |
| UseBossKeysForSkillLevel | Set this to true to use a more dynamic skill control dependant on boss completion. Skill minimum will start at 0 and increase by BossKeysSkillPerKey for each boss defeated. Skill maximum will be capped at [ 100 - (number of bosses: 6) * (BossKeysSkillPerKey: 10) = 40 ] with the current game state. For example, if you defeat one boss then your skill minimum for loss will be raised to 10, and your skill maximum will be raised to 50. |
| BossKeysSkillPerKey | Amount used in calculation above. Note: This feature will only work if you DO NOT override the minimum and/or maximum skill levels as described above. Overridden values will take precedence. |

### World Scaling

This was a previous feature of this mod that has been pulled out into it's own module. To use scaling please download and configure the new mod. Directions on how to port over existing yaml files included with new mod.

## Installation

This mod needs to be on both the client and server for all features to work. Config Syncing is included with Jotunn. Install on the server to enforce the same mod configuration for all players. Live changes to the configurations will not always take effect until the player relogs into the world/server.

### Server-Side Only?

This mod will partially work server-side only to control the global key list. If you just need to stop global spawns and raids this mod can just be installed on the server. However, you may have to spawn in any of Haldor's items that would otherwise be unavailable when blocking global keys. The private keys, Haldor, locking, and skills features have to be on a client to work.

### Client-Side Only?

If you do not install this mod on the server then any player can change the configurations however they please.

## Other Mod Support

This mod may behave unexpectedly when used with other mods that rely on global keys to function. If you see any mod incompatibilities please report the issue so it can be resolved.

### Seasonality

This mod automatically recognizes season keys as global and does not require further user configuration.

## Changelog

Moved to new file, it will appear as a new tab on the thunderstore page.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
