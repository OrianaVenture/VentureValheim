# World Scaling

Created by [OrianaVentureMod@gmail.com](https://github.com/OrianaVenture/VentureValheim).

## Introduction

Rebalance creatures and items with a configurable auto-scaling system. Ability to add your own custom overrides.

## Features

Set the difficulty of each biome and configure all creatures and items by using the automatic scaling system. You may also define your own custom configuration for individual creatures and items (including other modded content). This mod can dynamically create a game balance that is very different from the Vanilla gameplay experience!

It is highly recommend to use a mod that shows the creature health in-game so you and your players can see the differences when scaling creatures.

Below are some explanations of features and how to configure them. See more details in the config file. Generate the config file by launching the game once with this mod installed.

<details open>
<summary>Expand/Collapse Features</summary>

### World Scaling

The world is scaled according to the "natural" vanilla game progression by default. To see this mod's default classification of vanilla creatures and items you can view the code on Github. The scaling is applied after logging into a local world or server. You will have to have players log out and back in after changing these configurations for them to take effect.

**When adding to an existing world or changing the settings mid-game the creatures already spawned in the world may retain old values. To ensure your settings are working as intended spawn in new creatures to test.

#### Configuration Options

* AutoScaleType: Vanilla, Linear, Exponential, Logarithmic, or Custom. If set to Vanilla these auto-scaling features will not be enabled!
* AutoScaleFactor: Change the biome scaling factor.
* AutoScaleIgnoreOverrides: When true ignores the overrides specified in the yaml files. (yaml files explained below)

#### Custom Scaling

If you do not want to rescale the whole game and just want to change just a few things (or a lot of things, you do you) you can use the "Custom" scaling type to ignore the mod defaults and just scale things in your yaml override(s). If any information is missing from your yaml configuration that is needed for scaling the mod will fallback to using the default values for the missing fields. Custom scaling will use the linear scaling methods whenever applicable.

#### Linear Scaling

This setting will make an even difficulty progression increase. Smaller growth will allow player to skip gear grinding more easily, where greater growth will force players to craft every tier of weapons to move forward effectively.

Linear scaling by default is a 75% growth (**0.75**). This means your 1st biome (Black Forest, Meadows is 0th) will be about 1.75 times harder, and the 7th (Ashlands) will be about 6 times harder than meadows.

#### Exponential Scaling

This setting will make the later biomes progressively much greater in difficulty. Earlier biomes will be closer together in difficulty making it easier to complete Black Forest or Swamp with lower level gear. Can be used to help skip the gear grind at the *beginning* of the game by keeping gear relevant longer.

An exponential scaling of **0.3** will make the 1st biome (Black Forest, Meadows is 0th) about 1.3 times harder, and the 7th (Ashlands) will be 6 times harder than meadows.

#### Logarithmic Scaling

This setting will make the later biomes closer together in difficulty. Earlier biomes will be further apart in difficulty making it harder to complete Black Forest or Swamp with lower level gear. Can be used to help skip the gear grind at the *end* of the game by keeping gear relevant longer.

The formula for logarithmic scaling is: 1 + (log(biome + 1) / log(factor)). The factor for this scaling must be a number greater than 1, as it will become the logarithmic base.

An logarithmic scaling of **1.5** will make the 1st biome (Black Forest, Meadows is 0th) about 2.7 times harder, and the 7th (Ashlands) will be about 6 times harder than meadows.

#### Recommend Settings

| Desired Playstyle                  | Linear  | Exponential | Logarithmic |
| ---------------------------------- | ------- | ----------- | ----------- |
| Biomes close in difficulty         | 0.5     | 0.24        | 1.8         |
|                ...                 | 0.75    | 0.3         | 1.5         |
|                ...                 | 1.0     | 0.35        | 1.35        |
| Cautious of higher level biomes    | 2.0     | 0.47        | 1.16        |
|                ...                 | 3.0     | 0.56        | 1.1         |
| Killer higher level biomes         | 4.0     | 0.62        | 1.08        |
|                ...                 | 5.0     | 0.67        | 1.06        |

* Note: This is a work in progress. If you test these different settings and have feedback please send me a message! See contributing at the bottom.

### Creature Scaling

The total damage a creature can do will be scaled and then distributed to individual damage types for each attack; scaling will maintain the ratio for creatures with more than one attack (some attacks are stronger, some are weaker, scaling maintains this). All values for chop and pickaxe damage are ignored and will retain their original values without affecting scaling. To override this mod's default configurations there is a file called "VWS.CreatureOverrides.yaml" in which you can add customizations.

** Currently, all players must have the same file locally since this feature does not sync yet. Directions on how to use the override feature are included in that file.

#### Configuration Options

The list for health and damage configs is in the format (for difficulty spread): Harmless, Novice, Average, Intermediate, Expert, Boss

* AutoScaleCreatures: Set to true to scale creature health and damage.
* AutoScaleCreaturesHealth: Leave blank to use default values: 5, 10, 30, 50, 200, 500; to override enter a comma-separated list of 6 integers.
* AutoScaleCreaturesDamage: Leave blank to use default values: 0, 5, 10, 12, 15, 20; to override enter a comma-separated list of 6 integers.

### Item Scaling

Vanilla items are grouped by custom types and are assigned the biome in which they normally can be crafted. To override this mod's default configurations there is a file called "VWS.ItemOverrides.yaml" in which you can add customizations.

** Currently, all players must have the same file locally since this feature does not sync yet. Directions on how to use the override feature are included in that file.

#### Configuration Options

* AutoScaleItems: Set to true to scale player items.

### Customization Help

Under the general section of the config file there is a GenerateGameDataFiles setting. When true this will create files of game data (for viewing only) which can help if you want to use the override files. Move the two folders in the config path that get created to another location on your computer to save the files permanently. These files can be overwritten with scaled data if you launch multiple games in one session. To get the vanilla data load a world once then quit the game. To get scaled data load a world, logout, and load it again, then quit.

Once you have the files set GenerateGameDataFiles back to false to keep your mod manager configs clean.

Another way to view the things this mod is changing you can turn on debugging logs. To do this you must change your BepInEx.cfg settings and include "Debug" under the Logging.Disk - LogLevels setting. This will allow the mod to print out debugging information I use myself to check what the mod is doing.

#### Custom Data Definitions

| Biomes          | Difficulty | 
| --------------- | ---------- |
| Meadow = 0      | Vanilla = 0 |
| BlackForest = 1 | Harmless = 1 |
| Swamp = 2       | Novice = 2 |
| Mountain = 3    | Average = 3 |
| Plain = 4       | Intermediate = 4 |
| AshLand = 5     | Expert = 5 |
| DeepNorth = 6   | Boss = 6 |
| Ocean = 7       | |
| Mistland = 8    | |

| Armor Types         | Shield Types       | Weapon Types      | Misc Types      |
| ------------------- | ------------------ | ----------------- | --------------- |
| Helmet = 2          | Shield = 1         | PickAxe = 8       | None = 0        |
| Chest = 3           | TowerShield = 29   | Axe = 9           | Utility = 6     |
| Legs = 4            | BucklerShield = 30 | Bow = 10          | Tool = 7        |
| Shoulder = 5        | MagicShield = 54   | Ammo = 11         | TurretBolt = 38 |
| PrimativeArmor = 31 |                    | Sword = 20        |  |
| HelmetRobe = 34     |                    | Knife = 21        |  |
| ChestRobe = 35      |                    | Mace = 22         |  |
| LegsRobe = 36       |                    | Sledge = 23       |  |
| HelmetMedium = 49   |                    | Atgeir = 25       |  |
| ChestMedium = 51    |                    | Battleaxe = 26    |  |
| LegsMedium = 52     |                    | Primative = 27    |  |
|                     |                    | Spear = 28        |  |
|                     |                    | Bolt = 32         |  |
|                     |                    | Crossbow = 33     |  |
|                     |                    | Fist = 37         |  |
|                     |                    | GemAxe = 39       |  |
|                     |                    | GemBow = 40       |  |
|                     |                    | GemSword = 41     |  |
|                     |                    | GemKnife = 42     |  |
|                     |                    | GemMace = 43      |  |
|                     |                    | GemSledge = 44    |  |
|                     |                    | GemAtgeir = 45    |  |
|                     |                    | GemBattleaxe = 46 |  |
|                     |                    | GemSpear = 47     |  |
|                     |                    | GemCrossbow = 48  |  |
|                     |                    | StaffRapid = 52   |  |
|                     |                    | StaffSlow = 53    |  |

</details>

## How to port over from World Advancement & Progression

If you were previously using this feature in my other mod the configs are very similar with some renaming. Manually update the config file after it auto-generates for you upon launching the game once. If you were using the yaml files all you have to do is rename them with the new prefix (from the old WAP): VWS.

## Installation

This mod needs to be on all clients to work properly. Config Syncing is included with Jotunn. Install on the server to enforce the same mod configuration for all players. Live changes to the configurations will not take effect until the player relogs into the world/server.

## Changelog

### 0.3.0

* Hildir, Ashlands, and Magic Weapon default configuration additions.
* New item types available.
* Added Logarithmic scaling support.
* Tweaked some default base values and classifications:
  * Increased base damage for creatures.
  * Greyling, Neck, TentaRoot, Blob, Deathsquito, SeekerBrood reduced one classification level.
  * SeekerBrute, all Dverger increased one classification level.
  * Ammo and Bolt increased by 2.
  * Crossbow reduced by 5.
* Improved rounding in calculations, items should be less likely to end up with 0 upgrade values.

### 0.2.0

* Added Jotunn library as new dependency for config syncing, you now must also install Jotunn for this mod to work.

### 0.1.0

* Pulled out from World Advancement & Progression version 0.0.28.
* Some config renaming, no major feature changes.

## Contributing

This mod started out as a bit of a thought experiment and is not well polished. There will be a major rework of the mod when the game exits early access and is completed. Until then please experiment, take notes of what you don't like and what you wish could be done better. All feedback will be very valuable for this mod, especially for tweaking the default balancing and assignment of the item and creature strengths.

All issues can be reported on the project Github. To report issues please be as specific as possible and provide the following:

1. Version of this mod you are using.
2. List of the other mods being used.

All feedback, ideas, and requests are welcome! You can message me at my discord [Venture Gaming](https://discord.gg/tAd5hapt88).
