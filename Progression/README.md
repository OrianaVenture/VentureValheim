# World Advancement and Progression

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

This mod is in Beta: Use at your own risk! (make a backup of your data before you update). You will likely need to generate a fresh config file.

World Advancement and Progression lets you fine tune world settings. Ideal to use on multiplayer/RP (roleplay) servers to control world advancement.

## Features

NOTE: This mod is under heavy development and is not finished. All pre-releases are intended for those interested in helping test new features until the first official release.

The main feature of this mod is to have an easy way to fully customize the world difficulty and to control the rate at which the world and individual player advances. Set the difficulty of each biome and configure all creatures and items by using the automatic scaling system. You may also define your own custom configuration for individual creatures and items (including other modded content). This mod can dynamically create a game balance that is very different from the Vanilla gameplay experience!

WARNING: If you are using an in-game configuration manager you may be missing out of vital information about the settings! When in doubt please read the config descriptions included in the config file. The information included in the file will not always match the information provided in this readme.

Below are some explanations of features and how to configure them. See more details in the config file. Generate the config file by launching the game once with this mod installed.

### Public & Private Key Management

What is a key and what is controlled by them? In vanilla Valheim there exists a "global key" list that is a bunch of strings shared by all players. Worldly spawns, raids, dreams, and Haldor's items are all controlled by the presence of specific keys. By default this mod will prevent/block public keys from being added to the global list which will prevent game behaviors that rely on the presence of these keys. This lets you control the game progression by choosing when these keys can be added to the game.

This mod also adds a private player key system in which data is saved to the character file. You can use this private key system to tailor game functionality to individuals rather than the vanilla default server-wide public keys. Gameplay will be altered when using private keys: The player that is hosting a loaded chunk will control the worldly spawns, and raids will only spawn on players when appropriate. For example, a player A with no keys that is in a base with a player B with all the boss keys can still get all those raids, but if player A is alone they should not get higher level raids. If player A loads and hosts an area and is later joined by player B, the area should not spawn the higher level monsters that become unlocked with keys. Private keys will be added to any player within a 100 meter range of the hosting player when the action occurs. For example, when a boss dies any player close enough to the chunk-hosting player should also get the private key, but a player online on the other side of the map will not get it.

Taming can be locked by keys when enabled. By default Wolf is locked by the defeated_bonemass key and Lox is locked by the defeated_dragon key. You can override this by using the prefab name of the creature, allowing you to add support for content from other mods. When overriding you must define all the creatures since it will no longer include the defaults. When using private keys make sure the player who can tame the animals is the first to load the area since taming is controlled by the player hosting the chunk. You will still see taming hearts (for now), but if you check the taming status on the animals you will see the percentage no longer increases when blocked.

Guardian Powers and Boss Alter Summoning can be locked too. By default summoning is locked the key given by the previous boss in the natural progression order. You can override this by using the prefab name of the creature the alter summons, allowing you to add support for content from other mods. When overriding you must define all the bosses manually (similar to the taming override). Additionally, these actions have a fun special effect on failure!

Using equipment, crafting, building, and cooking can all be locked with individual settings. The materials are categorized by the biome they are naturally found in. For example, if you lock all these features, if you have not defeated any bosses then you cannot use or craft the antler pickaxe, place a forge, make deer stew, or unlock swamp crypts. These features force new players joining your server to follow the progression of the game in order to advance. This will apply to all game items that use vanilla crafting materials.

#### Important Notice!!

When this mod is installed there will be a key "cleanup" performed for the server and any player who joins the game based off the mod configurations. When using the default settings you can expect all global keys to be cleared when you start up the server, resetting your server's key progress. When using private keys a similar principal applies, depending on your blocked or allowed key list, any keys that are not expected will be removed. All enforced keys will be added to the appropriate list on startup regardless of other settings. If you see your keys resetting unexpectedly make sure to check your mod configuration is allowing the keys you want to exist. Any keys added manually will persist until the server is restarted (for private keys when the player logs back in), to ensure these keys remain after a restart you must check your mod configuration!

#### Configuration Options

* BlockedActionMessage: The message used in-game for certain actions when certain keys are blocked for players.
* BlockAllGlobalKeys: Prevent/block public all keys from being added to the global list, set to false to use vanilla behavior
* AllowedGlobalKeys: Allow only these keys being added to the global list when BlockAllGlobalKeys is true
* BlockedGlobalKeys: Stop only these keys being added to the global list when BlockAllGlobalKeys is false
* EnforcedGlobalKeys: Always add these keys to the global list on startup (regardless of other settings)
* UsePrivateKeys: Use private player keys, rather than global keys for game key checking
* BlockedPrivateKeys: Stop only these keys being added to the player's key list when UsePrivateKeys is true (use this or AllowedPrivateKeys)
* AllowedPrivateKeys: Allow only these keys being added to the player's key list when UsePrivateKeys is true (use this or BlockedPrivateKeys, if the BlockedPrivateKeys has any values it will use that setting)
* EnforcedPrivateKeys: Always add these keys to the player's private list on startup (regardless of other settings)
* UnlockAllHaldorItems: If true bypasses the key check for haldor's items and unlocks everything
* LockTaming: If true you can only tame certain creatures if you have the required key.
* OverrideLockTamingDefaults: Define your own required keys to tame specific creatures or leave blank to use the defaults. Example: Boar, defeated_eikthyr, Wolf, defeated_dragon, Lox, defeated_goblinking
* LockGuardianPower: If true locks the ability to get or use boss powers based on the required key.
* LockBossSummons: If true you can only summon bosses based on the required key.
* OverrideLockBossSummonsDefaults: Define your own required keys to summon bosses or leave blank to use the defaults. Example (also the mod defaults): gd_king, defeated_eikthyr, Bonemass, defeated_gdking, Dragon, defeated_bonemass, GoblinKing, defeated_dragon, SeekerQueen, defeated_goblinking
* LockEquipment: If true you can only equip or use boss items or items made from biome metals/materials if you have the required key
* LockCrafting: If true you can only craft items made from boss items or biome metals/materials if you have the required key
* LockBuilding: If true you can only build pieces made from boss items or biome metals/materials if you have the required key
* LockCooking: If true you can only cook items made from biome materials if you have the required key
* UseBossKeysForSkillLevel and BossKeysSkillPerKey explained under Skill Manager section below

#### Vanilla Public Keys

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
* nomap
* noportals

#### Commands

Due to the changes this mod makes the vanilla "setkey" command will not function as expected in most cases. There is an added command "setglobalkey" that will work in it's place. For private keys there are 4 new commands added that work similar to the vanilla public key commands: setprivatekey, removeprivatekey, resetprivatekeys, listprivatekeys. For example, you can set your local player's key with "setprivatekey defeated_eikthyr", or any online player with "setprivatekey defeated_eikthyr PlayerName".

The server also tracks the player's keys in each game session. This list is cleared on a server restart and data for each player will only be available once the player reconnects. The command is "listserverkeys", if you are hosting the data will be available in the console window. If you have a dedicated server you can send this command to the server from the client and the data will be printed to the bepinex/logoutput.log file.

Don't know how to use commands? Dedicated servers do not allow for use of commands, but there are mods that can enable them (like Server devcommands by JereKuusela). All of these commands are considered "cheats" except the "listprivatekeys" command. To use cheats you must enable them with the "devcommands" command, you may have to be an admin for them to work.

#### Possible Future Additions

* Ability to lock items or crafting to boss completion
* More configuration options to toggle public or private key usage for different game features
* Requests welcome! See the bottom for my discord link

### Skill Manager

Have more control over Skill loss and gain. When using skill capping any skills that are already above the maximum skill cap will remain "frozen" and will not gain, but can still be lowered on death. Console cheats will still work as intended. Other mods that change how skill gain and loss functions may cause unexpected behaviors. Turn off this feature if using another mod for skill management if you see mod conflicts.

#### Configuration Options

* EnableSkillManager must be set to True to enable these features.
* AllowSkillDrain: Set to False to turn off all skill loss on death.
* UseAbsoluteSkillDrain: Set to True to use an absolute number (AbsoluteSkillDrain) for skill loss. (Vanilla uses a percentage for skill loss, so you will lose more skill the higher the skill is)
* CompareAndSelectDrain: Set to true to use the minimum or maximum value between the vanilla skill loss and the absolute skill loss. Set CompareUseMinimumDrain to False to use the maximum of these two values as the skill loss.
* OverrideMaximumSkillLevel: True to set a server wide skill level ceiling for gaining skill to the value of MaximumSkillLevel. For example, if this value is 50 then you will not gain skill once you reach level 50+ in that skill.
* OverrideMinimumSkillLevel: True to set a server wide skill level floor for skill loss to the value of MinimumSkillLevel. For example, if this value is 10 then you will not lose skill on death until you reach level 10 in that skill, so your skills will not drop below 10.

Under the Keys category there are more configuration options for skills. This feature will only work if you DO NOT override the minimum and/or maximum skill levels as described above. Overridden values will take precedence.

* UseBossKeysForSkillLevel: Set this to true to use a more dynamic skill control dependant on boss completion. Skill minimum will start at 0 and increase by BossKeysSkillPerKey for each boss defeated. Skill maximum will be capped at [ 100 - (number of bosses: 6) * (BossKeysSkillPerKey: 10) = 40 ] with the current game state. For example, if you defeat one boss then your skill minimum for loss will be raised to 10, and your skill maximum will be raised to 50.
* BossKeysSkillPerKey: Amount used in calculation above.

### World Scaling

The world is scaled according to the "natural" vanilla game progression by default. To see this mod's default classification of vanilla creatures and items you can view the code on Github. The scaling is applied after logging into a local world or server. You will have to have players log out and back in after changing these configurations for them to take effect.

Under the general section of the config file there is a GenerateGameDataFiles setting. When true this will create files of game data for viewing only which can help if you want to use the override files (explained below). These files can be overwritten with scaled data if you launch multiple games in one session. To ensure you have the vanilla data just launch the game and log into a world once, then log out. Move the two folders in the config path that get created to another location on your computer to save the files. Once you have the files set GenerateGameDataFiles back to false to keep your mod manager configs clean.

#### Configuration Options

* EnableAutoScaling must be set to True to enable these features.
* AutoScaleType: Vanilla, Linear, Exponential, or Custom. If set to Vanilla these auto-scaling features will not be enabled!
* AutoScaleFactor: Change the biome scaling factor.

If you do not want to rescale the whole game and just want to change just a few things (or a lot of things, you do you) you can use the "Custom" scaling type to ignore the mod defaults and just scale things in your yaml override(s). If any information is missing from your yaml configuration that is needed for scaling the mod will fallback to using the default values for the missing fields. Custom scaling will use the linear scaling methods whenever applicable.

Linear scaling by default is a 75% growth (0.75). This means your 1st biome (Black Forest, Meadows is 0th) will have a scaling factor of 1.75, and 7th will be 6.25 for calculations. This setting will make a pretty even difficulty progression.

To use Exponential scaling PLEASE READ THIS PART: Given that there are by default only 8 biome difficulties to scale, the maximum scaling value you can input is roughly 21 without blatantly breaking the code generating the values (If using 12 custom biome difficulties this number is about 6). However, 21 is a much, much bigger number than you could ever want. Recommended values for exponential scaling are in a range of 0.25 - 1. For example, an exponential scaling of 0.75 will set the 1st biome to 1.75x harder, 7th biome to be about 50x harder than the base biome. This setting will make the first few biomes closer together in difficulty than the later biomes.

#### Creature Scaling

The total damage a creature can do will be scaled and then distributed to individual damage types for each attack; scaling will maintain the ratio for creatures with more than one attack (some attacks are stronger, some are weaker, scaling maintains this). All values for chop and pickaxe damage are ignored and will retain their original values without affecting scaling. To override this mod's default configurations there is a file called "WAP.CreatureOverrides.yaml" in which you can add customizations. Currently, all players must have the same file locally since this feature does not sync yet. Directions on how to use the override feature are included in that file.

##### Configuration Options

The list for health and damage configs is in the format (for difficulty spread): Harmless, Novice, Average, Intermediate, Expert, Boss

* AutoScaleCreatures: Set to true to scale creature health and damage.
* AutoScaleCreaturesHealth: Leave blank to use default values: 5, 10, 30, 50, 200, 500; to override enter a comma-separated list of 6 integers.
* AutoScaleCreaturesDamage: Leave blank to use default values: 0, 5, 10, 12, 15, 20; to override enter a comma-separated list of 6 integers.

#### Item Scaling

Vanilla items are grouped by custom types and are assigned the biome in which they naturally can be crafted. To override this mod's default configurations there is a file called "WAP.ItemOverrides.yaml" in which you can add customizations. Currently, all players must have the same file locally since this feature does not sync yet. Directions on how to use the override feature are included in that file.

##### Configuration Options

* AutoScaleItems: Set to true to scale player items.

#### Custom Data Definitions

| Biomes | Difficulty | Item Types |
| ------ | ---------- | ---------- |
| Meadow = 0 | Vanilla = 0 | None = 0 |
| BlackForest = 1 | Harmless = 1 | Shield = 1 |
| Swamp = 2 | Novice = 2 | Helmet = 2 |
| Mountain = 3 | Average = 3 | Chest = 3 |
| Plain = 4 | Intermediate = 4 | Legs = 4 |
| AshLand = 5 | Expert = 5 | Shoulder = 5 |
| DeepNorth = 6 | Boss = 6 | Utility = 6 |
| Ocean = 7 | | Tool = 7 |
| Mistland = 8 | | PickAxe = 8 |
| | | Axe = 9 |
| | | Bow = 10 |
| | | Ammo = 11 |
| | | Sword = 20 |
| | | Knife = 21 |
| | | Mace = 22 |
| | | Sledge = 23 |
| | | Atgeir = 25 |
| | | Battleaxe = 26 |
| | | Primative = 27 |
| | | Spear = 28 |
| | | TowerShield = 29 |
| | | BucklerShield = 30 |
| | | PrimativeArmor = 31 |
| | | Bolt = 32 |
| | | Crossbow = 33 |
| | | HelmetRobe = 34 |
| | | ChestRobe = 35 |
| | | LegsRobe = 36 |

### Other Features

* ServerSync included

## Developers (Always In Progress)

Want to define customizations of your own mod with these features so your users don't have to? If there is something you need let me know and I can add support for your mod!

### Custom Biomes

Planned features include: Define your own biomes and add them to the scaling system by using any int value as a key that is not used. This mod's default values can be overridden to set custom scaling after initialization.

Examples (Will update this for first official release):

* To add a Biome '10' that is one biome harder than Plains (4th hardest by default, Meadows is 0th) use: AddBiome(10, 5)
* To add a Biome '10' that is 250% harder than the baseline use: AddCustomBiome(10, 2.5)
* To override Meadow's difficulty after it has been initialized: AddBiome(0, 8, true) or AddCustomBiome(0, 1.3, true)

## Changelog

### 0.0.24

* Added ability to lock equipping and using boss items or items made from biome metals/materials (defaults to on)
* Similarly added ability to lock crafting (defaults to on)
* Similarly added ability to lock building (defaults to on)
* Similarly added ability to lock cooking (defaults to on)
* Bug fix for blocking taming not always working correctly
* Added Flesh Rippers to auto-scaling with new item type "Fist"
* Improved key commands so that they work on any player online rather than only players in your same chunk
* Improved key commands to accept player names with spaces
* Now compatible with Multiplayer Tweaks "SkillLossOnPVPDeath" option

### 0.0.23

* Added ability to print data to the server log file with the existing listserverkeys command
* Changed the "listprivatekeys" from a cheat to a normal command so all players can use it

### 0.0.22

* Added ability to enforce private and global keys with new config options (useful for "nomap" and "noportal" modes)
* Fix for global key cleanup happening before the data was available, should be working now
* Small qol update for the scaling system: Added new "Custom" scaling type for ignoring the mod defaults
* Removed the AutoScaleCreaturesIgnoreDefaults and AutoScaleItemsIgnoreDefaults configs (use AutoScaleType = "Custom")
* Fixed the scaling resetting logic such that custom values will now reset properly between different servers in one game session
* Fixed items possibly being scaled before recording vanilla data, causing them to scale incorrectly
* Fixed the logic behind getting the maximum damage a creature can do, caused creature attacks/items to scale incorrectly
* Fixed unnecessary scaling being reapplied on player death
* Added missing items to the GenerateGameDataFiles feature
* Notes on current limitations:
  * Some creatures share the same attacks and items, the order in which the creatures are scaled affects the final result
  * With creature scaling turned on, if an item is used by a creature and you turn off item scaling the item will still be scaled
  * Creature armor and shields are not scaled, but will be integrated eventually
  * I am working on a smarter scaling design, but may take some time to get right (looking to do that for version 2)

### 0.0.21

* Added ability to lock taming creatures by keys.
* Added ability to lock guardian powers by keys.
* Added ability to lock Alter Summoning by keys.
* Fixed a bug where the key configurations were not updating correctly internally unless you used certain settings.

### 0.0.20

* Fixed an issue with private key cleanup that would crash the game on player spawn.

### 0.0.19

* Depreciated use of the additional file for saving private player keys, saves to the main player file now. If using the previous version you can expect your keys to update to the new system and the file will be automatically deleted. Support for the upgrade will be removed in the future.
* Added ability to use private keys (rather than global keys) for game behaviors (i.e worldy spawns and raid checks)
* Added new config UsePrivateKeys under General, removed UsePrivateBossKeysForSkillLevel (now included under the new setting)
* Added new configs BlockedPrivateKeys and AllowedPrivateKeys, similar to the public key settings but for the private ones
* Added ability to manage other player's private keys with commands (if the player is online)
* Added ability to unlock all of haldor's items with new config UnlockAllHaldorItems
* Increased Key Manager cache timer to 10 seconds
* Small optimizations to Skill Manager, increased cache timer to 10 seconds
* Fix for some creature attacks not being scaled (whoops): will now track all random weapons and items from random sets
* Decreased base Bow damage from 22 to 18
* Thank you all for your patience!!!

### 0.0.18

* Added (most) Mistlands additions to keys and scaling
* Added new scaling categories for "robes": reclassified troll, root, and fenris armors to the new types
* Added ability to override creature and item data using the specified yaml files
* New configuration options to ignore auto-scaling defaults, allows you to change only what you specify in the yaml files
* Renamed AutoScale config to EnableAutoScaling, you have to edit this value to use auto scaling
* Upgraded ServerSync to version 1.13 for game patch 0.212.7

### 0.0.17

* Bug fix for auto-scaling where a live configuration update would override stored vanilla data
* Removed chop and pickaxe damage from auto-scaling (I didn't like how it changed the game)

### 0.0.16

* Bug fix for auto-scaling item/creature damages calculating the values incorrectly.
* Bug fix from last update for creature attacks not configuring
* Added auto-scaling for item upgrades

### 0.0.15

* Upgraded ServerSync to V1.11: Crossplay compatibility upgrade for config sync.
* ServerSync patch for game patch 0.211.11.
* Major rework for testing purposes, added more unit tests, general code cleanup.
* Fixed a few bugs with autoscaling values calculated incorrectly.
* Added ability to restore vanilla game data without a game restart for autoscaling (in testing).
* Optimization for Key and Skills managers caching.
* Fixed biome ordering for Mistland, Ashland, DeepNorth.


See all patch notes on Github.

## Contributing

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
